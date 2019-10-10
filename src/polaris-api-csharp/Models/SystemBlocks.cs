using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Clc.Polaris.Api.Models
{
    /// <summary>
    /// System patron blocks
    /// </summary>
	public enum SystemBlocks
	{
        /// <summary>
        /// Self registration from PAC
        /// </summary>
		SelfRegistrationFromPAC = 64,

        /// <summary>
        /// Address update from PAC
        /// </summary>
		AddressUpdateFromPAC = 128,

        /// <summary>
        /// Express registration
        /// </summary>
		ExpressRegistration = 256,

        /// <summary>
        /// Offline registration
        /// </summary>
		OfflineRegisteration = 512
	}
}
