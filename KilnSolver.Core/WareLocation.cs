using ZenLib;

namespace KilnSolver.Core
{
    [ZenObject]
    public partial class WareLocation
    {
        public required int WareId { get; set; }
        public required int Level { get; set; }
    }
}