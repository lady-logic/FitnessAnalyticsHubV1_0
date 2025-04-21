using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;

namespace FitnessAnalytisHub.UI.WPF.ViewModels
{
    public class ActivitiesViewModel : ViewModelBase
    {
        private readonly IActivityService _activityService;
        private readonly IAthleteService _athleteService;

        private ObservableCollection<ActivityDto> _activities;
        public ObservableCollection<ActivityDto> Activities
        {
            get => _activities;
            set => SetProperty(ref _activities, value);
        }

        private ObservableCollection<AthleteDto> _athletes;
        public ObservableCollection<AthleteDto> Athletes
        {
            get => _athletes;
            set => SetProperty(ref _athletes, value);
        }

        private ActivityStatisticsDto _statistics;
        public ActivityStatisticsDto Statistics
        {
            get => _statistics;
            set => SetProperty(ref _statistics, value);
        }

        private AthleteDto _selectedAthlete;
        public AthleteDto SelectedAthlete
        {
            get => _selectedAthlete;
            set
            {
                if (SetProperty(ref _selectedAthlete, value) && value != null)
                {
                    LoadActivitiesForAthleteAsync(value.Id);
                }
            }
        }

        private ActivityDto _selectedActivity;
        public ActivityDto SelectedActivity
        {
            get => _selectedActivity;
            set => SetProperty(ref _selectedActivity, value);
        }

        public ICommand LoadAthletesCommand { get; }
        public ICommand LoadActivitiesCommand { get; }

        public ActivitiesViewModel(IActivityService activityService, IAthleteService athleteService)
        {
            _activityService = activityService;
            _athleteService = athleteService;

            Activities = new ObservableCollection<ActivityDto>();
            Athletes = new ObservableCollection<AthleteDto>();

            LoadAthletesCommand = new AsyncRelayCommand(_ => LoadAthletesAsync());
            LoadActivitiesCommand = new AsyncRelayCommand(parameter => LoadActivitiesForAthleteAsync(parameter != null ? (int)parameter : 0));

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
                    SelectedAthlete = Athletes[0];
                }
            });
        }

        private async Task LoadActivitiesForAthleteAsync(int athleteId)
        {
            if (athleteId <= 0) return;

            await RunCommandAsync(async () =>
            {
                var activities = await _activityService.GetActivitiesByAthleteIdAsync(athleteId);
                var statistics = await _activityService.GetAthleteActivityStatisticsAsync(athleteId);

                Activities.Clear();
                foreach (var activity in activities)
                {
                    Activities.Add(activity);
                }

                Statistics = statistics;
            });
        }
    }
}
