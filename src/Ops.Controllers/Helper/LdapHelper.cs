using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using Ocuda.Ops.Models;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Helper
{
    public class LdapHelper
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public LdapHelper(ILogger<LdapHelper> logger,
            IConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public User Lookup(User user)
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
                using (var cn = new LdapConnection())
                {
                    try
                    {
                        string[] attributesToReturn = {
                                        "sAMAccountName",
                                        "displayName",
                                        "objectGUID",
                                        "logonCount"
                                    };
                        var constraints = new LdapSearchConstraints
                        {
                            MaxResults = 1
                        };
                        cn.Connect(ldapServer, port);
                        cn.Bind(ldapDn, ldapPassword);

                        LdapSearchQueue queue = cn.Search(ldapSearchBase,
                            LdapConnection.SCOPE_SUB,
                            $"(sAMAccountName={user.Username})",
                            null,
                            false,
                            null,
                            constraints);

                        LdapMessage message;
                        while ((message = queue.getResponse()).Type == 4)
                        {
                            var entry = ((LdapSearchResult)message).Entry;
                            var attributes = entry.getAttributeSet().GetEnumerator();
                            while (attributes.MoveNext())
                            {
                                var attribute = (LdapAttribute)attributes.Current;
                                switch (attribute.Name)
                                {
                                    case "displayName":
                                        user.Name = attribute.StringValue;
                                        break;
                                    case "telephoneNumber":
                                        user.Phone = attribute.StringValue;
                                        break;
                                    case "title":
                                        if (string.IsNullOrEmpty(user.Title))
                                        {
                                            user.Title = attribute.StringValue;
                                        }
                                        break;
                                    case "mail":
                                        user.Email = attribute.StringValue;
                                        break;
                                    case "givenName":
                                        if (string.IsNullOrWhiteSpace(user.Nickname))
                                        {
                                            user.Nickname = attribute.StringValue;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        cn.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Problem connecting to LDAP: {Message}", ex);
                    }
                }
            }
            return user;
        }
    }
}
