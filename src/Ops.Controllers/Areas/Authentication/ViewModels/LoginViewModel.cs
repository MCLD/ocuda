using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.Authentication.ViewModels
{
    public class LoginViewModel
    {
        public string AdminEmail { get; set; }
        public IEnumerable<IdentityProvider> IdentityProviders { get; set; }

        [Required]
        public string Password { get; set; }

        public string ReturnUrl { get; set; }

        [Required]
        public string Username { get; set; }
    }
}