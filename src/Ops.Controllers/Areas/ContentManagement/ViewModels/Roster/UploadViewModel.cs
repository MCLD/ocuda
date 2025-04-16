using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Roster
{
    public class UploadViewModel
    {
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