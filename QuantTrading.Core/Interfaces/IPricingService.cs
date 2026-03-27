using QuantTrading.Core.Models;

namespace QuantTrading.Core.Interfaces;

public interface IPricingService
{
    /// <summary>
    /// Prend un flux de prix et retourne un flux d'options pricées selon Black-Scholes.
    /// </summary>
    /// <param name="parameters">Paramètres BS regroupés en Value Object.</param>
    /// <param name="cancellationToken">Token pour arrêter le flux proprement.</param>
    IObservable<PricedOption> PriceStream(
        string symbol,
        OptionParameters parameters,
        CancellationToken cancellationToken = default);
}
