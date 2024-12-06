using System.Diagnostics;
using ZenLib;
using ZenLib.ModelChecking;

namespace KilnSolver.Core;

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

        int GetWareGridIndex(int levelId, int wareId) => levelId * levels.Length + wareId;

        // 2d array of counts of each ware on each level
        var wareGrid = Zen.Symbolic<CMap<int,int>>();

        var constraints = new List<Zen<bool>>();
            
        List<Zen<int>> fillRates = new List<Zen<int>>();
            
        // Ensure each level doesn't exceed max size
        for (var levelId = 0; levelId < levels.Length; levelId++)
        {
            List<Zen<int>> waresInLevel = new List<Zen<int>>();
            for (var wareId = 0; wareId < wares.Length; wareId++)
            {
                var wareGridIndex = GetWareGridIndex(levelId, wareId);

                var wareGridItem = wareGrid.Get(wareGridIndex);
                    
                var wareSizeOnLevel = wareGridItem * wares[wareId].Size;
                waresInLevel.Add(wareSizeOnLevel);
                constraints.Add( Zen.And(wareGridItem >= 0, wareGridItem <= wares[wareId].ItemCount) );
            }

            var fillRateOfLevel = waresInLevel.Aggregate(Zen.Plus);
                
            constraints.Add( fillRateOfLevel < 100 );
            fillRates.Add(fillRateOfLevel);
        }
            
        // Ensure each ware has the right number of items across all levels
        for (var wareId = 0; wareId < wares.Length; wareId++)
        {
            List<Zen<int>> wareCounts = new List<Zen<int>>();
            var ware = wares[wareId];
                
            for (var levelId = 0; levelId < levels.Length; levelId++)
            {
                var allowedOnLevel = ware.AllowedLevel switch
                {
                    AllowedLevel.All => true,
                    AllowedLevel.TopMost => levelId == 0,
                    AllowedLevel.TopTwo => levelId <= 1,
                    AllowedLevel.BottomMost => levelId == levels.Length - 1,
                    AllowedLevel.BottomTwo => levelId >= levels.Length - 2,
                    _ => throw new ArgumentOutOfRangeException()
                };
                        
                var wareGridIndex = GetWareGridIndex(levelId, wareId);

                var wareCountOnLevel = wareGrid.Get(wareGridIndex);
                wareCounts.Add(wareCountOnLevel);
                    
                if (!allowedOnLevel)
                    constraints.Add( wareCountOnLevel == 0 );
            }
                
            constraints.Add( wareCounts.Aggregate(Zen.Plus) == ware.ItemCount );
        }
            

        var startTimeStamp = Stopwatch.GetTimestamp();
        ZenSolution solution;
        if (optimise)
        {
            var minUsage = fillRates.Aggregate(Zen.Min);
            var maxUsage = fillRates.Aggregate(Zen.Max);
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

        var wareGridSolution = solution.Get(wareGrid);

        var solveLevelInfos = new SolveLevelInfo[levels.Length];

        for (var levelId = 0; levelId < levels.Length; levelId++)
        {
            var waresOnLevel = new List<SolveWareCount>();
            for (var wareId = 0; wareId < wares.Length; wareId++)
            {
                var wareGridIndex = GetWareGridIndex(levelId, wareId);
                var wareCount = wareGridSolution.Get(wareGridIndex);
                if (wareCount != 0)
                    waresOnLevel.Add(new SolveWareCount(wares[wareId], wareCount));
            }
            solveLevelInfos[levelId] = new SolveLevelInfo(waresOnLevel.ToArray());
                
        }

        return solveLevelInfos;
    }
    
    public record SolveLevelInfo(SolveWareCount[] WareCounts);
    public record SolveWareCount(Ware Ware, int Count);
}