using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Keys;
using Serilog.Context;

namespace Ocuda.Ops.Service
{
    public class LdapService : ILdapService
    {
        /// <summary>
        /// The LDAP filter to return all enabled users. Exclsued members of the DisabledUsers OU
        /// </summary>
        private const string ADAllEnabledUserFilter = "(&(objectCategory=person)(objectClass=user)(!(memberOf:1.2.840.113556.1.4.1941:=OU=DisabledUsers,DC=library,DC=local))(!(userAccountControl:1.2.840.113556.1.4.803:=2)))";

        private const string ADDepartment = "department";
        private const string ADDescription = "description";
        private const string ADDirectReports = "directreports";
        private const string ADDisplayName = "displayname";
        private const string ADDistinguishedName = "distinguishedname";
        private const string ADEmployeeNumber = "employeenumber";
        private const string ADExtensionDate = "extensionattribute1";
        private const string ADGivenName = "givenname";

        /// <summary>
        /// The name of "Distribution Groups" in LDAP
        /// </summary>
        private const string ADGroupDistributionGroup = "Distribution Groups";

        /// <summary>
        /// The parent AD Group which contains the Distribution Groups and Security Groups
        /// </summary>
        private const string ADGroupParentOu = "Groups";

        /// <summary>
        /// The name of "Security Groups" in LDAP
        /// </summary>
        private const string ADGroupSecurityGroup = "Security Groups";

        private const string ADMail = "mail";
        private const string ADMailAlias = "proxyaddresses";
        private const string ADMemberOf = "memberof";
        private const string ADMobileNumber = "mobile";
        private const string ADPhysicalDeliveryOfficeName = "physicaldeliveryofficename";
        private const string ADsAMAccountName = "samaccountname";
        private const string ADTelephoneNumber = "telephonenumber";
        private const string ADTitle = "title";
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        /// <summary>
        /// Default set of attributes to return on an LDAP query
        /// </summary>
        private readonly string[] AttributesToReturn = [
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
            ADMemberOf,
            ADMobileNumber,
            ADPhysicalDeliveryOfficeName,
            ADsAMAccountName,
            ADTelephoneNumber,
            ADTitle
        ];

