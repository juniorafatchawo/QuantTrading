using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantTrading.Core.Models
{
    /// <summary>
    /// Résultat complet du pricing pour l'affichage.
    /// </summary>
    public readonly record struct PricedOption(
        string Symbol,
        double SpotPrice,
        double OptionPrice,
        OptionGreeks Greeks,
        DateTime Timestamp
    );
}
