namespace Domain.Domain
{
    public class PageParams
    {
        private const int MaxPageSize = 50;
        public int CurrentPage { get; set; } = 1;
        private int MaxItemsPerPage = 10;
        public string? Description { get; set; }
        public int ItemsPerPage
        {
            get { return MaxItemsPerPage; }
            set { MaxItemsPerPage = value > MaxPageSize ? MaxPageSize : value; }
        }
    }

    public class PageGeneralParams
    {
        private const int MaxPageSize = 50;
        private int MaxItemsPerPage = 10;
        public string? Description { get; set; }
    }

}
