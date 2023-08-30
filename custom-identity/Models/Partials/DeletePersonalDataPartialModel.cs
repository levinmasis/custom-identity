using System.ComponentModel.DataAnnotations;

namespace custom_identity.Models.Partials
{
    public class DeletePersonalDataPartialModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        public bool RequirePassword { get; set; } = true;
    }
}
