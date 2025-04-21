using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessAnalyticsHub.Application.Interfaces;
using System.Windows.Input;

namespace FitnessAnalytisHub.UI.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IAthleteService _athleteService;
        private readonly IActivityService _activityService;
        private readonly StravaAuthViewModel _stravaAuthViewModel;
        private readonly ActivitiesViewModel _activitiesViewModel;
        private readonly AthleteProfileViewModel _athleteProfileViewModel;

        private ViewModelBase _currentViewModel;

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand NavigateToAuthCommand { get; }
        public ICommand NavigateToActivitiesCommand { get; }
        public ICommand NavigateToProfileCommand { get; }

        public MainViewModel(
            IAthleteService athleteService,
            IActivityService activityService,
            StravaAuthViewModel stravaAuthViewModel,
            ActivitiesViewModel activitiesViewModel,
            AthleteProfileViewModel athleteProfileViewModel)
        {
            _athleteService = athleteService;
            _activityService = activityService;
            _stravaAuthViewModel = stravaAuthViewModel;
            _activitiesViewModel = activitiesViewModel;
            _athleteProfileViewModel = athleteProfileViewModel;

            // Set initial view
            CurrentViewModel = _stravaAuthViewModel;

            // Configure navigation commands
            NavigateToAuthCommand = new RelayCommand(_ => CurrentViewModel = _stravaAuthViewModel);
            NavigateToActivitiesCommand = new RelayCommand(_ => CurrentViewModel = _activitiesViewModel);
            NavigateToProfileCommand = new RelayCommand(_ => CurrentViewModel = _athleteProfileViewModel);
        }
    }
}
