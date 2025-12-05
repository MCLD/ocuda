namespace Ocuda.Ops.Models.Entities
{
    public class IdentityProvider : Abstract.BaseEntity
    {
        /// <summary>
        /// The provider's assertion consumer URL, where the provider redirects
        /// </summary>
        public string AssertionConsumerLink { get; set; }

        /// <summary>
        /// The encrypted certificate
        /// </summary>
        public string EncryptedCertificate { get; set; }

        /// <summary>
        /// The provider's endpoint link, receives the login request on the provider
        /// </summary>
        public string EndpointLink { get; set; }

        /// <summary>
        /// The app's entity id as configured with the provider
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// The form name that the provider will pass back, usually SAMLResponse for SAML
        /// </summary>
        public string FormName { get; set; }

        /// <summary>
        /// Whether or not this identity provider is active for the site
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Try this identity provider by default when authentication is requested
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// The name for administrators to identify the provdier by
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A <see cref="IdentityProviderType"/> specifying the provdier type
        /// </summary>
        public IdentityProviderType ProviderType { get; set; }

        /// <summary>
        /// The provider's slug to use for the Assertion Consumer Link
        /// </summary>
        public string Slug { get; set; }
    }
}