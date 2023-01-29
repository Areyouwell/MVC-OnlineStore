using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVC_Store.Models.ViewModels.Account
{
    public class LoginUserVM
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }
        [DisplayName("Remember Me")]
        public bool RememberMe { get; set; }
    }
}
