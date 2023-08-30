using Microsoft.AspNetCore.Authentication;
using System.ComponentModel.DataAnnotations;

namespace custom_identity.Models
{
    public class LoginModel
    {
        public IList<AuthenticationScheme> ExternalLogins { get; set; } = default!;

        public string? ReturnUrl { get; set; } = default!;

        [Required]
        public string Username { get; set; } = default!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
