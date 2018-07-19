﻿using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using Ocuda.Ops.Models;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Helpers
{
    public class LdapHelper
    {
        private const int LDAPSearchResponse = 4;

        private const string ADsAMAccountName = "sAMAccountName";
        private const string ADDisplayName = "displayName";
        private const string ADTelephoneNumber = "telephoneNumber";
        private const string ADTitle = "title";
        private const string ADMail = "mail";
        private const string ADGivenName = "givenName";

        private readonly string[] AttributesToReturn = {
            ADsAMAccountName,
            ADDisplayName,
            ADTelephoneNumber,
            ADTitle,
            ADMail,
            ADGivenName
        };

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
                        var constraints = new LdapSearchConstraints
                        {
                            MaxResults = 1
                        };
                        cn.Connect(ldapServer, port);
                        cn.Bind(ldapDn, ldapPassword);

                        LdapSearchQueue queue = cn.Search(ldapSearchBase,
                            LdapConnection.SCOPE_SUB,
                            $"({ADsAMAccountName}={user.Username})",
                            AttributesToReturn,
                            false,
                            null,
                            constraints);

                        LdapMessage message = queue.getResponse();
                        while (message.Type == LDAPSearchResponse)
                        {
                            var entry = ((LdapSearchResult)message).Entry;
                            var attributes = entry.getAttributeSet().GetEnumerator();
                            while (attributes.MoveNext())
                            {
                                var attribute = (LdapAttribute)attributes.Current;
                                switch (attribute.Name)
                                {
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
                            message = queue.getResponse();
                        }
                        user.LastLdapUpdate = DateTime.Now;
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
