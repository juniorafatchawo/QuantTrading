using System.Reactive;
using System.Reactive.Linq;
using QuantTrading.Core.Interfaces;
using QuantTrading.Core.Models;
using Microsoft.Extensions.Logging;

namespace QuantTrading.Engine.Services;

public class RandomMarketDataService : IMarketDataService
{
    private readonly ILogger<RandomMarketDataService> _logger;

    private readonly Dictionary<string, double> _initialPrices = new()
    {
        { "EURUSD",  1.10    },
        { "AAPL",    150.0   },
        { "BTCUSD",  45000.0 },
        { "SPY",     400.0   }
    };

    public RandomMarketDataService(ILogger<RandomMarketDataService> logger)
    {
        _logger = logger;
    }

    public IObservable<MarketTick> GetTickerStream(string symbol, CancellationToken cancellationToken = default)
    {
        if (!_initialPrices.ContainsKey(symbol))
        {
            _logger.LogError("Unknown symbol requested: {Symbol}", symbol);
            throw new ArgumentException($"Symbole inconnu pour la simulation : {symbol}", nameof(symbol));
        }

        double currentPrice = _initialPrices[symbol];
        var random = Random.Shared;

        // Construit un observable qui se termine proprement sur annulation
        var cancelSignal = Observable.Create<Unit>(obs =>
        {
            var reg = cancellationToken.Register(() => { obs.OnNext(Unit.Default); obs.OnCompleted(); });
            return () => reg.Unregister();
        });

        _logger.LogInformation("Starting market data stream for {Symbol} at initial price {Price}", symbol, currentPrice);

        return Observable.Create<MarketTick>(observer =>
        {
            var subscription = Observable
                .Interval(TimeSpan.FromMilliseconds(50))
                .TakeUntil(cancelSignal)
                .Select(_ =>
                {
                    if (random.NextDouble() < 0.0005)
                    {
                        throw new InvalidOperationException($"Simulated feed disruption for {symbol}");
                    }

                    var change = (random.NextDouble() - 0.5) * (currentPrice * 0.002);
                    currentPrice += change;
                    return new MarketTick(symbol, currentPrice, DateTime.UtcNow);
                })
                .Subscribe(observer);

            return subscription;
        });
    }
}
