using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class CoverIssueDetail : BaseEntity
    {
        [Required]
        public int CoverIssueHeaderId { get; set; }

        public CoverIssueHeader CoverIssueHeader { get; set; }

        public bool IsResolved { get; set; }
    }
}
