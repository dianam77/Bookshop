using System.ComponentModel.DataAnnotations;

namespace Bookshop.Models
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name ="Confrim Password")]
        [Compare("Password",ErrorMessage ="the password and confrimation password does not match. ")]
        public string ConfrimPassword { get; set; }


        public int MyProperty3 { get; set; }
    }
}
