using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantTrading.Core.Models
{
    public readonly record struct OptionGreeks(
    double Delta,
    double Gamma,
    double Vega,
    double Theta,
    double Rho
);
}
