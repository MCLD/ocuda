using System;
using System.Collections.Generic;
using Ocuda.Ops.Models;

namespace Ops.Service
{
    public class SectionService
    {
        public IEnumerable<Section> GetAll()
        {
            return new List<Section>
            {
                new Section
                {
                   Icon = "fa-smile",
                   Name = "Human Resources",
                   Path = "HumanResources",
                   SortOrder = 0
                },
                new Section
                {
                   Icon = "fa-users",
                   Name = "Operations",
                   Path = "Operations",
                   SortOrder = 1
                },
                new Section
                {
                   Icon = "fa-comments",
                   Name = "Communications",
                   Path = "Communications",
                   SortOrder = 2
                },
                new Section
                {
                   Icon = "fa-thumbs-up",
                   Name = "Services",
                   Path = "Services",
                   SortOrder = 3
                },
                new Section
                {
                   Icon = "fa-laptop",
                   Name = "IT",
                   Path = "IT",
                   SortOrder = 4
                } 
            };
        }

        public IEnumerable<SectionPost> GetBlogPosts()
        {
            return new List<SectionPost>
            {
                new SectionPost
                {
                    Content = "Nam dignissim porta leo vitae sodales. Morbi mollis, libero vitae sagittis tincidunt, mi dui luctus metus, elementum tincidunt nisl nisl at elit. Nulla tellus elit, aliquam in pulvinar id, interdum in velit. Suspendisse non aliquam dolor, vestibulum lacinia est. Pellentesque placerat nibh blandit, gravida nulla sed, tristique nisi. Nunc volutpat ultrices augue et congue. In in lacus condimentum dui finibus sagittis nec ut neque. Aenean accumsan nisl quis convallis rutrum. Cras vel imperdiet est. Curabitur et sagittis dui.",
                    CreatedAt = DateTime.Parse("2018-06-04 15:00"),
                    CreatedBy = 1,
                    IsPinned = true,
                    Title = "Test Post 3"
                },
                new SectionPost
                {
                    Content = "Ut auctor risus diam, sed aliquam quam iaculis ac. Sed rutrum tortor eget ante consequat, ac malesuada ligula dictum. Phasellus non urna interdum, vehicula augue ac, egestas orci. Sed ut nisl ipsum. Donec hendrerit, nisl vitae interdum pretium, ligula lorem varius nisi, non cursus libero libero eu dolor. Fusce bibendum, lorem sed tempor condimentum, enim ante sollicitudin dolor, faucibus viverra quam erat ac neque. Integer sagittis magna eu augue eleifend, at pellentesque diam malesuada.",
                    CreatedAt = DateTime.Parse("2018-06-04 17:25"),
                    CreatedBy = 1,
                    IsPinned = false,
                    Title = "Test Post 4"
                },
                new SectionPost
                {
                    Content = "Pellentesque sit amet risus eu lorem elementum porttitor. Ut pretium facilisis finibus. Suspendisse potenti. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Duis vestibulum tortor blandit, imperdiet sem et, venenatis lectus. Sed id metus magna. Etiam vel congue sapien, nec blandit lectus. Etiam feugiat est ornare quam sollicitudin, et luctus neque efficitur. Nullam quis pretium ipsum. Pellentesque eleifend quam vitae laoreet varius. Integer fringilla velit in metus finibus, a aliquet lectus tincidunt. Pellentesque varius eleifend est, id maximus nunc tristique ac.",
                    CreatedAt = DateTime.Parse("2018-06-04 13:30"),
                    CreatedBy = 1,
                    IsPinned = false,
                    Title = "Test Post 2"
                },
                new SectionPost
                {
                    Content = "Sed semper, sapien quis luctus semper, nibh eros sollicitudin tellus, at tincidunt arcu odio a est. Nam nec nulla ex. Nullam et maximus ex, at porttitor velit. Sed ac justo ligula. Morbi sed lectus turpis. Aenean suscipit tellus nec risus aliquam, et dignissim urna mollis. Aliquam erat volutpat. Curabitur risus tellus, facilisis a tempus eu, hendrerit ut elit. Phasellus ut quam consequat, molestie mauris non, faucibus felis. Pellentesque finibus lobortis arcu, a tincidunt erat pulvinar vel. Proin in egestas magna, nec feugiat velit.",
                    CreatedAt = DateTime.Parse("2018-06-04 12:15"),
                    CreatedBy = 1,
                    IsPinned = false,
                    Title = "Test Post 1"
                }
            };
        }

        public IEnumerable<SectionLink> GetLinks()
        {
            return new List<SectionLink>
            {
                new SectionLink
                {
                    Url = "#",
                    Name = "Summer Reading"
                },
                new SectionLink
                {
                    Url = "#",
                    Name = "Reading Adventure"
                },
                new SectionLink
                {
                    Url = "#",
                    Name = "Find Libraries"
                }
            };
        }

        public IEnumerable<SectionCalendar> GetCalendars()
        {
            return new List<SectionCalendar>
            {
                new SectionCalendar
                {
                    IsPinned = true,
                    Name = "Staff Training",
                    Url = "https://www.google.com/",
                    When = DateTime.Parse("2018-06-19 10:00")
                },
                new SectionCalendar
                {
                    Name = "Fun Event!",
                    Url = "https://www.google.com/",
                    When = DateTime.Parse("2018-06-08 12:00")
                },
                new SectionCalendar
                {
                    Name = "Important Date Reminder",
                    Url = "https://www.google.com/",
                    When = DateTime.Parse("2018-06-12 9:00")
                }
            };
        }

        public IEnumerable<SectionFile> GetFiles()
        {
            return new List<SectionFile>
            {
                new SectionFile
                {
                    CreatedAt = DateTime.Parse("2018-05-01"),
                    IsPinned = true,
                    FilePath = "/file.txt",
                    Name = "Important File!",
                    Icon = "fa-file-word alert-primary"
                },
                new SectionFile
                {
                    CreatedAt = DateTime.Parse("2018-06-04"),
                    FilePath = "/file.txt",
                    Name = "New File 2",
                    Icon = "fa-file-excel alert-success"
                },
                new SectionFile
                {
                    CreatedAt = DateTime.Parse("2018-05-20"),
                    FilePath = "/file.txt",
                    Name = "New File 1",
                    Icon = "fa-file-pdf alert-danger"
                }
            };
        }
    }
}
