using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clc.Polaris.Api.Models
{
    /// <summary>
    /// Types of patron blocks
    /// </summary>
	public enum BlockType
	{
        /// <summary>
        /// Free text
        /// </summary>
		FreeText = 1,

        /// <summary>
        /// Library assigned
        /// </summary>
		LibraryAssigned,

        /// <summary>
        /// System
        /// </summary>
		System
	}
}
