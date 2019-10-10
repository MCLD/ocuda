using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class CoverIssueDetail: BaseEntity
    {
        [Required]
        public string Isbn { get; set; }

        public bool IsOpenIssue { get; set; }

        public string UPC { get; set; }

        public string OCLC { get; set; }

        [MaxLength(255)]
        public string Message { get; set; }

        [Required]
        public int CoverIssueTypeId { get; set; }

        [Required]
        public int CoverIssueHeaderId { get; set; }
    }
}
