﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Decks
{
    public class DetailViewModel
    {
        public DetailViewModel()
        {
            CardDetails = [];
        }

        public string BackLink { get; set; }
        public IList<CardDetail> CardDetails { get; }
        public int DeckId { get; set; }
        public string DeckName { get; set; }
        public SelectList LanguageList { get; set; }

        [DisplayName("Language")]
        public Language SelectedLanguage { get; set; }

        public static string LanguageTitle(int languageCount)
        {
            return languageCount == 1
                ? $"Available in {languageCount} language"
                : $"Available in {languageCount} languages";
        }

        public string LanguageCssClass(int languageCount)
        {
            return languageCount < LanguageList.Count()
                ? "text-warning"
                : "text-success";
        }
    }
}