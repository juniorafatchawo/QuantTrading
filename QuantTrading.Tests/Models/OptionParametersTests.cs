using QuantTrading.Core.Models;

namespace QuantTrading.Tests.Models;

public class OptionParametersTests
{
    [Fact]
    public void Default_HasExpectedValues()
    {
        var p = OptionParameters.Default;

        Assert.Equal(150.0, p.Strike);
        Assert.Equal(0.05,  p.RiskFreeRate);
        Assert.Equal(0.25,  p.Volatility);
        Assert.Equal(0.5,   p.TimeToMaturity);
    }

    [Fact]
    public void With_Record_CreatesNewInstanceWithOverriddenValue()
    {
        var original = OptionParameters.Default;
        var modified = original with { Strike = 200.0 };

        Assert.Equal(200.0, modified.Strike);
        Assert.Equal(original.RiskFreeRate,   modified.RiskFreeRate);
        Assert.Equal(original.Volatility,     modified.Volatility);
        Assert.Equal(original.TimeToMaturity, modified.TimeToMaturity);
    }

    [Fact]
    public void TwoDefaults_AreEqual()
    {
        Assert.Equal(OptionParameters.Default, OptionParameters.Default);
    }
}
