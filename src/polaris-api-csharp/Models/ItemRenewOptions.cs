using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clc.Polaris.Api.Models
{
    /// <summary>
    /// Options whenr renewing an item
    /// </summary>
    public class ItemRenewOptions
    {
        /// <summary>
        /// The branch where the renewal takes place
        /// </summary>
        public int BranchId { get; set; } = 1;

        /// <summary>
        /// The user performing the renewal
        /// </summary>
        public int UserId { get; set; } = 1;

        /// <summary>
        /// The workstation the renewal takes place
        /// </summary>
        public int WorkstationId { get; set; } = 1;

        /// <summary>
        /// Ignore ignorable errors and continue if possible
        /// </summary>
        public bool IgnoreOverrideErrors { get; set; } = true;
    }
}
