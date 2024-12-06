using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;
using ZenLib;
using ZenLib.ModelChecking;
using ZenLib.Solver;

namespace KilnSolver.Core
{
    public static class SolveGenerator
    {
        public static SolveLevelInfo[]? GenerateSolution(Ware[] wares, int[] levels, bool optimise = false)
        {
            if (levels.Length == 0)
            {
                throw new InvalidOperationException("Levels are required!");
            }

            if (wares.Length == 0)
            {
                throw new InvalidOperationException("Wares are required!");
            }

            var placements = Zen.Symbolic<FSeq<WareLocation>>(depth: wares.Sum(w => w.ItemCount));

            var constraints = new List<Zen<bool>> {
                placements.Select(p => p.GetWareId()).All(id => Zen.And(id >= 0, id < wares.Length)),
                placements.Select(p => p.GetLevel()).All(l => Zen.And(l >= 0, l < levels.Length))
            };

            var levelRules = new LevelRules(levels.Length);


            for (var wareId = 0; wareId < wares.Length; wareId++)
            {
                var ware = wares[wareId];

                var wareCount = placements.Where(p => p.GetWareId() == wareId).Length();

                constraints.Add(wareCount == (BigInteger)ware.ItemCount);


                var allowedLevels = ware.AllowedLevel switch
                {
                    AllowedLevel.TopMost => levelRules.TopOnly(1),
                    AllowedLevel.TopTwo => levelRules.TopOnly(2),
                    AllowedLevel.BottomTwo => levelRules.BottomOnly(2),
                    AllowedLevel.BottomMost => levelRules.BottomOnly(1),
                    AllowedLevel.All => levelRules.AnyLevel(),
                    _ => throw new ArgumentOutOfRangeException()
                };

                //Console.WriteLine(ware.Name);
                //Console.WriteLine(string.Join(",", allowedLevels.Select(l => l.ToString())));

                for (var i = 0; i < allowedLevels.Length; i++)
                {
                    var level = i;
                    if (!allowedLevels[i])
                    {
                        constraints.Add(Zen.Not(placements.Any(w => Zen.And(w.GetWareId() == wareId, w.GetLevel() == level))));
                    }
                }
            }

            var levelUsages = new List<Zen<int>>();

            for (var levelId = 0; levelId < levels.Length; levelId++)
            {
                var waresOnLevel = placements.Where(p => p.GetLevel() == levelId).Select(p => p.GetWareId());

                var levelUsage = waresOnLevel.Fold(Zen.Constant(0), (val, acc) => acc + GetSize(val));
                levelUsages.Add(levelUsage);
                constraints.Add(levelUsage <= levels[levelId]);
            }

            var startTimeStamp = Stopwatch.GetTimestamp();
            ZenSolution solution;
            if (optimise)
            {
                var minUsage = levelUsages.Aggregate(Zen.Min);
                var maxUsage = levelUsages.Aggregate(Zen.Max);
                solution = Zen.Minimize( maxUsage - minUsage, Zen.And(constraints));
            }
            else
            {
                solution = Zen.And(constraints).Solve();
            }

            var elapsed = Stopwatch.GetElapsedTime(startTimeStamp);

            if (!solution.IsSatisfiable())
            {
                return null;
            }

            var placementsSolution = solution.Get(placements).ToList();

            var solveLevelInfos = new SolveLevelInfo[levels.Length];

            for (var i = 0; i < levels.Length; i++)
            {
                var waresOnLevel = placementsSolution
                .Where(w => w.Level == i)
                .GroupBy(p => p.WareId)
                .Select(p => new SolveWareCount(wares[p.Key], p.Count()))
                .ToArray();

                solveLevelInfos[i] = new(waresOnLevel);

            }

            return solveLevelInfos;

            Zen<int> GetSize(Zen<int> wareId)
            {
                return Zen.Cases(Zen.Constant(int.MaxValue),
                    wares.Select((ware, index) => (wareId == index, Zen.Constant(ware.Size))).ToArray()
                );
            }
        }
    }

    public record SolveLevelInfo(SolveWareCount[] WareCounts);
    public record SolveWareCount(Ware Ware, int Count);
}