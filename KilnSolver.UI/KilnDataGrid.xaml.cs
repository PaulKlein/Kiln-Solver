using KilnSolver.Core;
using ModernWpf.Controls;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace KilnSolver.UI
{
    public partial class KilnDataGrid : UserControl
    {
        readonly ObservableCollection<Ware> _data;

        readonly static int[] _levels = [100, 100, 100, 100];

        public KilnDataGrid()
        {
            InitializeComponent();

            _data = new ObservableCollection<Ware>();
            dataGrid.DataContext = _data;
        }


        private void GenerateSolve(object sender, RoutedEventArgs e)
        {
            var solution = SolveGenerator.GenerateSolution(_data.ToArray(), _levels);

            if (solution is null)
            {
                MessageBox.Show("No Solution Found");
                return;
            }

            var resultBuilder = new StringBuilder();

            resultBuilder.AppendLine("*** Top of Kiln ***");
            for (var i = 0; i < solution.Length; i++)
            {
                resultBuilder.AppendFormat("Level {0} -[{1}]\n", i, string.Join(", ", solution[i].WareCounts.Select(w => $"{w.WareName} x{w.Count}")));
            }
            resultBuilder.AppendLine("*** Bottom of Kiln ***");
            MessageBox.Show(resultBuilder.ToString());
        }
    }
}