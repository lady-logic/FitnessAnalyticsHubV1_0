using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Domain.Interfaces;
using System.Windows.Input;

namespace FitnessAnalytisHub.UI.WPF.ViewModels
{
    public class StravaAuthViewModel : ViewModelBase
    {
        private readonly IStravaService _stravaService;
        private readonly IAthleteService _athleteService;
        private readonly IActivityService _activityService;

        private string _authorizationUrl;
        public string AuthorizationUrl
        {
            get => _authorizationUrl;
            set => SetProperty(ref _authorizationUrl, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private bool _isAuthenticated;
        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set => SetProperty(ref _isAuthenticated, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand AuthorizeCommand { get; }
        public ICommand ImportActivitiesCommand { get; }

        private string _accessToken;
        private int _athleteId;

        public StravaAuthViewModel(
            IStravaService stravaService,
            IAthleteService athleteService,
            IActivityService activityService)
        {
            _stravaService = stravaService;
            _athleteService = athleteService;
            _activityService = activityService;

            AuthorizeCommand = new AsyncRelayCommand(_ => AuthorizeWithStrava());
            ImportActivitiesCommand = new AsyncRelayCommand(_ => ImportActivities(), _ => IsAuthenticated);

            // Initialize the auth URL
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                AuthorizationUrl = await _stravaService.GetAuthorizationUrlAsync();
                StatusMessage = "Bereit zur Authentifizierung mit Strava";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fehler beim Initialisieren: {ex.Message}";
            }
        }

        private async Task AuthorizeWithStrava()
        {
            await RunCommandAsync(async () =>
            {
                StatusMessage = "Starte Authentifizierung...";

                // Setup a local callback server
                var port = 8080;
                var listener = new HttpListener();
                listener.Prefixes.Add($"http://localhost:{port}/callback/");
                listener.Start();

                // Open browser with auth URL
                Process.Start(new ProcessStartInfo
                {
                    FileName = AuthorizationUrl,
                    UseShellExecute = true
                });

                StatusMessage = "Warte auf Strava Callback...";

                // Wait for the callback
                var context = await Task.Run(() => listener.GetContext());
                var code = context.Request.QueryString["code"];

                // Respond to the browser request
                var response = context.Response;
                string responseString = "<html><body><h1>Authentifizierung erfolgreich!</h1>" +
                                       "<p>Du kannst dieses Fenster jetzt schließen und zur Anwendung zurückkehren.</p></body></html>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.OutputStream.Close();
                listener.Stop();

                if (string.IsNullOrEmpty(code))
                {
                    throw new Exception("Authentifizierungscode nicht erhalten");
                }

                StatusMessage = "Code erhalten, hole Token...";

                // Exchange code for token
                var tokenInfo = await _stravaService.ExchangeCodeForTokenAsync(code);
                _accessToken = tokenInfo.AccessToken;

                // Import athlete profile
                StatusMessage = "Importiere Athletenprofil...";
                var athlete = await _athleteService.ImportAthleteFromStravaAsync(_accessToken);
                _athleteId = athlete.Id;

                IsAuthenticated = true;
                StatusMessage = $"Erfolgreich authentifiziert als {athlete.FirstName} {athlete.LastName}";
            });
        }

        private async Task ImportActivities()
        {
            await RunCommandAsync(async () =>
            {
                if (string.IsNullOrEmpty(_accessToken) || _athleteId == 0)
                {
                    throw new Exception("Nicht authentifiziert");
                }

                StatusMessage = "Importiere Aktivitäten von Strava...";
                var activities = await _activityService.ImportActivitiesFromStravaAsync(_athleteId, _accessToken);
                StatusMessage = $"{activities.Count()} Aktivitäten importiert";
            });
        }
    }
}
