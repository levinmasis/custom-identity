using System.ComponentModel.DataAnnotations;

namespace custom_identity.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = default!;
    }
}
