using Microsoft.AspNetCore.Identity;
using Sakan.Domain.Enums;

namespace Sakan.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string? AvatarUrl { get; set; }
        public UserIntent? OnboardingIntent { get; set; }
        public bool OnboardingCompleted { get; set; }
    }
}
