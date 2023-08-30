using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace custom_identity.Models.Partials
{
    public class EmailPartialModel
    {
        public string Email { get; set; } = default!;

        public bool IsEmailConfirmed { get; set; } = default!;

        [TempData]
        public string StatusMessage { get; set; } = default!;
        [Required]
        [EmailAddress]
        [Display(Name = "New email")]
        public string NewEmail { get; set; } = default!;
    }
}
