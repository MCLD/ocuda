﻿using System;
using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class CoverIssueHeader : BaseEntity
    {
        [Required]
        public int BibId { get; set; }

        public bool HasPendingIssue { get; set; }

        public DateTime? LastResolved { get; set; }
    }
}
