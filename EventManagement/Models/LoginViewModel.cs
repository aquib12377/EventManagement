using System.ComponentModel.DataAnnotations;

namespace EventManagement.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Mobile number is required.")]
        [Phone(ErrorMessage = "Invalid mobile number.")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