        public LdapService(ILogger<LdapService> logger, IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(logger);

            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// The kinds of AD groups we want to potentially carry back to users
        /// </summary>
        private enum AdGroupType

        {
            DistributionGroup,
            SecurityGroup
        }

        /// <summary>
        /// Return a distinct list of all physicalDeliveryOfficeName LDAP data points. Note that
        /// this list is acquired by querying all users which may not be the most efficient
        /// means.
        /// </summary>
        /// <returns>A string <see cref=">IEnumberable"/> of physicalDeliveryOfficeNames.</returns>
        public IEnumerable<string> GetAllLocations()
        {
            var users = LdapUserLookup(ADAllEnabledUserFilter, null, [ADPhysicalDeliveryOfficeName]);
            return users.Where(_ => !string.IsNullOrEmpty(_.PhysicalDeliveryOfficeName)).
                Select(_ => _.PhysicalDeliveryOfficeName)
                .Distinct();
        }

        /// <summary>
        /// Return a list of all enabled AD users with all available attributes.
        /// </summary>
        /// <returns>An <see cref="IEnumerable"/> of <see cref="User"/> items populated with
        /// LDAP information.</returns>
        public IEnumerable<User> GetAllUsers()
        {
            return LdapUserLookup(ADAllEnabledUserFilter, null, AttributesToReturn);
        }

        /// <summary>
        /// Look up a particular user by their email address (as specified in the user object which
        /// is provided). This will error if multiple users are returned from this query.
        /// </summary>
        /// <param name="user">A <see cref="User"/> with the email address populated.</param>
        /// <returns>The <see cref="User"/> with LDAP information added.</returns>
        public User LookupByEmail(User user)
        {
            ArgumentNullException.ThrowIfNull(user);
            var filter = $"(&(|({ADMail}={user.Email})({ADMailAlias}=smtp:{user.Email}))(!(UserAccountControl:1.2.840.113556.1.4.803:=2)))";
            return LdapUserLookup(filter, user, AttributesToReturn).SingleOrDefault();
        }

        /// <summary>
        /// Look up a particular user by their username (as specified in the user object which is
        /// provided). This will error if multiple users are returned from this query.
        /// </summary>
        /// <param name="user">A <see cref="User"/> with a populated Username.</param>
        /// <returns>The <see cref="User"/> with LDAP information added.</returns>
        public User LookupByUsername(User user)
        {
            ArgumentNullException.ThrowIfNull(user);
            var filter = $"(&({ADsAMAccountName}={user.Username})(!(UserAccountControl:1.2.840.113556.1.4.803:=2)))";
            return LdapUserLookup(filter, user, AttributesToReturn).SingleOrDefault();
        }

        /// <summary>
        /// When provided with an LDAP DN (as as string), establish if it belongs in the top level
        /// specified parent group and return all Common Names along with the type of group that it
        /// is.
        /// </summary>
        /// <param name="attributeString">The LDAP DN as a string</param>
        /// <param name="groupParent">The parent OU of Distirbution Groups and
        /// Security Groups</param>
        /// <returns>An <see cref="AdGroup"/> with the group information</returns>
        /// <exception cref="InvalidDataException">Thrown when bad group data comes back, usually
        /// meaning that the group has the wrong parent group or is not an known group type.
        /// </exception>
        private static AdGroup ExtractGroupDetails(string attributeString, string groupParent)
        {
            var adGroup = new AdGroup();

            int ouCount = 0;

            foreach (var rdn in new CPI.DirectoryServices.DN(attributeString).RDNs.Reverse())
            {
                foreach (var element in rdn.Components)
                {
                    if (element.ComponentType.Equals("CN", StringComparison.OrdinalIgnoreCase))
                    {
                        adGroup.Names.Add(element.ComponentValue);
                    }
                    else if (element.ComponentType.Equals("OU", StringComparison.OrdinalIgnoreCase))
                    {
                        if (ouCount == 0 && !element.ComponentValue.Equals(groupParent, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidDataException($"Group does not have required parent group: {groupParent}");
                        }
                        else if (ouCount == 1)
                        {
                            if (element.ComponentValue.Equals(ADGroupDistributionGroup, StringComparison.OrdinalIgnoreCase))
                            {
                                if (adGroup.GroupType.HasValue)
                                {
                                    throw new InvalidDataException("Invalid group: applies to multiple group types.");
                                }
                                else
                                {
                                    adGroup.GroupType = AdGroupType.DistributionGroup;
                                }
                            }
                            else if (element.ComponentValue.Equals(ADGroupSecurityGroup, StringComparison.OrdinalIgnoreCase))
                            {
                                if (adGroup.GroupType.HasValue)
                                {
                                    throw new InvalidDataException("Invalid group: applies to multiple group types.");
                                }
                                else
                                {
                                    adGroup.GroupType = AdGroupType.SecurityGroup;
                                }
                            }
                            else
                            {
                                throw new InvalidDataException($"Invalid group type found: {element.ComponentValue}");
                            }
                        }
                        ouCount++;
                    }
                }
            }

            return adGroup;
        }

        /// <summary>
        /// Apply LDAP attributes to the provided <see cref="User"/> object.
        /// </summary>
        /// <param name="attributes">LDAP attributes retreived from a query</param>
        /// <param name="user">A <see cref=">User"/> populated with non-LDAP application
        /// information</param>
        /// <param name="groupNamePrefix">A prefix to preprend to group names (typically for
        /// Active Directory this is the domain name)</param>
        private void ApplyLdapAttributes(SearchResultAttributeCollection attributes,
            User user,
            string groupNamePrefix)
        {
            ArgumentNullException.ThrowIfNull(attributes);
            ArgumentNullException.ThrowIfNull(user);

            foreach (System.Collections.DictionaryEntry attributeEntry in attributes)
            {
                if (attributeEntry.Value is not DirectoryAttribute attribute)
                {
                    _logger.LogError("Unable to view LDAP attribute {AttributeKey}",
                        attributeEntry.Key);
                    continue;
                }

                switch (attributeEntry.Key)
                {
                    case ADDepartment:
                        user.Department = attribute[0]?.ToString()?.Trim()?.TruncateTo(255);
                        break;

                    case ADDescription:
                        user.Description = attribute[0]?.ToString()?.Trim()?.TruncateTo(255);
                        break;

                    case ADDirectReports:
                        for (int i = 0; i < attribute.Count; i++)
                        {
                            user.DirectReportDNs.Add(attribute[i]?
                                .ToString()?
                                .Trim()?
                                .TruncateTo(255));
                        }
                        break;

                    case ADDisplayName:
                        user.Name = attribute[0]?.ToString()?.Trim()?.TruncateTo(255);
                        break;

                    case ADDistinguishedName:
                        user.DistinguishedName = attribute[0]?.ToString()?.Trim()?.TruncateTo(255);
                        break;

                    case ADEmployeeNumber:
                        if (int.TryParse(attribute[0]?.ToString()?.Trim()?.TruncateTo(255),
                                out var employeeNumber))
                        {
                            user.EmployeeId = employeeNumber;
                        }
                        break;

                    case ADExtensionDate:
                        if (DateTime.TryParse(attribute[0]?.ToString()?.Trim()?.TruncateTo(255),
                                out var dateValue))
                        {
                            user.ServiceStartDate = dateValue;
                        }
                        break;

                    case ADGivenName:
                        if (string.IsNullOrWhiteSpace(user.Nickname))
                        {
                            user.Nickname = attribute[0]?.ToString()?.Trim()?.TruncateTo(255);
                        }
                        break;

                    case ADMail:
                        user.Email = attribute[0]?.ToString()?.Trim()?.TruncateTo(255);
                        break;

                    case ADMobileNumber:
                        user.Mobile = attribute[0]?.ToString()?.Trim()?.TruncateTo(255);
                        break;

                    case ADPhysicalDeliveryOfficeName:
                        user.PhysicalDeliveryOfficeName = attribute[0]?
                            .ToString()?
                            .Trim()?
                            .TruncateTo(255);
                        break;

                    case ADsAMAccountName:
                        if (string.IsNullOrEmpty(user.Username))
                        {
                            user.Username = attribute[0]?.ToString()?.Trim()?.TruncateTo(255);
                        }
                        break;

                    case ADTelephoneNumber:
                        user.Phone = attribute[0]?.ToString()?.Trim()?.TruncateTo(255);
                        break;

                    case ADTitle:
                        user.Title = attribute[0]?.ToString()?.Trim()?.TruncateTo(255);
                        break;

                    case ADMemberOf:
                        for (int i = 0; i < attribute.Count; i++)
                        {
                            var attributeString = attribute[i] as string;

                            if (string.IsNullOrEmpty(attributeString))
                            {
                                _logger.LogWarning("Group name {GroupName} could not be converted to a string, ignoring",
                                    Encoding.ASCII.GetString((byte[])attribute[i]));
                            }
                            else
                            {
                                try
                                {
                                    var group = ExtractGroupDetails(attributeString,
                                        ADGroupParentOu);
                                    if (group.GroupType == AdGroupType.DistributionGroup)
                                    {
                                        foreach (var groupName in group.Names)
                                        {
                                            user.DistributionGroups
                                                .Add($"{groupNamePrefix}{groupName}");
                                        }
                                    }
                                    else if (group.GroupType == AdGroupType.SecurityGroup)
                                    {
                                        foreach (var groupName in group.Names)
                                        {
                                            user.SecurityGroups
                                                .Add($"{groupNamePrefix}{groupName}");
                                        }
                                    }
                                }
                                catch (InvalidDataException idex)
                                {
                                    _logger.LogError(idex,
                                        "Invalid data extracting group info: {ErrorMessage}",
                                        idex.Message);
                                }
                            }
                        }
                        break;

                    case ADMailAlias:
                        break;

                    default:
                        _logger.LogTrace("Unused LDAP property supplied: {Key} = {Value}",
                            attributeEntry.Key,
                            attribute[0]);
                        break;
                }
            }
        }

        /// <summary>
        /// Using the supplied LDAP Filter, look up one or more users and either apply the supplied
        /// LDAP attributes to the provided <see cref="User"/> or return an
        /// <see cref="IEnumerable"/> of all that match.
        /// </summary>
        /// <param name="filter">LDAP filter to limit user selection</param>
        /// <param name="user">A <see cref=">User"/> object to start with if this is a single
        /// user lookup, otherwise this can be null</param>
        /// <param name="attributesToPopulate">Which LDAP attributes to populate. Limit this to
        /// limit the data returned from the LDAP server</param>
        /// <returns>An <see cref="List"/> of <see cref="User"/> objects macthing the filter
        /// and populated with LDAP data</returns>
        private List<User> LdapUserLookup(string filter,
            User user,
            string[] attributesToPopulate)
        {
            var stopwatch = Stopwatch.StartNew();
            var now = DateTime.Now;
            var users = new List<User>();

            using (LogContext.PushProperty("LdapFilter", filter))
            using (LogContext.PushProperty("LdapRequestedAttributes", attributesToPopulate, true))
            {
                string domainName = _config[Configuration.OpsDomainName];
                string ldapPassword = _config[Configuration.OpsLdapPassword];
                string ldapSearchBase = _config[Configuration.OpsLdapSearchBase];
                string ldapServer = _config[Configuration.OpsLdapServer];
                string ldapUser = _config[Configuration.OpsLdapUser];

                if (!string.IsNullOrEmpty(ldapServer)
                    && !string.IsNullOrEmpty(ldapUser)
                    && !string.IsNullOrEmpty(ldapPassword)
                    && !string.IsNullOrEmpty(ldapSearchBase))
                {
                    var endpoint = new LdapDirectoryIdentifier(ldapServer);
                    var credential = new NetworkCredential(ldapUser, ldapPassword, domainName);

                    using var connection = new LdapConnection(endpoint, credential)
                    {
                        AuthType = AuthType.Basic,
                    };

                    try
                    {
                        connection.Bind();

                        var searchRequest = new SearchRequest(ldapSearchBase,
                            filter,
                            SearchScope.Subtree,
                            attributesToPopulate)
                            ?? throw new OcudaException($"Unable to create LDAP Search request from LDAP base {ldapSearchBase} filter {filter} attributes {string.Join(',', attributesToPopulate)}");

                        var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);

                        foreach (SearchResultEntry entry in searchResponse.Entries)
                        {
                            user ??= new User();
                            ApplyLdapAttributes(entry.Attributes, user, $"{domainName}\\");
                            user.LastLdapCheck = now;
                            users.Add(user);
                            user = null;
                        }
                    }
                    catch (Exception ex) when (ex is InvalidOperationException
                        || ex is LdapException
                        || ex is NotSupportedException
                        || ex is ObjectDisposedException)
                    {
                        _logger.LogError(ex,
                            "Problem retreiving LDAP data from {LDAPServer}: {ErrorMessage}",
                            ldapServer,
                            ex.Message);
                    }
                }
                else
                {
                    _logger.LogError("Missing LDAP configuration values, please ensure {LdapServer}, {LdapUser}, {LdapPassword}, and {LdapSearchBase} are configured.",
                        Configuration.OpsLdapServer,
                        Configuration.OpsLdapUser,
                        Configuration.OpsLdapPassword,
                        Configuration.OpsLdapSearchBase);
                }
                stopwatch.Stop();

                if (users?.Count == 1)
                {
                    _logger.LogInformation("LDAP lookup found {ResultCount} result: user {User} with {SecurityGroups} security groups in {ElapsedMs} ms",
                        users.Count,
                        users.FirstOrDefault()?.Username,
                        users.FirstOrDefault()?.SecurityGroups.Count,
                        stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogInformation("LDAP lookup found {ResultCount} result(s) in {ElapsedMs} ms",
                        users.Count,
                        stopwatch.ElapsedMilliseconds);
                }

                return users;
            }
        }

        /// <summary>
        /// Object representing an Active Directory Group that we've looked up.
        /// </summary>
        private class AdGroup
        {
            public AdGroup()
            {
                Names = [];
            }

            public AdGroupType? GroupType { get; set; }
            public IList<string> Names { get; }
        }
    }
}