using System.Windows;
using FitnessAnalyticsHub.UI.WPF.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FitnessAnalyticsHub.Infrastructure;
using FitnessAnalyticsHub.Application;

namespace FitnessAnalyticsHub.UI.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((context, services) =>
                {
                    // Register Application Services first
                    services.AddApplication();  // Changed to match the expected extension method

                    // Then register Infrastructure Services
                    services.AddInfrastructure(context.Configuration);  // Using the single method

                    // Register ViewModels
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<StravaAuthViewModel>();
                    services.AddTransient<ActivitiesViewModel>();
                    services.AddTransient<AthleteProfileViewModel>();

                    // Register Windows
                    services.AddTransient<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }

            base.OnExit(e);
        }
    }
}
