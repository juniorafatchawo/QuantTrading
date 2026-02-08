using Microsoft.Extensions.DependencyInjection;
using QuantTrading.UI.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;

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

            // 1. Enregistrement des ViewModels
            services.AddTransient<MainViewModel>();

            // 2. Enregistrement des Services (Mock pour l'instant ou vides)
            // Demain on ajoutera : services.AddSingleton<IMarketDataService, RandomMarketDataService>();

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
