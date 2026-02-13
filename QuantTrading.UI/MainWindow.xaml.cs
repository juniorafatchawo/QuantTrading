using ScottPlot;
using System.Reactive.Linq;
using QuantTrading.UI.ViewModels;
using System.Windows;

namespace QuantTrading.UI;

public partial class MainWindow : Window
{
    private readonly List<double> _priceHistory = new();
    private readonly List<DateTime> _timeHistory = new();

    public MainWindow()
    {
        InitializeComponent();

        // Configuration ScottPlot 5
        // Pour dire à l'axe X de traiter les nombres comme des dates
        PricePlot.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.DateTimeAutomatic();

        // Design Dark Mode
        PricePlot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#1E1E1E");
        PricePlot.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#1E1E1E");

        // Grille et Axes
        PricePlot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#444444");
        PricePlot.Plot.Axes.Color(ScottPlot.Color.FromHex("#FFFFFF"));
    }

    // On s'abonne au flux de données une fois que le DataContext est prêt
    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        if (DataContext is MainViewModel vm)
        {
            // On écoute le même flux que la grille
            // Note : Dans un projet complexe, on passerait par un service de messagerie
            // Ici on simplifie pour le portfolio
        }
    }

    public void UpdatePlot(double newPrice, DateTime time)
    {
        _priceHistory.Add(newPrice);
        _timeHistory.Add(time);

        if (_priceHistory.Count > 50)
        {
            _priceHistory.RemoveAt(0);
            _timeHistory.RemoveAt(0);
        }

        // On efface et on redessine
        PricePlot.Plot.Clear();

        // Conversion des DateTime en format numérique ScottPlot (OADate)
        double[] xs = _timeHistory.Select(t => t.ToOADate()).ToArray();
        double[] ys = _priceHistory.ToArray();

        var scatter = PricePlot.Plot.Add.Scatter(xs, ys);
        scatter.Color = ScottPlot.Color.FromHex("#2ECC71");
        scatter.LineWidth = 2;

        // Ajuster automatiquement les axes pour voir les données
        PricePlot.Plot.Axes.AutoScale();

        // Demander le rafraîchissement visuel
        PricePlot.Refresh();
    }
}