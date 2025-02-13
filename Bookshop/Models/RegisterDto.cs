using System.ComponentModel.DataAnnotations;

namespace Bookshop.Models
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name ="Confrim Password")]
        [Compare("Password",ErrorMessage ="the password and confrimation password does not match. ")]
        public string ConfrimPassword { get; set; } = string.Empty;


        public int MyProperty3 { get; set; }
    }
}
