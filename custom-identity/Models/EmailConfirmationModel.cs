using Microsoft.AspNetCore.Mvc;

namespace custom_identity.Models
{
    public class EmailConfirmationModel
    {
        [TempData]
        public string? StatusMessage { get; set; } = default!;
    }
}
