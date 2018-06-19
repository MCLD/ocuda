using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ocuda.Ops.Controllers.ViewModels.Roster
{
    public class RosterUploadViewModel
    {
        [Required]
        public IFormFile Roster { get; set; }

        [DisplayName("Roster")]
        [FileExtensions(Extensions = "xls")]
        public string FileName
        {
            get
            {
                return Roster?.FileName;
            }
        }
    }
}
