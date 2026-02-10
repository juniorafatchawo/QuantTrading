using QuantTading.Engine.Analytics;
using QuantTrading.Core.Interfaces;
using QuantTrading.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantTading.Engine.Services
{
    public class PricingService : IPricingService
    {
        private readonly IMarketDataService _marketDataService;

        public PricingService(IMarketDataService marketDataService)
        {
            _marketDataService = marketDataService;
        }

        public IObservable<PricedOption> PriceStream(string symbol, double strike, double rate, double volatility, double timeToMaturity)
        {
            // 1. On récupère le flux brut
            return _marketDataService.GetTickerStream(symbol)
                .Select(tick =>
                {
                    // 2. Pour chaque tick, on lance le calcul Black-Scholes
                    // Note : Dans un vrai système, les paramètres (T, r, sigma) pourraient aussi bouger.
                    var result = BlackScholes.CalculateCall(tick.Price, strike, timeToMaturity, rate, volatility);

                    // 3. On retourne l'objet enrichi
                    return new PricedOption(
                        Symbol: tick.Symbol,
                        SpotPrice: tick.Price,
                        OptionPrice: result.Price,
                        Greeks: result.Greeks,
                        Timestamp: tick.Timestamp
                    );
                });
        }
    }
}
