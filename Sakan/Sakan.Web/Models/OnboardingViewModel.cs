using System.ComponentModel.DataAnnotations;
using Sakan.Domain.Enums;

namespace Sakan.Web.Models;

public class OnboardingViewModel
{
    [Required(ErrorMessage = "Please select what you want to do first.")]
    public UserIntent? Intent { get; set; }
}
