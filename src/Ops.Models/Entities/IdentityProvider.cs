namespace Ocuda.Ops.Models.Entities
{
    public class IdentityProvider : Abstract.BaseEntity
    {
        /// <summary>
        /// Whether or not this identity provider is active for the site
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// The provider's assertion consumer URL, where the provider redirects
        /// </summary>
        public string AssertionConsumerLink { get; set; }

        /// <summary>
        /// The encrypted certificate
        /// </summary>
        public string EncryptedCertificate { get; set; }

        /// <summary>
        /// The app's entity id as configured with the provider
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// The name for administrators to identify the provdier by
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The provider's endpoint link, usually receives the HTTP POST for authentication
        /// </summary>
        public string PostLink { get; set; }

        /// <summary>
        /// A <see cref="IdentityProviderType"/> specifying the provdier type
        /// </summary>
        public IdentityProviderType ProviderType { get; set; }
    }
}