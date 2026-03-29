using QuantTrading.Engine.Analytics;

namespace QuantTrading.Tests.Analytics;

public class BlackScholesTests
{
    // Tolérance : 1 % de la valeur de référence (Hull, Options, Futures and Other Derivatives)
    private const double Tolerance = 0.01;

    // -------------------------------------------------------------------------
    // Prix
    // -------------------------------------------------------------------------

    [Fact]
    public void CalculateCall_AtTheMoney_ReturnsExpectedPrice()
    {
        // Référence Hull : S=100, K=100, T=1, r=5%, σ=20% → Call ≈ 10.45
        var (price, _) = BlackScholes.CalculateCall(S: 100, K: 100, T: 1, r: 0.05, sigma: 0.20);

        Assert.InRange(price, 10.45 * (1 - Tolerance), 10.45 * (1 + Tolerance));
    }

    [Fact]
    public void CalculateCall_DeepInTheMoney_PriceAboveIntrinsicValue()
    {
        // S=150, K=100 → valeur intrinsèque = 50 ; le Call doit valoir plus
        var (price, _) = BlackScholes.CalculateCall(S: 150, K: 100, T: 1, r: 0.05, sigma: 0.20);

        Assert.True(price > 50, $"Prix attendu > 50 (valeur intrinsèque), obtenu : {price:F4}");
    }

    [Fact]
    public void CalculateCall_DeepOutOfTheMoney_PriceNearZero()
    {
        // S=50, K=100 → très OTM, prix doit tendre vers 0
        var (price, _) = BlackScholes.CalculateCall(S: 50, K: 100, T: 1, r: 0.05, sigma: 0.20);

        Assert.True(price < 1.0, $"Prix attendu < 1.0 (très OTM), obtenu : {price:F4}");
    }

    // -------------------------------------------------------------------------
    // Greques — ATM reference
    // -------------------------------------------------------------------------

    [Fact]
    public void CalculateCall_AtTheMoney_DeltaInExpectedRange()
    {
        // Delta ATM ≈ 0.6368 (légèrement > 0.5 à cause du drift r)
        var (_, greeks) = BlackScholes.CalculateCall(S: 100, K: 100, T: 1, r: 0.05, sigma: 0.20);

        Assert.InRange(greeks.Delta, 0.6368 * (1 - Tolerance), 0.6368 * (1 + Tolerance));
    }

    [Fact]
    public void CalculateCall_DeepInTheMoney_DeltaNearOne()
    {
        var (_, greeks) = BlackScholes.CalculateCall(S: 150, K: 100, T: 1, r: 0.05, sigma: 0.20);

        Assert.True(greeks.Delta > 0.90, $"Delta ITM attendu > 0.90, obtenu : {greeks.Delta:F4}");
    }

    [Fact]
    public void CalculateCall_DeepOutOfTheMoney_DeltaNearZero()
    {
        var (_, greeks) = BlackScholes.CalculateCall(S: 50, K: 100, T: 1, r: 0.05, sigma: 0.20);

        Assert.True(greeks.Delta < 0.10, $"Delta OTM attendu < 0.10, obtenu : {greeks.Delta:F4}");
    }

    [Fact]
    public void CalculateCall_GammaIsPositive()
    {
        var (_, greeks) = BlackScholes.CalculateCall(S: 100, K: 100, T: 1, r: 0.05, sigma: 0.20);

        Assert.True(greeks.Gamma > 0, $"Gamma doit être positif, obtenu : {greeks.Gamma:F6}");
    }

    [Fact]
    public void CalculateCall_VegaIsPositive()
    {
        var (_, greeks) = BlackScholes.CalculateCall(S: 100, K: 100, T: 1, r: 0.05, sigma: 0.20);

        Assert.True(greeks.Vega > 0, $"Vega doit être positif, obtenu : {greeks.Vega:F6}");
    }

    [Fact]
    public void CalculateCall_ThetaIsNegative()
    {
        // Le temps qui passe détruit de la valeur pour un Call long
        var (_, greeks) = BlackScholes.CalculateCall(S: 100, K: 100, T: 1, r: 0.05, sigma: 0.20);

        Assert.True(greeks.Theta < 0, $"Theta doit être négatif, obtenu : {greeks.Theta:F6}");
    }

    // -------------------------------------------------------------------------
    // Cas limites (edge cases)
    // -------------------------------------------------------------------------

    [Fact]
    public void CalculateCall_TimeToMaturityZero_ReturnsIntrinsicValue()
    {
        // T=0 : pas de valeur temps, on retourne Max(0, S-K)
        var (price, greeks) = BlackScholes.CalculateCall(S: 110, K: 100, T: 0, r: 0.05, sigma: 0.20);

        Assert.Equal(10.0, price, precision: 5);
        Assert.Equal(0.0, greeks.Delta);
    }

    [Fact]
    public void CalculateCall_TimeToMaturityZero_OtmReturnsZero()
    {
        var (price, _) = BlackScholes.CalculateCall(S: 90, K: 100, T: 0, r: 0.05, sigma: 0.20);

        Assert.Equal(0.0, price, precision: 5);
    }

    [Fact]
    public void CalculateCall_SigmaZero_ReturnsIntrinsicValue()
    {
        // σ=0 : traité comme T=0
        var (price, _) = BlackScholes.CalculateCall(S: 110, K: 100, T: 1, r: 0.05, sigma: 0);

        Assert.Equal(10.0, price, precision: 5);
    }

    // -------------------------------------------------------------------------
    // Propriété de non-arbitrage
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(80,  100, 1, 0.05, 0.20)]
    [InlineData(100, 100, 1, 0.05, 0.20)]
    [InlineData(120, 100, 1, 0.05, 0.20)]
    public void CalculateCall_PriceAlwaysAboveIntrinsicPresentValue(double S, double K, double T, double r, double sigma)
    {
        // Condition de non-arbitrage : Call >= Max(0, S - K*e^(-rT))
        var (price, _) = BlackScholes.CalculateCall(S, K, T, r, sigma);
        var lowerBound = Math.Max(0, S - K * Math.Exp(-r * T));

        Assert.True(price >= lowerBound - 1e-9,
            $"Non-arbitrage violé : prix={price:F4}, borne inférieure={lowerBound:F4}");
    }
}
