using QuantTrading.Core.Models;

namespace QuantTrading.Core.Interfaces;

public interface IMarketDataService
{
    /// <summary>
    /// S'abonne à un flux de ticks pour un symbole donné.
    /// </summary>
    /// <param name="cancellationToken">Token pour arrêter le flux proprement.</param>
    IObservable<MarketTick> GetTickerStream(string symbol, CancellationToken cancellationToken = default);
}
