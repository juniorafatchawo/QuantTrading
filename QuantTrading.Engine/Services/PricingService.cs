using QuantTrading.Engine.Analytics;
using QuantTrading.Core.Interfaces;
using QuantTrading.Core.Models;
using Microsoft.Extensions.Logging;
using System.Reactive.Linq;

namespace QuantTrading.Engine.Services;

public class PricingService : IPricingService
{
    private readonly IMarketDataService _marketDataService;
    private readonly ILogger<PricingService> _logger;

    private const int MaxRetries = 5;

    public PricingService(IMarketDataService marketDataService, ILogger<PricingService> logger)
    {
        _marketDataService = marketDataService;
        _logger = logger;
    }

    public IObservable<PricedOption> PriceStream(
        string symbol,
        OptionParameters parameters,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting pricing stream — Symbol={Symbol} K={Strike} σ={Volatility} r={Rate} T={Maturity}",
            symbol, parameters.Strike, parameters.Volatility, parameters.RiskFreeRate, parameters.TimeToMaturity);

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
            })
            .Do(
                _ => { },
                ex => _logger.LogError(ex, "Error in pricing stream for {Symbol}", symbol),
                () => _logger.LogInformation("Pricing stream completed for {Symbol}", symbol))
            .RetryWhen(errors => errors
                .Take(MaxRetries)
                .Select((ex, attempt) => (ex, attempt))
                .SelectMany(t =>
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, t.attempt));
                    _logger.LogWarning(t.ex,
                        "Pricing stream error for {Symbol} (attempt {Attempt}/{Max}), retry in {Delay}s",
                        symbol, t.attempt + 1, MaxRetries, delay.TotalSeconds);
                    return Observable.Timer(delay);
            }));
    }
}
