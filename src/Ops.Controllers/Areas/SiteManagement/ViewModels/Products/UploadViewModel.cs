using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Products
{
    public class UploadViewModel
    {
        [DisplayName("Inventory")]
        [FileExtensions(Extensions = "xls,xlsx")]
        public string FileName
        {
            get
            {
                return Inventory?.FileName;
            }
        }

        [Required]
        public IFormFile Inventory { get; set; }

        public bool IsReplenishment { get; set; }
        public Product Product { get; set; }
        public int ProductId { get; set; }

        public string UploadType
        {
            get
            {
                return IsReplenishment ? "Replenishment" : "Distribution";
            }
        }
    }
}