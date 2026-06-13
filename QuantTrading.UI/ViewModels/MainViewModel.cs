using CommunityToolkit.Mvvm.ComponentModel;
using QuantTrading.Core.Interfaces;
using QuantTrading.Core.Models;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using ReactiveUI;

namespace QuantTrading.UI.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IPricingService _pricingService;
    private readonly ILogger<MainViewModel> _logger;
    private IDisposable? _subscription;
    private CancellationTokenSource _cts = new();

    // --- Chart data management ---
    // Queue<T> : enqueue/dequeue en O(1), contrairement à List<T>.RemoveAt(0) en O(n)
    private readonly Queue<double>   _chartPrices = new();
    private readonly Queue<DateTime> _chartTimes  = new();
    private const int ChartWindowSize = 50;

    private readonly Subject<(double[] Prices, DateTime[] Times)> _chartDataSource = new();

    /// <summary>Flux de données prêtes à rendre (fenêtre glissante de 50 points).</summary>
    public IObservable<(double[] Prices, DateTime[] Times)> ChartDataStream
        => _chartDataSource.AsObservable();

    public ObservableCollection<OptionDisplayViewModel> Options { get; } = new();

    [ObservableProperty] private bool _isStreaming;
    [ObservableProperty] private bool _isConnected = true;

    // --- Paramètres Black-Scholes — configurables depuis l'UI ---
    [ObservableProperty] private double _strike        = OptionParameters.Default.Strike;
    [ObservableProperty] private double _riskFreeRate  = OptionParameters.Default.RiskFreeRate;
    [ObservableProperty] private double _volatility    = OptionParameters.Default.Volatility;
    [ObservableProperty] private double _timeToMaturity = OptionParameters.Default.TimeToMaturity;

    private OptionParameters CurrentParameters => new()
    {
        Strike         = Strike,
        RiskFreeRate   = RiskFreeRate,
        Volatility     = Volatility,
        TimeToMaturity = TimeToMaturity
    };

    public MainViewModel(IPricingService pricingService, ILogger<MainViewModel> logger)
    {
        _pricingService = pricingService;
        _logger = logger;
        StartRealTimeStream();
    }

    // Redémarre le flux dès qu'un paramètre change (CommunityToolkit partial methods)
    partial void OnStrikeChanged(double value)        => RestartStream();
    partial void OnVolatilityChanged(double value)    => RestartStream();
    partial void OnRiskFreeRateChanged(double value)  => RestartStream();
    partial void OnTimeToMaturityChanged(double value) => RestartStream();

    private void RestartStream()
    {
        _subscription?.Dispose();
        _cts.Cancel();
        _cts.Dispose();
        _cts = new CancellationTokenSource();
        StartRealTimeStream();
    }

    private void StartRealTimeStream()
    {
        _subscription = _pricingService
            .PriceStream("AAPL", CurrentParameters, _cts.Token)
            .Sample(TimeSpan.FromMilliseconds(100))           // Évite de saturer l'UI
            .SubscribeOn(TaskPoolScheduler.Default)           // Calculs sur thread pool
            .ObserveOn(RxApp.MainThreadScheduler)            // Rendu sur thread UI
            .Subscribe(
                pricedOption =>
                {
                    UpdateOrAddOption(pricedOption);
                    UpdateChartData(pricedOption.OptionPrice, pricedOption.Timestamp);
                },
                ex =>
                {
                    _logger.LogCritical(ex, "Pricing stream permanently failed for {Symbol} after exhausting retries", "AAPL");
                    IsConnected = false;
                    IsStreaming = false;
                });

        IsStreaming = true;
    }

    private void UpdateOrAddOption(PricedOption priced)
    {
        var existing = Options.FirstOrDefault(x => x.Symbol == priced.Symbol);
        if (existing is null)
        {
            var vm = new OptionDisplayViewModel(priced.Symbol);
            vm.Update(priced);
            Options.Add(vm);
        }
        else
        {
            existing.Update(priced);
        }
    }

    private void UpdateChartData(double price, DateTime time)
    {
        _chartPrices.Enqueue(price);
        _chartTimes.Enqueue(time);

        if (_chartPrices.Count > ChartWindowSize)
        {
            _chartPrices.Dequeue(); // O(1)
            _chartTimes.Dequeue();
        }

        _chartDataSource.OnNext((_chartPrices.ToArray(), _chartTimes.ToArray()));
    }

    public void Dispose()
    {
        _cts.Cancel();
        _subscription?.Dispose();
        _chartDataSource.OnCompleted(); // Fix : OnCompleted() et non OnNext(default)
        _chartDataSource.Dispose();
        _cts.Dispose();
    }
}
