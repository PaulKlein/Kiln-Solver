using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace KilnSolver.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        Dispatcher.UnhandledException += OnUnhandledDispatcherException;
    }

    private void OnUnhandledDispatcherException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Console.WriteLine(e.Exception);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Console.WriteLine(e.ExceptionObject?.ToString());
    }
}

