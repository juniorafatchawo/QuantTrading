namespace QuantTrading.Core.Models;

/// <summary>
/// Value Object immuable regroupant les paramètres du modèle Black-Scholes.
/// Élimine les erreurs d'ordre de paramètres et garantit la cohérence des données.
/// </summary>
public sealed record OptionParameters
{
    public double Strike        { get; init; } = 150.0;
    public double RiskFreeRate  { get; init; } = 0.05;
    public double Volatility    { get; init; } = 0.25;
    public double TimeToMaturity { get; init; } = 0.5;

    public static OptionParameters Default => new();
}
