using AIAssistant.Domain.Models;
using FitnessAnalyticsHub.AIAssistant.Application.DTOs;

namespace FitnessAnalyticsHub.AIAssistant.Extensions;

public static class AthleteProfileExtensions
{
    // Domain → DTO (für Services die Domain Models zurückgeben)
    public static AthleteProfileDto ToDto(this AthleteProfile domain)
    {
        return new AthleteProfileDto
        {
            Id = domain.Id,
            Name = domain.Name,
            FitnessLevel = domain.FitnessLevel,
            PrimaryGoal = domain.PrimaryGoal,
            Preferences = domain.Preferences
        };
    }

    // DTO → Domain (für Services die Domain Models erwarten)
    public static AthleteProfile ToDomain(this AthleteProfileDto dto)
    {
        return new AthleteProfile
        {
            Id = dto.Id ?? Guid.NewGuid().ToString(),
            Name = dto.Name,
            FitnessLevel = dto.FitnessLevel,
            PrimaryGoal = dto.PrimaryGoal,
            Preferences = dto.Preferences
        };
    }

    // GrpcJson → DTO (für deinen Controller)
    public static AthleteProfileDto ToAthleteProfileDto(this GrpcJsonAthleteProfileDto grpcProfile)
    {
        return new AthleteProfileDto
        {
            Id = Guid.NewGuid().ToString(),  // Neue ID generieren
            Name = grpcProfile.Name ?? "",
            FitnessLevel = grpcProfile.FitnessLevel,
            PrimaryGoal = grpcProfile.PrimaryGoal,
            Preferences = null  // gRPC-JSON hat keine Preferences
        };
    }

    public static AthleteProfileDto ToAthleteProfileDto(
    this global::Fitnessanalyticshub.AthleteProfile grpcProfile)
    {
        return new AthleteProfileDto
        {
            Id = Guid.NewGuid().ToString(),
            Name = grpcProfile.Name ?? "",
            FitnessLevel = grpcProfile.FitnessLevel ?? "",
            PrimaryGoal = grpcProfile.PrimaryGoal ?? "",
            Preferences = null // gRPC hat keine Preferences
        };
    }
}
