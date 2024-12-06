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

        static readonly int[] _levels = [100, 100, 100, 100];

        public KilnDataGrid()
        {
            InitializeComponent();

            _data = new ObservableCollection<Ware>();
            dataGrid.DataContext = _data;
        }


        private void GenerateSolve(object sender, RoutedEventArgs e)
        {
            GenerateSolution(false);
        }
        
        private void GenerateOptimisedSolve(object sender, RoutedEventArgs e)
        {
            GenerateSolution(true);
        }
        
        private void GenerateSolution(bool optimise)
        {
            try
            {
                if (_data.Count == 0)
                {
                    MessageBox.Show("No Wares added to list");
                    return;
                }

                var input = _data.ToArray();
                
                var solution = SolveGenerator.GenerateSolution(input, _levels, optimise);

                if (solution is null)
                {
                    MessageBox.Show("No Solution Found");
                    return;
                }

                var resultWindow = new KilnSolutionWindow(input, solution);
                resultWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Whoops, something went wrong. Tell Paul :(\n\nDetails:\n{ex}");
            }
        }
    }
}