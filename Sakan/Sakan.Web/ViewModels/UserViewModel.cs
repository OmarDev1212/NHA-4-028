using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Sakan.Web.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}