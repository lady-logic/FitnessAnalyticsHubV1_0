using AutoMapper;
using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Domain.Entities;
using FitnessAnalyticsHub.Domain.Models;
using FitnessAnalyticsHub.Domain.ValueObjects;

namespace FitnessAnalyticsHub.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Athlete mappings
        CreateMap<Athlete, AthleteDto>();
        CreateMap<CreateAthleteDto, Athlete>();
        CreateMap<UpdateAthleteDto, Athlete>();
        CreateMap<Athlete, Athlete>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        // Activity mappings
        CreateMap<Activity, ActivityDto>()
            .ForMember(dest => dest.MovingTime, opt => opt.MapFrom(src => TimeSpan.FromSeconds(src.MovingTime)))
            .ForMember(dest => dest.ElapsedTime, opt => opt.MapFrom(src => TimeSpan.FromSeconds(src.ElapsedTime)))
            .ForMember(dest => dest.AthleteFullName, opt => opt.MapFrom(src =>
            src.Athlete != null ? $"{src.Athlete.FirstName} {src.Athlete.LastName}" : string.Empty));

        CreateMap<CreateActivityDto, Activity>()
            .ForMember(dest => dest.MovingTime, opt => opt.MapFrom(src => src.MovingTimeSeconds))
            .ForMember(dest => dest.ElapsedTime, opt => opt.MapFrom(src => src.ElapsedTimeSeconds))
            // Pace als berechneten Wert aus Distance und MovingTime
            .ForMember(dest => dest.Pace, opt => opt.MapFrom(src =>
                CreatePaceFromDistanceAndTime(src.Distance, src.MovingTimeSeconds)));

        CreateMap<UpdateActivityDto, Activity>()
            .ForMember(dest => dest.MovingTime, opt => opt.MapFrom(src => src.MovingTimeSeconds))
            .ForMember(dest => dest.ElapsedTime, opt => opt.MapFrom(src => src.ElapsedTimeSeconds));

        CreateMap<Activity, Activity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AthleteId, opt => opt.Ignore()) // Wird separat gesetzt
            .ForMember(dest => dest.Athlete, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.Now))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => (string)null)); // Explizit null setzen

        // Training plan mappings
        CreateMap<TrainingPlan, TrainingPlanDto>();
        CreateMap<CreateTrainingPlanDto, TrainingPlan>();
        CreateMap<UpdateTrainingPlanDto, TrainingPlan>();

        // Planned activity mappings
        CreateMap<PlannedActivity, PlannedActivityDto>()
            .ForMember(dest => dest.PlannedDuration, opt =>
                opt.MapFrom(src => src.PlannedDuration.HasValue ? TimeSpan.FromMinutes(src.PlannedDuration.Value) : (TimeSpan?)null));

        CreateMap<CreatePlannedActivityDto, PlannedActivity>()
            .ForMember(dest => dest.PlannedDuration, opt =>
                opt.MapFrom(src => src.PlannedDurationMinutes));

        CreateMap<UpdatePlannedActivityDto, PlannedActivity>()
            .ForMember(dest => dest.PlannedDuration, opt =>
                opt.MapFrom(src => src.PlannedDurationMinutes));

        // Prediction mappings
        CreateMap<PredictionResult, PredictionResultDto>();
    }

    private Pace CreatePaceFromDistanceAndTime(double distanceInKm, int movingTimeSeconds)
    {
        if (distanceInKm <= 0 || movingTimeSeconds <= 0)
            return null; // oder einen Default-Pace

        // Berechne Sekunden pro Kilometer
        double secondsPerKm = movingTimeSeconds / distanceInKm;

        // Erstelle Pace-Value-Object
        return new Pace(TimeSpan.FromSeconds(secondsPerKm));
    }
}
