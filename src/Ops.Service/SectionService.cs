using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public IEnumerable<Calendar> GetCalendars()
        {
            return new List<Calendar>
            {
                new Calendar
                {
                    IsPinned = true,
                    Name = "Staff Training",
                    Url = "https://www.google.com/",
                    When = DateTime.Parse("2018-06-19 10:00")
                },
                new Calendar
                {
                    Name = "Fun Event!",
                    Url = "https://www.google.com/",
                    When = DateTime.Parse("2018-06-08 12:00")
                },
                new Calendar
                {
                    Name = "Important Date Reminder",
                    Url = "https://www.google.com/",
                    When = DateTime.Parse("2018-06-12 9:00")
                }
            };
        }
    }
}
