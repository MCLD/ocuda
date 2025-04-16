using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section
{
    public class PostViewModel
    {
        public bool CanPromote { get; set; }

        public static IList<SelectListItem> IsPinnedSelect
        {
            get
            {
                return new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Value = false.ToString(),
                        Text = "Show this post in chronological order"
                    },
                    new SelectListItem
                    {
                        Value = true.ToString(),
                        Text = "Pin this post to the top"
                    }
                };
            }
        }

        public Post Post { get; set; }

        public bool Publish { get; set; }
        public DateTime? PinUntilDate { get; set; }

        public DateTime? PinUntilTime { get; set; }

        [Display(Name = "Publish At")]
        public DateTime? PublishAtDate { get; set; }

        public DateTime? PublishAtTime { get; set; }

        public static IList<SelectListItem> PublishSelect
        {
            get
            {
                return new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Value = false.ToString(),
                        Text = "Save as a draft"
                    },
                    new SelectListItem
                    {
                        Value = true.ToString(),
                        Text = "Publish post"
                    }
                };
            }
        }

        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public string SectionSlug { get; set; }

        public IList<SelectListItem> ShowOnHomepageSelect
        {
            get
            {
                return new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Value = false.ToString(),
                        Text = $"Only show on the {SectionName} section page"
                    },
                    new SelectListItem
                    {
                        Value = true.ToString(),
                        Text = "Show post on home page",
                        Selected = Post?.ShowOnHomePage == true
                    }
                };
            }
        }
    }
}