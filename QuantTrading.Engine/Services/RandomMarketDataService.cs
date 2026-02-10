using System.Reactive.Linq;
using QuantTrading.Core.Interfaces;
using QuantTrading.Core.Models;

namespace QuantTading.Engine.Services
{
    public class RandomMarketDataService : IMarketDataService
    {
        // Simulation : Prix de départ par symbole
        private readonly Dictionary<string, double> _initialPrices = new()
    {
        { "EURUSD", 1.10 },
        { "AAPL", 150.0 },
        { "BTCUSD", 45000.0 },
        { "SPY", 400.0 }
    };

        public IObservable<MarketTick> GetTickerStream(string symbol)
        {
            if (!_initialPrices.ContainsKey(symbol))
                throw new ArgumentException($"Symbole inconnu pour la simulation : {symbol}");

            double currentPrice = _initialPrices[symbol];
            var random = Random.Shared;

            // Émet un tick toutes les 10 à 200 ms (aléatoire) pour simuler la haute fréquence
            return Observable.Create<MarketTick>(observer =>
            {
                return Observable.Interval(TimeSpan.FromMilliseconds(50)) // Tick de base toutes les 50ms
                    .Select(_ =>
                    {
                        // Random Walk : Le prix bouge un petit peu (volatilité simulée)
                        var change = (random.NextDouble() - 0.5) * (currentPrice * 0.002); // +/- 0.1%
                        currentPrice += change;

                        return new MarketTick(symbol, currentPrice, DateTime.UtcNow);
                    })
                    .Subscribe(observer);
            });
        }
    }
}
