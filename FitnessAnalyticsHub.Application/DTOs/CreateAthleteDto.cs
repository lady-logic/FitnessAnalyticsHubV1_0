using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessAnalyticsHub.Application.DTOs
{
    public class CreateAthleteDto
    {
        public string? StravaId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
