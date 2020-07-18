using System.ComponentModel.DataAnnotations;

namespace DatingApp.Api.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [StringLength(16, MinimumLength = 8, ErrorMessage = "The password must have between 8 and 16 characters long.")]
        public string Password { get; set; }
    }
}