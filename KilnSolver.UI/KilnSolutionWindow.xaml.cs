using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using KilnSolver.Core;

namespace KilnSolver.UI;

public partial class KilnSolutionWindow : Window
{
    readonly Ware[] _wares;
    readonly SolveGenerator.SolveLevelInfo[] _solution;

    public KilnSolutionWindow(Ware[] wares, SolveGenerator.SolveLevelInfo[] solution)
    {
        _wares = wares;
        _solution = solution;
        InitializeComponent();
        Loaded += OnLoaded;
    }

    void OnLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnLoaded;

        var solutionSection = CreateSection("Solution");


        for (var i = 0; i < _solution.Length; i++)
        {
            var level = _solution[i];
            var itemCount = 0;
            var fill = 0;
            var list = new List();
            if (level.WareCounts.Length == 0)
            {
                list.ListItems.Add(new ListItem(new Paragraph(new Italic(new Run("No Items") { Foreground = Brushes.Gray }))));
            }
            else
            {
                foreach (var item in level.WareCounts)
                {
                    itemCount += item.Count;
                    fill += item.Count * item.Ware.Size;
                    list.ListItems.Add(new ListItem(new Paragraph(new Italic(new Run($"{item.Count}x {item.Ware.Name}")))));
                }
            }

            var header = $"Level {i + 1}";
            if (itemCount > 0)
                header += $" ({itemCount} items, {fill}% full)";
            solutionSection.Blocks.Add(new Paragraph(new Run(header)));
            solutionSection.Blocks.Add(list);
            ResultDocument.Blocks.Add(solutionSection);
        }

        
        var inputsSection = CreateSection("Input");

        foreach (var waresByAllowedLevel in _wares
                     .Where(w => w.ItemCount > 0)
                     .ToLookup(w => w.AllowedLevel)
                     .OrderBy(wg => wg.Key))
        {
            var allowedLevel = waresByAllowedLevel.Key;
            var header = allowedLevel switch
            {
                AllowedLevel.All => "Any Level",
                AllowedLevel.TopMost => "Top Level Only",
                AllowedLevel.BottomMost => "Bottom Level Only",
                AllowedLevel.TopTwo => "Top Two Levels Only",
                AllowedLevel.BottomTwo => "Bottom Two Levels Only",
                _ => $"{allowedLevel} Levels Only",
            };
            inputsSection.Blocks.Add(new Paragraph(new Run(header)));
            
            var inputList = new List();
            foreach (var ware in waresByAllowedLevel)
            {
                var desc = $"{ware.ItemCount}x {ware.Name}";
                inputList.ListItems.Add(new ListItem(new Paragraph(new Italic(new Run(desc)))));
            }
            inputsSection.Blocks.Add(inputList);
        }

        
        ResultDocument.Blocks.Add(inputsSection);
    }

    static Section CreateSection(string title)
    {
        var section = new Section
        {
            //BorderBrush = new SolidColorBrush(Colors.Gainsboro), 
            //BorderThickness = new Thickness(1), 
            Margin = new Thickness(10),
            Padding = new Thickness(10),
        };
        
        section.Blocks.Add(new Paragraph(new Bold(new Run(title) { FontSize = 36, Foreground = Brushes.Gray })));

        section.Blocks.Add(new Paragraph(new InlineUIContainer(new Line
        {
            X1 = 0, X2 = 10, Y1 = 0, Y2 = 0, Stroke = new SolidColorBrush(Colors.Gray), StrokeThickness = 0.5,
            Stretch = Stretch.Fill
        })));
        return section;
    }

    void Print(object sender, RoutedEventArgs e)
    {
        var printDialog = new PrintDialog();
        if (printDialog.ShowDialog() != true)
            return;

        IDocumentPaginatorSource paginatorSource = ResultDocument;
        printDialog.PrintDocument(paginatorSource.DocumentPaginator, "Kiln Solver Solution");

        this.Close();
    }
}