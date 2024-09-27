using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Service
{
    public class LdapService : ILdapService
    {
        private const string ADAllEnabledUserFilter = "(&(objectCategory=person)(objectClass=user)(!(memberOf:1.2.840.113556.1.4.1941:=OU=DisabledUsers,DC=library,DC=local))(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";
        private const string ADDepartment = "department";
        private const string ADDescription = "description";
        private const string ADDirectReports = "directReports";
        private const string ADDisplayName = "displayName";
        private const string ADDistinguishedName = "distinguishedName";
        private const string ADEmployeeNumber = "employeeNumber";
        private const string ADExtensionDate = "extensionAttribute1";
        private const string ADGivenName = "givenName";
        private const string ADMail = "mail";
        private const string ADMailAlias = "proxyAddresses";
        private const string ADMobileNumber = "mobile";
        private const string ADPhysicalDeliveryOfficeName = "physicalDeliveryOfficeName";
        private const string ADsAMAccountName = "sAMAccountName";
        private const string ADTelephoneNumber = "telephoneNumber";
        private const string ADTitle = "title";
        private const int LDAPSearchResponse = 4;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        private readonly string[] AttributesToReturn = {
            ADDepartment,
            ADDescription,
            ADDirectReports,
            ADDisplayName,
            ADDistinguishedName,
            ADEmployeeNumber,
            ADExtensionDate,
            ADGivenName,
            ADMail,
            ADMailAlias,
            ADMobileNumber,
            ADPhysicalDeliveryOfficeName,
            ADsAMAccountName,
            ADTelephoneNumber,
            "memberOf",
            ADTitle
        };

        public LdapService(ILogger<LdapService> logger,
            IConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public static void ApplyLdapAttributes(LdapAttributeSet ldapAttributes, User user)
        {
            if (ldapAttributes == null)
            {
                throw new ArgumentNullException(nameof(ldapAttributes));
            }
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            foreach (var attribute in ldapAttributes)
            {
                switch (attribute.Name)
                {
                    case ADDepartment:
                        user.Department = attribute.StringValue.TruncateTo(255)?.Trim();
                        break;

                    case ADDescription:
                        user.Description = attribute.StringValue.TruncateTo(255)?.Trim();
                        break;

                    case ADDirectReports:
                        foreach (var item in attribute.StringValueArray)
                        {
                            user.DirectReportDNs.Add(item);
                        }
                        break;

                    case ADDisplayName:
                        user.Name = attribute.StringValue.TruncateTo(255)?.Trim();
                        break;

                    case ADDistinguishedName:
                        user.DistinguishedName = attribute.StringValue;
                        break;

                    case ADEmployeeNumber:
                        if (int.TryParse(attribute.StringValue, out var employeeNumber))
                        {
                            user.EmployeeId = employeeNumber;
                        }
                        break;

                    case ADExtensionDate:
                        if (DateTime.TryParse(attribute.StringValue, out var dateValue))
                        {
                            user.ServiceStartDate = dateValue;
                        }
                        break;

                    case ADGivenName:
                        if (string.IsNullOrWhiteSpace(user.Nickname))
                        {
                            user.Nickname = attribute.StringValue.TruncateTo(255)?.Trim();
                        }
                        break;

                    case ADMail:
                        user.Email = attribute.StringValue.TruncateTo(255)?.Trim();
                        break;

                    case ADMobileNumber:
                        user.Mobile = attribute.StringValue.TruncateTo(255)?.Trim();
                        break;

                    case ADPhysicalDeliveryOfficeName:
                        user.PhysicalDeliveryOfficeName
                            = attribute.StringValue.TruncateTo(255)?.Trim();
                        break;

                    case ADsAMAccountName:
                        if (string.IsNullOrEmpty(user.Username))
                        {
                            user.Username = attribute.StringValue.TruncateTo(255)?.Trim();
                        }
                        break;

                    case ADTelephoneNumber:
                        user.Phone = attribute.StringValue.TruncateTo(255)?.Trim();
                        break;

                    case ADTitle:
                        user.Title = attribute.StringValue.TruncateTo(255)?.Trim();
                        break;
                }
            }
        }

        public IEnumerable<string> GetAllLocations()
        {
            var stopwatch = Stopwatch.StartNew();
            var locations = new List<string>();

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
                if (!string.IsNullOrEmpty(_config[Configuration.OpsLdapPort])
                    && !int.TryParse(_config[Configuration.OpsLdapPort], out port))
                {
                    _logger.LogWarning("Invalid port specified: {InvalidPort}",
                        _config[Configuration.OpsLdapPort]);
                }
                using var cn = new LdapConnection();
                try
                {
                    cn.Connect(ldapServer, port);
                    cn.Bind(ldapDn, ldapPassword);

                    var queue = cn.Search(ldapSearchBase,
                        LdapConnection.ScopeSub,
                        ADAllEnabledUserFilter,
                        new[] { ADPhysicalDeliveryOfficeName },
                        false,
                        null,
                        null);

                    LdapMessage message = queue.GetResponse();
                    while (message.Type == LDAPSearchResponse)
                    {
                        var entry = ((LdapSearchResult)message).Entry;
                        try
                        {
                            var attribute = entry.GetAttribute(ADPhysicalDeliveryOfficeName);
                            if (!string.IsNullOrEmpty(attribute.StringValue)
                                && !locations.Contains(attribute.StringValue.Trim()))
                            {
                                locations.Add(attribute.StringValue.Trim());
                            }
                        }
                        catch (KeyNotFoundException knfex)
                        {
                            _logger.LogError("Could not find {Key} in record {Entry}: {ErrorMessage}",
                                ADPhysicalDeliveryOfficeName,
                                entry.Dn,
                                knfex.Message);
                        }
                        message = queue.GetResponse();
                    }
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
            stopwatch.Stop();
            _logger.LogInformation("LDAP lookup found {ResultCount} distinct results in {ElapsedMs} ms",
                locations.Count,
                stopwatch.ElapsedMilliseconds);

            return locations;
        }

        public IEnumerable<User> GetAllUsers()
        {
            return GetUsers(ADAllEnabledUserFilter, null);
        }

        public User LookupByEmail(User user)
        {
            // Lookup non-disabled account by email
            var filter = $"(&(|({ADMail}={user.Email})({ADMailAlias}=smtp:{user.Email}))(!(UserAccountControl:1.2.840.113556.1.4.803:=2)))";
            return Lookup(user, filter);
        }

        public User LookupByUsername(User user)
        {
            // Lookup non-disabled account by username
            var filter = $"(&({ADsAMAccountName}={user.Username})(!(UserAccountControl:1.2.840.113556.1.4.803:=2)))";
            return Lookup(user, filter);
        }

        private IEnumerable<User> GetUsers(string filter, User user)
        {
            var stopwatch = Stopwatch.StartNew();
            var now = DateTime.Now;
            var users = new List<User>();

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
                if (!string.IsNullOrEmpty(_config[Configuration.OpsLdapPort])
                    && !int.TryParse(_config[Configuration.OpsLdapPort], out port))
                {
                    _logger.LogWarning("Invalid port specified: {InvalidPort}",
                        _config[Configuration.OpsLdapPort]);
                }
                using var cn = new LdapConnection();
                try
                {
                    var constraints = user != null
                        ? new LdapSearchConstraints { MaxResults = 1 }
                        : null;

                    cn.Connect(ldapServer, port);
                    cn.Bind(ldapDn, ldapPassword);

                    var queue = cn.Search(ldapSearchBase,
                        LdapConnection.ScopeSub,
                        filter,
                        AttributesToReturn,
                        false,
                        null,
                        constraints);

                    LdapMessage message = queue.GetResponse();
                    while (message.Type == LDAPSearchResponse)
                    {
                        user ??= new User();
                        var entry = ((LdapSearchResult)message).Entry;
                        ApplyLdapAttributes(entry.GetAttributeSet(), user);
                        user.LastLdapCheck = now;
                        users.Add(user);
                        user = null;
                        message = queue.GetResponse();
                    }
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
            stopwatch.Stop();
            _logger.LogInformation("LDAP lookup found {ResultCount} results in {ElapsedMs} ms",
                users.Count,
                stopwatch.ElapsedMilliseconds);

            return users;
        }

        private User Lookup(User user, string filter)
        {
            return GetUsers(filter, user).SingleOrDefault();
        }
    }
}