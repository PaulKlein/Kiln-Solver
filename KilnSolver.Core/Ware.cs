namespace KilnSolver.Core
{
    public partial class Ware
    {
        public required string Name { get; set; }
        public AllowedLevel AllowedLevel { get; set; } = AllowedLevel.All;
        public required int Size { get; set; }
        public int ItemCount { get; set; }
    }
}