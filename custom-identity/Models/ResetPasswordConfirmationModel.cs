using Microsoft.AspNetCore.Mvc;

namespace custom_identity.Models
{
    public class ResetPasswordConfirmationModel
    {
        [TempData]
        public string StatusMessage { get; set; } = default!;
    }
}
