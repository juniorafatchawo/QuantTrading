using CommunityToolkit.Mvvm.ComponentModel;
using QuantTrading.Core.Models;


namespace QuantTrading.UI.ViewModels
{
    public partial class OptionDisplayViewModel : ObservableObject
    {
        [ObservableProperty] private string _symbol;
        [ObservableProperty] private double _spotPrice;
        [ObservableProperty] private double _optionPrice;
        [ObservableProperty] private double _delta;
        [ObservableProperty] private double _vega;
        [ObservableProperty] private DateTime _lastUpdate;

        // Propriété technique pour le clignotement (Blinking)
        [ObservableProperty] private string _changeDirection; // "Up", "Down" ou "None"

        public OptionDisplayViewModel(string symbol)
        {
            _symbol = symbol;
        }

        public void Update(PricedOption priced)
        {
            // Logique pour déterminer la couleur du clignotement
            ChangeDirection = priced.OptionPrice > OptionPrice ? "Up" :
                              priced.OptionPrice < OptionPrice ? "Down" : "None";

            SpotPrice = priced.SpotPrice;
            OptionPrice = priced.OptionPrice;
            Delta = priced.Greeks.Delta;
            Vega = priced.Greeks.Vega;
            LastUpdate = priced.Timestamp;
        }
    }
}
