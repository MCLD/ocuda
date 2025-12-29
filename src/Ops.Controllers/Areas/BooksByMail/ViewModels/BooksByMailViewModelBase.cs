using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.BooksByMail.ViewModels
{
    public class BooksByMailViewModelBase : PaginateModel
    {
        protected BooksByMailViewModelBase()
        {
            Heading = "Books by Mail";
        }

        public string Heading { get; set; }
        public string Search { get; set; }
        public string SecondaryHeading { get; set; }
    }
}