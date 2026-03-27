using CommunityToolkit.Mvvm.ComponentModel;
using QuantTrading.Core.Interfaces;
using QuantTrading.Core.Models;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Subjects; // Essentiel pour le pont avec la View
using ReactiveUI;

namespace QuantTrading.UI.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IPricingService _pricingService;
    private IDisposable? _subscription;

    // 1. Source de diffusion pour le graphique (View)
    private readonly Subject<PricedOption> _priceStreamSource = new();

    // 2. Observable exposé pour que la MainWindow puisse s'y abonner
    public IObservable<PricedOption> PriceUpdatedStream => _priceStreamSource.AsObservable();

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
        // On définit les paramètres de simulation
        _subscription = _pricingService
            .PriceStream("AAPL", strike: 150, rate: 0.05, volatility: 0.25, timeToMaturity: 0.5)

            // --- PERFORMANCE ---
            // Échantillonnage pour ne pas saturer l'UI
            .Sample(TimeSpan.FromMilliseconds(100))

            // Calculs et logique sur le TaskPool (Thread de fond)
            .SubscribeOn(TaskPoolScheduler.Default)

            // Bascule sur le Thread UI pour la modification de la collection
            .ObserveOn(RxApp.MainThreadScheduler)

            .Subscribe(pricedOption =>
            {
                // Mise à jour de la grille
                UpdateOrAddOption(pricedOption);

                // Notification pour le graphique (Subject)
                _priceStreamSource.OnNext(pricedOption);
            },
            ex => {
                // Log de l'erreur (important en production)
                System.Diagnostics.Debug.WriteLine($"Erreur Flux: {ex.Message}");
            });

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
        // Nettoyage impératif pour éviter les fuites de mémoire (Memory Leaks)
        _subscription?.Dispose();
        _priceStreamSource.OnNext(default); // Optionnel: signaler la fin
        _priceStreamSource.Dispose();
    }
}