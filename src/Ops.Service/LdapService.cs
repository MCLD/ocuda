using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Service
{
    public class LdapService : ILdapService
    {
        private const int LDAPSearchResponse = 4;

        private const string ADsAMAccountName = "sAMAccountName";
        private const string ADDisplayName = "displayName";
        private const string ADTelephoneNumber = "telephoneNumber";
        private const string ADTitle = "title";
        private const string ADMail = "mail";
        private const string ADMailAlias = "proxyAddresses";
        private const string ADGivenName = "givenName";

        private readonly string[] AttributesToReturn = {
            ADsAMAccountName,
            ADDisplayName,
            ADTelephoneNumber,
            ADTitle,
            ADMail,
            ADMailAlias,
            ADGivenName
        };

        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public LdapService(ILogger<LdapService> logger,
            IConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public User LookupByUsername(User user)
        {
            // Lookup non-disabled account by username
            var filter = $"(&({ADsAMAccountName}={user.Username})(!(UserAccountControl:1.2.840.113556.1.4.803:=2)))";
            return Lookup(user, filter);
        }

        public User LookupByEmail(User user)
        {
            // Lookup non-disabled account by email
            var filter = $"(&(|({ADMail}={user.Email})({ADMailAlias}=smtp:{user.Email}))(!(UserAccountControl:1.2.840.113556.1.4.803:=2)))";
            return Lookup(user, filter);
        }

        private User Lookup(User user, string filter)
        {
            string ldapServer = _config[Configuration.OpsLdapServer];
            string ldapDn = _config[Configuration.OpsLdapDn];
            string ldapPassword = _config[Configuration.OpsLdapPassword];
            string ldapSearchBase = _config[Configuration.OpsLdapSearchBase];

            if (!string.IsNullOrEmpty(ldapServer)
                && !string.IsNullOrEmpty(ldapDn)
                && !string.IsNullOrEmpty(ldapPassword)
                && !string.IsNullOrEmpty(ldapSearchBase))
            {
                int port = 389;
                if (!string.IsNullOrEmpty(_config[Configuration.OpsLdapPort]))
                {
                    int.TryParse(_config[Configuration.OpsLdapPort], out port);
                }
                using var cn = new LdapConnection();
                try
                {
                    var constraints = new LdapSearchConstraints
                    {
                        MaxResults = 1
                    };
                    cn.Connect(ldapServer, port);
                    cn.Bind(ldapDn, ldapPassword);

                    LdapSearchQueue queue = cn.Search(ldapSearchBase,
                        LdapConnection.ScopeSub,
                        filter,
                        AttributesToReturn,
                        false,
                        null,
                        constraints);

                    LdapMessage message = queue.GetResponse();
                    var now = DateTime.Now;
                    while (message.Type == LDAPSearchResponse)
                    {
                        var entry = ((LdapSearchResult)message).Entry;
                        var attributes = entry.GetAttributeSet().GetEnumerator();
                        while (attributes.MoveNext())
                        {
                            var attribute = (LdapAttribute)attributes.Current;
                            switch (attribute.Name)
                            {
                                case ADsAMAccountName:
                                    if (string.IsNullOrEmpty(user.Username))
                                    {
                                        user.Username = attribute.StringValue;
                                    }
                                    break;
                                case ADDisplayName:
                                    user.Name = attribute.StringValue;
                                    break;
                                case ADTelephoneNumber:
                                    user.Phone = attribute.StringValue;
                                    break;
                                case ADTitle:
                                    if (string.IsNullOrEmpty(user.Title))
                                    {
                                        user.Title = attribute.StringValue;
                                    }
                                    break;
                                case ADMail:
                                    user.Email = attribute.StringValue;
                                    break;
                                case ADGivenName:
                                    if (string.IsNullOrWhiteSpace(user.Nickname))
                                    {
                                        user.Nickname = attribute.StringValue;
                                    }
                                    break;
                            }
                        }
                        user.LastLdapUpdate = now;
                        message = queue.GetResponse();
                    }
                    user.LastLdapCheck = now;
                    cn.Disconnect();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Problem connecting to LDAP server {LDAPServer}: {Message}",
                        ldapServer,
                        ex.Message);
                }
            }
            return user;
        }
    }
}
