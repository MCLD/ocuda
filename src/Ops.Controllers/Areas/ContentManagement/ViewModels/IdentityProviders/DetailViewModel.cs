using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.IdentityProviders
{
    public class DetailViewModel
    {
        [DisplayName("Assertion Consumer URL - where the provider will submit the SAML response")]
        [Required]
        public string AssertionConsumerLink { get; set; }

        [Required]
        public string Certificate { get; set; }

        [DisplayName("Endpoint link - receives the login request on the provider")]
        public string EndpointLink { get; set; }

        [DisplayName("Service Provider Entity ID")]
        [Required]
        public string EntityId { get; set; }

        [DisplayName("Name - shown in login form button as a clickable option")]
        [Required]
        public string Name { get; set; }

        [DisplayName("Slug - a short name for the Assertion Consumer Link to indicate which provider this is, keep it alphanumeric")]
        [Required]
        public string Slug { get; set; }
    }
}