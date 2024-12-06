using System.IO;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Xps.Packaging;
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

        ResultDocument.Blocks.Add(new Paragraph(new Bold(new Run("Solution") { FontSize = 16 })));
        
        ResultDocument.Blocks.Add(CreateLine());

        for (var i = 0; i < _solution.Length; i++)
        {
            var level = _solution[i];
            var fill = 0;
            var list = new List();
            if (level.WareCounts.Length == 0)
            {
                list.ListItems.Add(new ListItem(new Paragraph(new Run("No Items") { Foreground = Brushes.Gray })));
            }
            else
            {
                foreach (var item in level.WareCounts)
                {
                    fill += item.Count * item.Ware.Size;
                    list.ListItems.Add(new ListItem(new Paragraph(new Run($"{item.Count}x {item.Ware.Name}"))));
                }
            }

            var header = $"Level {i + 1}";
            if (fill > 0)
                header += $" ({fill}% full)";
            ResultDocument.Blocks.Add(new Paragraph(new Bold(new Run(header))));
            ResultDocument.Blocks.Add(list);
        }

        ResultDocument.Blocks.Add(CreateLine());
        
        ResultDocument.Blocks.Add(new Paragraph(new Bold(new Run("Input") { FontSize = 16 })));
        var inputList = new List();
        foreach (var ware in _wares)
            inputList.ListItems.Add(new ListItem(new Paragraph(new Italic(new Run($"{ware.ItemCount}x {ware.Name}")))));

        ResultDocument.Blocks.Add(inputList);
    }

    static Paragraph CreateLine()
    {
        return new Paragraph(new InlineUIContainer(new Line
        {
            X1 = 0, X2 = 10, Y1 = 0, Y2 = 0, Stroke = new SolidColorBrush(Colors.Gray), StrokeThickness = 1,
            Stretch = Stretch.Fill
        }));
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