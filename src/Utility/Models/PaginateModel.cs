namespace Ocuda.Utility.Models
{
    public class PaginateModel
    {
        public int CurrentPage { get; set; }
        public int ItemCount { get; set; }
        public int ItemsPerPage { get; set; }

        public int? NextPage
        {
            get
            {
                if (CurrentPage < LastPage)
                {
                    return CurrentPage + 1;
                }
                return null;
            }
        }

        public int? PreviousPage
        {
            get
            {
                if (CurrentPage > 1)
                {
                    return CurrentPage - 1;
                }
                return null;
            }
        }

        public int? FirstPage
        {
            get
            {
                if (CurrentPage > 1)
                {
                    return 1;
                }
                return null;
            }
        }

        public int? LastPage
        {
            get
            {
                if (ItemCount > ItemsPerPage)
                {
                    int last = MaxPage;
                    if (CurrentPage != last)
                    {
                        return last;
                    }
                }
                return null;
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

        public bool PastMaxPage
        {
            get
            {
                return MaxPage > 0 && CurrentPage > MaxPage;
            }
        }
    }
}