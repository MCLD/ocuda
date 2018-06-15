using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;

namespace Ops.Service
{
    public class LinkService
    {
        public IEnumerable<Link> GetLinks()
        {
            return new List<Link>
            {
                new Link
                {
                    Id = 1,
                    Url = "www.google.com",
                    Name = "Summer Reading",
                    IsFeatured = true
                },
                new Link
                {
                    Id = 2,
                    Url = "#",
                    Name = "Reading Adventure",
                    IsFeatured = false
                },
                new Link
                {
                    Id = 3,
                    Url = "#",
                    Name = "Find Libraries",
                    IsFeatured = false
                }
            };
        }

        public Link GetLinkById(int id)
        {
            return new Link
            {
                Id = id,
                Url = "#",
                Name = $"Link {id}"
            };
        }

        public IEnumerable<LinkCategory> GetLinkCategories()
        {
            return new List<LinkCategory>
            {
                new LinkCategory
                {
                    Id = 1,
                    Name = "Link Category 1",
                },
                new LinkCategory
                {
                    Id = 2,
                    Name = "Link Category 2",
                },
                new LinkCategory
                {
                    Id = 3,
                    Name = "Link Category 3",
                },
            };
        }

        public LinkCategory GetLinkCategoryById(int id)
        {
            return new LinkCategory
            {
                Id = id,
                Name = $"Category {id}",
            };
        }


        public async Task<Link> CreateLinkAsync(Link link)
        {
            // call create method from repository
            return link;
        }

        public async Task<Link> EditLinkAsync(Link link)
        {
            // get existing item and update properties that changed
            // call edit method on existing post
            return link;
        }

        public async Task DeleteLinkAsync(int id)
        {
            // call delete method from repository
        }

        public async Task<LinkCategory> CreateLinkCategoryAsync(LinkCategory linkCategory)
        {
            // call create method from repository
            return linkCategory;
        }

        public async Task<LinkCategory> EditLinkCategoryAsync(LinkCategory linkCategory)
        {
            // get existing item and update properties that changed
            // call edit method on existing post
            return linkCategory;
        }

        public async Task DeleteLinkCategoryAsync(int id)
        {
            // call delete method from repository
        }
    }
}
