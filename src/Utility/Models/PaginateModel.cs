namespace Ocuda.Utility.Models
{
    public class PaginateModel
    {
        public int CurrentPage { get; set; }

        public int? FirstPage
        {
            get
            {
                return CurrentPage > 1 ? 1 : null;
            }
        }

        public int ItemCount { get; set; }

        public int ItemsPerPage { get; set; }

        public int? LastPage
        {
            get
            {
                return ItemCount > ItemsPerPage && CurrentPage != MaxPage ? MaxPage : null;
            }
        }

        public int MaxPage
        {
            get
            {
                if (ItemsPerPage == 0)
                {
                    return 0;
                }
                int last = ItemCount / ItemsPerPage;
                if (ItemCount % ItemsPerPage > 0)
                {
                    last++;
                }
                return last;
            }
        }

        public int? NextPage
        {
            get
            {
                return CurrentPage < LastPage ? CurrentPage + 1 : null;
            }
        }

        public bool PastMaxPage
        {
            get
            {
                return MaxPage > 0 && CurrentPage > MaxPage;
            }
        }

        public int? PreviousPage
        {
            get
            {
                return CurrentPage > 1 ? CurrentPage - 1 : null;
            }
        }

        public static string BoolIconClass(bool value)
        {
            return value
                ? "fa-regular fa-circle-check text-success"
                : "fa-regular fa-circle-xmark text-danger";
        }
    }
}