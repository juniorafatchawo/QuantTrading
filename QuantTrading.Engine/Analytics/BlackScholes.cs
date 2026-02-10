using QuantTrading.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantTading.Engine.Analytics
{
    public static class BlackScholes
    {
        /// <summary>
        /// Calcule le prix d'un Call européen et ses Greques.
        /// </summary>
        /// <param name="S">Spot Price (Prix du sous-jacent)</param>
        /// <param name="K">Strike Price (Prix d'exercice)</param>
        /// <param name="T">Time to Maturity (en années)</param>
        /// <param name="r">Risk-free rate (taux sans risque, ex: 0.05)</param>
        /// <param name="sigma">Volatilité (ex: 0.20 pour 20%)</param>
        public static (double Price, OptionGreeks Greeks) CalculateCall(double S, double K, double T, double r, double sigma)
        {
            // Sécurité : évite la division par zéro si T ou sigma sont nuls
            if (T <= 0 || sigma <= 0)
                return (Math.Max(0, S - K), new OptionGreeks());

            var sqrtT = Math.Sqrt(T);
            var d1 = (Math.Log(S / K) + (r + 0.5 * sigma * sigma) * T) / (sigma * sqrtT);
            var d2 = d1 - sigma * sqrtT;

            var nd1 = NormalCdf(d1);
            var nd2 = NormalCdf(d2);
            var nPrimed1 = NormalPdf(d1); // Pour le Gamma/Vega

            // Formule du prix (Call)
            var price = S * nd1 - K * Math.Exp(-r * T) * nd2;

            // Calcul des Greques
            var delta = nd1;
            var gamma = nPrimed1 / (S * sigma * sqrtT);
            var vega = S * sqrtT * nPrimed1 / 100; // Divisé par 100 pour avoir une valeur lisible (% changement)
            var theta = (-((S * sigma * nPrimed1) / (2 * sqrtT)) - r * K * Math.Exp(-r * T) * nd2) / 365; // Theta par jour
            var rho = K * T * Math.Exp(-r * T) * nd2 / 100;

            return (price, new OptionGreeks(delta, gamma, vega, theta, rho));
        }

        // Cumulative Distribution Function (Approximation de Abramowitz & Stegun)
        // Très rapide et suffisant pour la finance temps réel.
        private static double NormalCdf(double x)
        {
            if (x < 0) return 1 - NormalCdf(-x);
            var k = 1 / (1 + 0.2316419 * x);
            var poly = k * (0.319381530 + k * (-0.356563782 + k * (1.781477937 + k * (-1.821255978 + 1.330274429 * k))));
            return 1.0 - 1.0 / Math.Sqrt(2 * Math.PI) * Math.Exp(-0.5 * x * x) * poly;
        }

        // Probability Density Function
        private static double NormalPdf(double x)
        {
            return 1.0 / Math.Sqrt(2 * Math.PI) * Math.Exp(-0.5 * x * x);
        }
    }
}
