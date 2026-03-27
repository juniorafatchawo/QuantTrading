using ScottPlot;
using System.Reactive.Linq;
using QuantTrading.UI.ViewModels;
using System.Windows;

namespace QuantTrading.UI;

/// <summary>
/// Code-behind strictement limité au rendu ScottPlot (MVVM : aucune logique métier ici).
/// Toute la gestion des données est dans MainViewModel.
/// </summary>
public partial class MainWindow : Window
{
    private IDisposable? _plotSubscription;

    public MainWindow()
    {
        InitializeComponent();
        SetupPlotStyles();
    }

    private void SetupPlotStyles()
    {
        PricePlot.Plot.Axes.DateTimeTicksBottom();
        PricePlot.Plot.FigureBackground.Color = Color.FromHex("#1E1E1E");
        PricePlot.Plot.DataBackground.Color   = Color.FromHex("#1E1E1E");
        PricePlot.Plot.Grid.MajorLineColor    = Color.FromHex("#444444");
        PricePlot.Plot.Axes.Color(Color.FromHex("#FFFFFF"));
        PricePlot.Refresh();
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        if (_plotSubscription is not null) return;

        if (DataContext is MainViewModel vm)
        {
            // On s'abonne au flux de données préparé par le ViewModel
            _plotSubscription = vm.ChartDataStream
                .ObserveOn(System.Reactive.Concurrency.DispatcherScheduler.Current)
                .Subscribe(data => RenderChart(data.Prices, data.Times));
        }
    }

    /// <summary>Rendu pur ScottPlot — aucune logique de données ici.</summary>
    private void RenderChart(double[] prices, DateTime[] times)
    {
        PricePlot.Plot.Clear();

        if (prices.Length == 0) return;

        double[] xs = times.Select(t => t.ToOADate()).ToArray();

        var scatter = PricePlot.Plot.Add.Scatter(xs, prices);
        scatter.Color      = Color.FromHex("#2ECC71");
        scatter.LineWidth  = 2;
        scatter.MarkerSize = 0;

        PricePlot.Plot.Axes.AutoScale();
        PricePlot.Refresh();
    }

    protected override void OnClosed(EventArgs e)
    {
        _plotSubscription?.Dispose();
        base.OnClosed(e);
    }
}
