using System.Windows;
using FitnessAnalytisHub.UI.WPF.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FitnessAnalyticsHub.Infrastructure;
using FitnessAnalyticsHub.Application;

namespace FitnessAnalytisHub.UI.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
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
                    // Registriere die Application-Services zuerst
                    ApplicationServiceRegistration.AddApplication(services);  // Vollqualifizierter Aufruf

                    // Dann registriere die Infrastructure-Services
                    InfrastructureServiceRegistration.AddInfrastructure(services, context.Configuration);  // Vollqualifizierter Aufruf

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
