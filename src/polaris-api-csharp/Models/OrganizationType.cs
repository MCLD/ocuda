using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clc.Polaris.Api.Models
{
	/// <summary>
	/// The types of organizations that exist within Polaris.
	/// </summary>
	public enum OrganizationType
	{
		/// <summary>
		/// All organization types.
		/// </summary>
		All,

		/// <summary>
		/// System level organizations.
		/// </summary>
		System,

		/// <summary>
		/// Library level organizations.
		/// </summary>
		Library,

		/// <summary>
		/// Branch level organizations.
		/// </summary>
		Branch
	}
}
