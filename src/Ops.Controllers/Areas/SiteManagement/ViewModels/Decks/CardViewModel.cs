using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Decks
{
    public class CardViewModel
    {
        [DisplayName("Image alternative text")]
        [Description("How should this image be described to someone who can't see it?")]
        [MaxLength(255)]
        public string AltText { get; set; }

        public string BackLink { get; set; }
        public CardDetail CardDetail { get; set; }

        [DisplayName("Image")]
        public IFormFile CardImage { get; set; }

        public int DeckId { get; set; }
        public string DeckName { get; set; }
        public bool IsOnlyCard { get; set; }
        public bool IsUpdate { get; set; }
        public string LanguageDescription { get; set; }
        public int LanguageId { get; set; }
        public SelectList LanguageList { get; set; }

        [DisplayName("Language")]
        public Language SelectedLanguage { get; set; }
    }
}