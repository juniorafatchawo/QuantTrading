using CommunityToolkit.Mvvm.ComponentModel;
using QuantTrading.Core.Interfaces;
using QuantTrading.Core.Models;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using ReactiveUI;

namespace QuantTrading.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject, IDisposable
    {
        private readonly IPricingService _pricingService;
        private IDisposable? _subscription;

        // La liste liée à la DataGrid
        public ObservableCollection<OptionDisplayViewModel> Options { get; } = new();

        [ObservableProperty] private bool _isStreaming;

        public MainViewModel(IPricingService pricingService)
        {
            _pricingService = pricingService;
            StartRealTimeStream();
        }

        private void StartRealTimeStream()
        {
            // On définit les paramètres de notre option à pricer (ex: AAPL Strike 150)
            // Dans un vrai projet, cela viendrait d'une sélection utilisateur

            _subscription = _pricingService
                .PriceStream("AAPL", strike: 150, rate: 0.05, volatility: 0.25, timeToMaturity: 0.5)

                // --- ÉTAPE CRITIQUE : PERFORMANCE ---
                // Le marché envoie 50ms (20 ticks/sec). L'œil humain et WPF n'ont pas besoin de tant.
                // .Sample(TimeSpan.FromMilliseconds(100)) capture la dernière valeur tous les 100ms (10 FPS).
                .Sample(TimeSpan.FromMilliseconds(100))

                // On s'assure que le traitement lourd reste en arrière-plan
                .SubscribeOn(TaskPoolScheduler.Default)

                // On revient sur le thread UI (Dispatcher) pour mettre à jour la collection
                .ObserveOn(RxApp.MainThreadScheduler)

                .Subscribe(pricedOption =>
                {
                    UpdateOrAddOption(pricedOption);
                },
                ex => { /* Gérer les erreurs de flux ici */ });

            IsStreaming = true;
        }

        private void UpdateOrAddOption(PricedOption priced)
        {
            var existing = Options.FirstOrDefault(x => x.Symbol == priced.Symbol);
            if (existing == null)
            {
                var newOption = new OptionDisplayViewModel(priced.Symbol);
                newOption.Update(priced);
                Options.Add(newOption);
            }
            else
            {
                existing.Update(priced);
            }
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
