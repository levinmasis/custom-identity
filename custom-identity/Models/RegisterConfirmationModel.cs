using Microsoft.AspNetCore.Mvc;

namespace custom_identity.Models
{
    public class RegisterConfirmationModel
    {
        [TempData]
        public string? StatusMessage { get; set; }
    }
}
