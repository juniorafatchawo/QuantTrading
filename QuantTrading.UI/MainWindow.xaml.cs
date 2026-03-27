using ScottPlot;
using System.Reactive.Linq;
using QuantTrading.UI.ViewModels;
using System.Windows;
using QuantTrading.Core.Models;

namespace QuantTrading.UI;

public partial class MainWindow : Window
{
    private readonly List<double> _priceHistory = new();
    private readonly List<DateTime> _timeHistory = new();
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
        PricePlot.Plot.DataBackground.Color = Color.FromHex("#1E1E1E");
        PricePlot.Plot.Grid.MajorLineColor = Color.FromHex("#444444");
        PricePlot.Plot.Axes.Color(Color.FromHex("#FFFFFF"));
        PricePlot.Refresh();
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);

        // Sécurité : on ne s'abonne qu'une seule fois
        if (_plotSubscription != null) return;

        if (DataContext is MainViewModel vm)
        {
            _plotSubscription = vm.PriceUpdatedStream
                .ObserveOn(System.Reactive.Concurrency.DispatcherScheduler.Current)
                .Subscribe(priced =>
                {
                    UpdatePlot(priced.OptionPrice, priced.Timestamp);
                });
        }
    }

    private void UpdatePlot(double newPrice, DateTime time)
    {
        _priceHistory.Add(newPrice);
        _timeHistory.Add(time);

        // Garder une fenêtre glissante de 50 points
        if (_priceHistory.Count > 50)
        {
            _priceHistory.RemoveAt(0);
            _timeHistory.RemoveAt(0);
        }

        PricePlot.Plot.Clear();

        // Conversion des dates pour ScottPlot
        double[] xs = _timeHistory.Select(t => t.ToOADate()).ToArray();
        double[] ys = _priceHistory.ToArray();

        if (xs.Length > 0)
        {
            var scatter = PricePlot.Plot.Add.Scatter(xs, ys);
            scatter.Color = Color.FromHex("#2ECC71");
            scatter.LineWidth = 2;
            scatter.MarkerSize = 0; // On ne veut que la ligne pour un look trading clean

            PricePlot.Plot.Axes.AutoScale();
            PricePlot.Refresh();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _plotSubscription?.Dispose();
        base.OnClosed(e);
    }
}