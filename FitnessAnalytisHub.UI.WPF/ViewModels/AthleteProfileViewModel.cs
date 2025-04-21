using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using System.Windows.Input;

namespace FitnessAnalyticsHub.UI.WPF.ViewModels
{
    public class AthleteProfileViewModel : ViewModelBase
    {
        private readonly IAthleteService _athleteService;

        private ObservableCollection<AthleteDto> _athletes;
        public ObservableCollection<AthleteDto> Athletes
        {
            get => _athletes;
            set => SetProperty(ref _athletes, value);
        }

        private AthleteDto _selectedAthlete;
        public AthleteDto SelectedAthlete
        {
            get => _selectedAthlete;
            set => SetProperty(ref _selectedAthlete, value);
        }

        private string _firstName;
        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName;
        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _city;
        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        private string _country;
        public string Country
        {
            get => _country;
            set => SetProperty(ref _country, value);
        }

        public ICommand LoadAthletesCommand { get; }
        public ICommand SelectAthleteCommand { get; }
        public ICommand SaveAthleteCommand { get; }

        public AthleteProfileViewModel(IAthleteService athleteService)
        {
            _athleteService = athleteService;

            Athletes = new ObservableCollection<AthleteDto>();

            LoadAthletesCommand = new AsyncRelayCommand(_ => LoadAthletesAsync());
            SelectAthleteCommand = new RelayCommand(SelectAthlete);
            SaveAthleteCommand = new AsyncRelayCommand(_ => SaveAthleteAsync(), _ => SelectedAthlete != null);

            // Load data on creation
            LoadAthletesAsync();
        }

        private async Task LoadAthletesAsync()
        {
            await RunCommandAsync(async () =>
            {
                var athletes = await _athleteService.GetAllAthletesAsync();
                Athletes.Clear();
                foreach (var athlete in athletes)
                {
                    Athletes.Add(athlete);
                }

                if (Athletes.Count > 0 && SelectedAthlete == null)
                {
                    SelectAthlete(Athletes[0]);
                }
            });
        }

        private void SelectAthlete(object parameter)
        {
            var athlete = parameter as AthleteDto;
            if (athlete == null) return;

            SelectedAthlete = athlete;
            FirstName = athlete.FirstName;
            LastName = athlete.LastName;
            Email = athlete.Email;
            City = athlete.City;
            Country = athlete.Country;
        }

        private async Task SaveAthleteAsync()
        {
            if (SelectedAthlete == null) return;

            await RunCommandAsync(async () =>
            {
                var updateAthleteDto = new UpdateAthleteDto
                {
                    Id = SelectedAthlete.Id,
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    City = City,
                    Country = Country,
                    Username = SelectedAthlete.Username,
                    ProfilePictureUrl = SelectedAthlete.ProfilePictureUrl
                };

                await _athleteService.UpdateAthleteAsync(updateAthleteDto);

                // Refresh the list
                await LoadAthletesAsync();

                // Find and select the updated athlete
                foreach (var athlete in Athletes)
                {
                    if (athlete.Id == SelectedAthlete.Id)
                    {
                        SelectAthlete(athlete);
                        break;
                    }
                }
            });
        }
    }
}
