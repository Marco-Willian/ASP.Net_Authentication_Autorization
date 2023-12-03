using System.ComponentModel.DataAnnotations;

namespace JwtAuth.Core.Dtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "User Name is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
