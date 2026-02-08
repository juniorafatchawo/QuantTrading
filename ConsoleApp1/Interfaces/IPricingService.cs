using QuantTrading.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantTrading.Core.Interfaces
{
    public interface IPricingService
    {
        // Prend un flux de prix et retourne un flux d'options pricées
        IObservable<PricedOption> PriceStream(string symbol, double strike, double rate, double volatility, double timeToMaturity);
    }
}
