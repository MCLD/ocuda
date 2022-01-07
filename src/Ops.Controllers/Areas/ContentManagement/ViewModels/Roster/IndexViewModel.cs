using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Roster
{
    public class IndexViewModel : PaginateModel
    {
        public ICollection<RosterHeader> RosterHeaders { get; set; }

        [Required]
        public IFormFile Roster { get; set; }

        [DisplayName("Roster")]
        [FileExtensions(Extensions = "xls,xlsx")]
        public string FileName
        {
            get
            {
                return Roster?.FileName;
            }
        }

    }
}
