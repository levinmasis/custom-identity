using Microsoft.AspNetCore.Mvc;

namespace custom_identity.Models
{
    public class ConfirmEmailModel
    {
        [TempData]
        public string StatusMessage { get; set; } = default!;
    }
}
