using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace custom_identity.Models.Partials
{
    public class ProfilePartialModel
    {
        public string Username { get; set; } = default!;

        [TempData]
        public string StatusMessage { get; set; } = default!;

        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; } = default!;
    }
}
