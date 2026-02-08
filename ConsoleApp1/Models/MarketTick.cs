using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantTrading.Core.Models
{
    public readonly record struct MarketTick(string Symbol, double Price, DateTime Timestamp);
}
