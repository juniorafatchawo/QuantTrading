using QuantTrading.Engine.Analytics;
using QuantTrading.Core.Interfaces;
using QuantTrading.Core.Models;
using System.Reactive.Linq;

namespace QuantTrading.Engine.Services;

public class PricingService : IPricingService
{
    private readonly IMarketDataService _marketDataService;

    public PricingService(IMarketDataService marketDataService)
    {
        _marketDataService = marketDataService;
    }

    public IObservable<PricedOption> PriceStream(
        string symbol,
        OptionParameters parameters,
        CancellationToken cancellationToken = default)
    {
        return _marketDataService
            .GetTickerStream(symbol, cancellationToken)
            .Select(tick =>
            {
                var result = BlackScholes.CalculateCall(
                    S: tick.Price,
                    K: parameters.Strike,
                    T: parameters.TimeToMaturity,
                    r: parameters.RiskFreeRate,
                    sigma: parameters.Volatility);

                return new PricedOption(
                    Symbol: tick.Symbol,
                    SpotPrice: tick.Price,
                    OptionPrice: result.Price,
                    Greeks: result.Greeks,
                    Timestamp: tick.Timestamp);
            });
    }
}
