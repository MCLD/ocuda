using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clc.Polaris.Api.Models
{
    /// <summary>
    /// Patron sort options
    /// </summary>
	public enum PatronSortKeys
	{
        /// <summary>
        /// City
        /// </summary>
		CITY,

        /// <summary>
        /// Registered branch
        /// </summary>
		ORG,

        /// <summary>
        /// Patron birthdate
        /// </summary>
		PATB,

        /// <summary>
        /// Patron first name
        /// </summary>
		PATN,

        /// <summary>
        /// Patron last name
        /// </summary>
		PATNL,

        /// <summary>
        /// State
        /// </summary>
		STATE,

        /// <summary>
        /// ZIP
        /// </summary>
		ZIP
	}
}
