using FitnessAnalyticsHub.Application.DTOs;
using FitnessAnalyticsHub.Application.Interfaces;
using FitnessAnalyticsHub.Domain.Interfaces;
using FitnessAnalyticsHub.Domain.Entities;
using AutoMapper;
using FitnessAnalyticsHub.Domain.Exceptions.Athletes;

namespace FitnessAnalyticsHub.Application.Services
{
    public class AthleteService : IAthleteService
    {
        private readonly IRepository<Athlete> _athleteRepository;
        private readonly IStravaService _stravaService;
        private readonly IMapper _mapper;

        public AthleteService(
            IRepository<Athlete> athleteRepository,
            IStravaService stravaService,
            IMapper mapper)
        {
            _athleteRepository = athleteRepository;
            _stravaService = stravaService;
            _mapper = mapper;
        }

        public async Task<AthleteDto> GetAthleteByIdAsync(int id) 
        {
            var athlete = await _athleteRepository.GetByIdAsync(id);
            if (athlete == null)
                throw new AthleteNotFoundException(id); 

            return _mapper.Map<AthleteDto>(athlete);
        }

        public async Task<IEnumerable<AthleteDto>> GetAllAthletesAsync()
        {
            var athletes = await _athleteRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<AthleteDto>>(athletes);
        }

        public async Task<AthleteDto> CreateAthleteAsync(CreateAthleteDto athleteDto)
        {
            var athlete = _mapper.Map<Athlete>(athleteDto);
            await _athleteRepository.AddAsync(athlete);
            await _athleteRepository.SaveChangesAsync();
            return _mapper.Map<AthleteDto>(athlete);
        }

        public async Task UpdateAthleteAsync(UpdateAthleteDto athleteDto)
        {
            var athlete = await _athleteRepository.GetByIdAsync(athleteDto.Id);
            if (athlete == null)
                throw new AthleteNotFoundException(athleteDto.Id);

            _mapper.Map(athleteDto, athlete);
            athlete.UpdatedAt = DateTime.Now;

            await _athleteRepository.UpdateAsync(athlete);
            await _athleteRepository.SaveChangesAsync();
        }

        public async Task DeleteAthleteAsync(int id)
        {
            var athlete = await _athleteRepository.GetByIdAsync(id);
            if (athlete == null)
                throw new AthleteNotFoundException(id);

            await _athleteRepository.DeleteAsync(athlete);
            await _athleteRepository.SaveChangesAsync();
        }

        public async Task<AthleteDto> ImportAthleteFromStravaAsync(string accessToken)
        {
            var stravaAthlete = await _stravaService.GetAthleteProfileAsync(accessToken);

            // Check if athlete already exists
            var existingAthletes = await _athleteRepository.FindAsync(a => a.StravaId == stravaAthlete.StravaId);
            var existingAthlete = existingAthletes.FirstOrDefault();

            if (existingAthlete != null)
            {
                // Update existing athlete
                existingAthlete.FirstName = stravaAthlete.FirstName;
                existingAthlete.LastName = stravaAthlete.LastName;
                existingAthlete.Username = stravaAthlete.Username;
                existingAthlete.Email = stravaAthlete.Email;
                existingAthlete.City = stravaAthlete.City;
                existingAthlete.Country = stravaAthlete.Country;
                existingAthlete.ProfilePictureUrl = stravaAthlete.ProfilePictureUrl;
                existingAthlete.UpdatedAt = DateTime.Now;

                await _athleteRepository.UpdateAsync(existingAthlete);
                await _athleteRepository.SaveChangesAsync();

                return _mapper.Map<AthleteDto>(existingAthlete);
            }
            else
            {
                // Create new athlete
                var newAthlete = new Athlete
                {
                    StravaId = stravaAthlete.StravaId,
                    FirstName = stravaAthlete.FirstName,
                    LastName = stravaAthlete.LastName,
                    Username = stravaAthlete.Username,
                    Email = stravaAthlete.Email,
                    City = stravaAthlete.City,
                    Country = stravaAthlete.Country,
                    ProfilePictureUrl = stravaAthlete.ProfilePictureUrl,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _athleteRepository.AddAsync(newAthlete);
                await _athleteRepository.SaveChangesAsync();

                return _mapper.Map<AthleteDto>(newAthlete);
            }
        }
    }
}
