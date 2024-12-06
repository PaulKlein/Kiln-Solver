using System.ComponentModel;
using System.Windows;

namespace KilnSolver.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        KilnDataGrid.WriteAutoSaveFile();
        base.OnClosing(e);
    }
}