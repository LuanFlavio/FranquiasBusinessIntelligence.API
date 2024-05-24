namespace Domain.Domain
{
    public class PaginationHeader
    {
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public int CountItems { get; set; }
        public int TotalPages { get; set; }

        public PaginationHeader(int currentPage,
                                int itemsPerPage,
                                int countItems,
                                int totalPages)
        {
            CurrentPage = currentPage;
            ItemsPerPage = itemsPerPage;
            CountItems = countItems;
            TotalPages = totalPages;
        }        

        public PaginationHeader(int countItems)
        {
            CountItems = countItems;
        }
    }
}
