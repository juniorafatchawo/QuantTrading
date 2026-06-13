using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantTrading.UI.ViewModels;
using System.Windows;
using QuantTrading.Core.Interfaces;
using QuantTrading.Engine.Services;
using Serilog;

namespace QuantTrading.UI
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services { get; }

        public App()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("logs/quanttrading.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Services = ConfigureServices();

            Log.Information("QuantTrading started");
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/quanttrading-.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            services.AddLogging(builder => builder.AddSerilog(dispose: true));

            // --- Services (Engine) ---
            services.AddSingleton<IMarketDataService, RandomMarketDataService>();
            services.AddSingleton<IPricingService, PricingService>();

            // --- ViewModels (UI) ---
            services.AddTransient<MainViewModel>();

            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = new MainWindow();
            mainWindow.DataContext = Services.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("QuantTrading shutting down");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}
