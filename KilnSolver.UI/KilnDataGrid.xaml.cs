using KilnSolver.Core;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ModernWpf.Controls;

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

            _ = OnLoadedAsync();

        }

        async Task OnLoadedAsync()
        {
            
            try
            {
                if (File.Exists(_autoSaveFile))
                {
                    await LoadWaresAsync(_autoSaveFile);
                }
            }
            catch (Exception ex)
            {
                await ShowDialogAsync("Error",$"Failed to load auto-save file\n\nDetails:{ex}");
            }

            dataGrid.DataContext = _data;
        }

        async Task<ContentDialogResult> ShowDialogAsync(string title, string message, string primary = "Ok", string secondary = "")
        {
            var contentDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = primary,
                SecondaryButtonText = secondary,
                Owner = Window.GetWindow(this)
            };
            return await contentDialog.ShowAsync();
        }


        private void GenerateSolve(object sender, RoutedEventArgs e)
        {
            _ = GenerateSolutionAsync(false);
        }

        private void GenerateOptimisedSolve(object sender, RoutedEventArgs e)
        {
            _ = GenerateSolutionAsync(true);
        }

        async Task GenerateSolutionAsync(bool optimise)
        {
            try
            {
                if (_data.Count == 0)
                {
                    await ShowDialogAsync("Error", "No Wares added to list");
                    return;
                }

                var input = _data.ToArray();

                var solution = SolveGenerator.GenerateSolution(input, _levels, optimise);

                if (solution is null)
                {
                    await ShowDialogAsync("Failed", "No solution could be found");
                    return;
                }

                var resultWindow = new KilnSolutionWindow(input, solution);
                resultWindow.Owner = Window.GetWindow(this);
                resultWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                await ShowDialogAsync("Error", $"Whoops, something went wrong. Tell Paul :(\n\nDetails:\n{ex}");
            }
        }

        async Task SaveWaresAsync(string fileName)
        {
            await using var writer = File.Create(fileName);
            await JsonSerializer.SerializeAsync(writer, _data.ToArray());
        }

        async Task LoadWaresAsync(string fileName)
        {
            await using var reader = File.OpenRead(fileName);
            var wares = await JsonSerializer.DeserializeAsync<Ware[]>(reader);
            if (wares is { Length: > 0 })
            {
                _data.Clear();
                foreach (var ware in wares)
                    _data.Add(ware);
            }
        }

        void Clear(object sender, RoutedEventArgs e)
        {
            _ = ClearAsync();
        }
        async Task ClearAsync()
        {
            if (_data.Count == 0)
                return;
            
            const string confirmation = "Are you sure you want to clear all wares?";
            if (await ConfirmOperationAsync(confirmation))
                _data.Clear();
        }

        async Task<bool> ConfirmOperationAsync(string message)
        {
            return await ShowDialogAsync("Confirm", message, "Yes", "No") == ContentDialogResult.Primary;
        }

        void Save(object sender, RoutedEventArgs e)
        {
            _ = SaveAsync();
        }
        
        async Task SaveAsync()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Json files (*.json)|*.json",
                OverwritePrompt = true,
                
            };

            if (dialog.ShowDialog(Window.GetWindow(this)) != true)
                return;
            
            await SaveWaresAsync(dialog.FileName);
        }

        void Load(object sender, RoutedEventArgs e)
        {
            _ = LoadAsync();
        }

        async Task LoadAsync()
        {
            try
            {
                if (_data.Count != 0 && !await ConfirmOperationAsync(
                        "You have existing wares loaded. Loading a new file will overwrite them. Do you want to continue?"))
                {
                    return;
                }

                var dialog = new OpenFileDialog()
                {
                    Filter = "Json files (*.json)|*.json",
                    CheckFileExists = true
                };

                if (dialog.ShowDialog(Window.GetWindow(this)) != true)
                    return;

                await LoadWaresAsync(dialog.FileName);
            }
            catch (Exception ex)
            {
                await ShowDialogAsync("Error", $"Failed to load file. It may be invalid");
            }
        }
        
        public async Task WriteAutoSaveFileAsync()
        {
            if (_data.Count == 0)
                File.Delete(_autoSaveFile);
            else
            {
                await SaveWaresAsync(_autoSaveFile);
            }
        }

    }
}