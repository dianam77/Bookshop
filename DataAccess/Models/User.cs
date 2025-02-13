using Microsoft.AspNetCore.Identity;


namespace DataAccess.Models
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
    }
}
