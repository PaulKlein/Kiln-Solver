// See https://aka.ms/new-console-template for more information
using KilnSolver.Core;

// Celedons have to be on the top 2 layers
// Flux has to be on the bottom two layers
// Clay types are important. White clay = anywhere. Speckled cream has to go on the top two levels. Brown on the top two levels.

int[] levelCapacities = [
    100,
    100,
    100,
    100
];

Ware[] wares = [
    new Ware {
        Name = "Sheep",
        AllowedLevel = AllowedLevel.TopTwo,
        ItemCount = 5,
        Size = 2,
    },
    new Ware {
        Name = "Tree",
        AllowedLevel = AllowedLevel.TopMost,
        ItemCount = 4,
        Size = 3
    },
    new Ware {
        Name = "Low Shelf Trees",
        AllowedLevel = AllowedLevel.BottomMost,
        ItemCount = 8,
        Size = 2,
    },
    new Ware {
        Name = "Tiny Cheese",
        AllowedLevel = AllowedLevel.All,
        ItemCount = 1,
        Size = 1,
    },
];

bool optimise = true;

Console.WriteLine("Solving to fit the following items: ");
foreach (var ware in wares)
{
    Console.WriteLine($"{ware.Name} x{ware.ItemCount} [{ware.AllowedLevel}]");
}

Console.WriteLine($"Total Items: {wares.Sum(w => w.ItemCount)}");

Console.WriteLine();
Console.WriteLine();

Console.Write(optimise ? "Optimising..." : "Solving...");

var solution = SolveGenerator.GenerateSolution(wares, levelCapacities, optimise);
Console.WriteLine("Done");

if (solution is null)
{
    Console.WriteLine("No solution exists :(");
    return;
}

Console.WriteLine("*** Top of Kiln");
for (var i = 0; i < solution.Length; i++)
{
    Console.WriteLine("Level {0}, {2}% Fill - [{1}]", i, string.Join(", ", solution[i].WareCounts.Select(w => $"{w.Ware.Name} x{w.Count}")), solution[i].WareCounts.Sum(w => w.Ware.Size * w.Count));
}
Console.WriteLine("*** Bottom of Kiln");
Console.WriteLine($"Total Items: {solution.Sum(l => l.WareCounts.Sum(w => w.Count))}");