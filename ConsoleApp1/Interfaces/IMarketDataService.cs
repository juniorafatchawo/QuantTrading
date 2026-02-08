using QuantTrading.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantTrading.Core.Interfaces
{
    public interface IMarketDataService
    {
        // S'abonne à un flux de ticks pour un symbole donné
        IObservable<MarketTick> GetTickerStream(string symbol);
    }
}
