using KilnSolver.Core;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace KilnSolver.UI
{
    public partial class KilnDataGrid : UserControl
    {
        readonly ObservableCollection<Ware> _data = [];

        static readonly int[] _levels = [100, 100, 100, 100];

        const string _autoSaveFile = "AutoSave.json";

        public KilnDataGrid()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            try
            {
                if (File.Exists(_autoSaveFile))
                {
                    LoadWares(_autoSaveFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load auto-save file\n\nDetails:{ex}");
            }

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

        void SaveWares(string fileName)
        {
            using var writer = File.Create(fileName);
            JsonSerializer.Serialize(writer, _data.ToArray());
        }

        void LoadWares(string fileName)
        {
            using var reader = File.OpenRead(fileName);
            var wares = JsonSerializer.Deserialize<Ware[]>(reader);
            if (wares is { Length: > 0 })
            {
                _data.Clear();
                foreach (var ware in wares)
                    _data.Add(ware);
            }
        }

        void Clear(object sender, RoutedEventArgs e)
        {
            if (_data.Count == 0)
                return;
            
            const string confirmation = "Are you sure you want to clear all wares?";
            if (ConfirmOperation(confirmation))
                _data.Clear();
        }

        bool ConfirmOperation(string message)
        {
            return MessageBox.Show(message, "Confirm", MessageBoxButton.YesNo) ==
                   MessageBoxResult.Yes;
        }

        void Save(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Json files (*.json)|*.json",
                OverwritePrompt = true
            };

            if (dialog.ShowDialog() != true)
                return;
            
            SaveWares(dialog.FileName);
        }

        void Load(object sender, RoutedEventArgs e)
        {
            if (_data.Count != 0 && !ConfirmOperation("You have existing wares loaded. Loading a new file will overwrite them. Do you want to continue?"))
            {
                return;
            }
            var dialog = new OpenFileDialog()
            {
                Filter = "Json files (*.json)|*.json",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() != true)
                return;
            
            LoadWares(dialog.FileName);
        }
        
        public void WriteAutoSaveFile()
        {
            if (_data.Count == 0)
                File.Delete(_autoSaveFile);
            else
            {
                SaveWares(_autoSaveFile);
            }
        }

    }
}