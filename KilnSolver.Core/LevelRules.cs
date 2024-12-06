namespace KilnSolver.Core;


public class LevelRules
{
    private int _levelCount;

    public LevelRules(int levelCount)
    {
        _levelCount = levelCount;
    }

    public bool[] AnyLevel() => Enumerable.Range(0, _levelCount).Select(_ => true).ToArray();

    public bool[] TopOnly(int topLevels = 1)
    {
        if (topLevels > _levelCount)
            throw new ArgumentOutOfRangeException(nameof(topLevels), "Must be < " + _levelCount);

        return [..Enumerable.Range(0, topLevels).Select(_ => true), ..Enumerable.Range(0, _levelCount - topLevels).Select(_ => false)
        ];
    }

    public bool[] BottomOnly(int bottomLevels = 1)
    {
        if (bottomLevels > _levelCount)
            throw new ArgumentOutOfRangeException(nameof(bottomLevels), "Must be < " + _levelCount);

        return [..Enumerable.Range(0, _levelCount - bottomLevels).Select(_ => false), ..Enumerable.Range(0, bottomLevels).Select(_ => true)
        ];
    }
}