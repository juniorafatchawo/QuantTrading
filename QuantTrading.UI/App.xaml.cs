using Microsoft.Extensions.DependencyInjection;
using QuantTrading.UI.ViewModels;
using System.Windows;
using QuantTading.Engine.Services;
using QuantTrading.Core.Interfaces;

namespace QuantTrading.UI
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services { get; }

        public App()
        {
            Services = ConfigureServices();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // --- Services (Engine) ---
            // On enregistre les interfaces vers leurs implémentations concrètes
            services.AddSingleton<IMarketDataService, RandomMarketDataService>();
            services.AddSingleton<IPricingService, PricingService>();

            // --- ViewModels (UI) ---
            services.AddTransient<MainViewModel>();

            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Lancement manuel de la fenêtre principale avec injection du ViewModel
            var mainWindow = new MainWindow();
            mainWindow.DataContext = Services.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }
    }

}
