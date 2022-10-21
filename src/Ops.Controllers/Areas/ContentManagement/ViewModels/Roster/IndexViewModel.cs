using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Roster
{
    public class IndexViewModel : PaginateModel
    {
        [DisplayName("Roster")]
        [FileExtensions(Extensions = "xls,xlsx")]
        public string FileName
        {
            get
            {
                return Roster?.FileName;
            }
        }

        [Required]
        public IFormFile Roster { get; set; }

        public ICollection<RosterHeader> RosterHeaders { get; set; }
    }
}