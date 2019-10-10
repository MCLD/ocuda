using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clc.Polaris.Api.Models
{
    /// <summary>
    /// Result of an OrganizationsGet call
    /// </summary>
	public class OrganizationsGetResult : PapiResponseCommon
	{
        /// <summary>
        /// List of organization data
        /// </summary>
		public List<OrganizationsGetRow> OrganizationsGetRows { get; set; }
	}

    /// <summary>
    /// Organization data
    /// </summary>
	public class OrganizationsGetRow
	{
        /// <summary>
        /// OrganizationID
        /// </summary>
		public int OrganizationID { get; set; }

        /// <summary>
        /// Parent OrganizationID, if any
        /// </summary>
		public int? ParentOrganizationID { get; set; }

        /// <summary>
        /// Type of organization
        /// </summary>
		public int OrganizationCodeID { get; set; }

        /// <summary>
        /// Name
        /// </summary>
		public string Name { get; set; }

        /// <summary>
        /// Abbreviation
        /// </summary>
		public string Abbreviation { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
		public string DisplayName { get; set; }
	}	
}
