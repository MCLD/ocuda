using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Controllers.Areas.Authentication.ViewModels
{
    public class LoginViewModel
    {
        public LoginViewModel()
        {
            ProviderLinks = new Dictionary<string, string>();
        }

        public string AuthenticatedUser { get; set; }

        public string AdminEmail { get; set; }

        [Required]
        public string Password { get; set; }

        public IDictionary<string, string> ProviderLinks { get; }
        public string ReturnUrl { get; set; }

        [Required]
        [DisplayName("Windows login")]
        public string Username { get; set; }
    }
}