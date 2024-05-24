namespace Domain.Domain
{
    public class PageList<T> : List<T>
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int ItemsPerPage { get; set; }
        public int CountItens { get; set; }

        public PageList(List<T> items, int count)
        {
            CountItens = count;
            AddRange(items);
        }

        public PageList(List<T> items, int count, int currentPage, int itemsPerPage)
        {
            CurrentPage = currentPage;
            ItemsPerPage = itemsPerPage;
            CountItens = count;
            TotalPages = (int)Math.Ceiling(count / (double)itemsPerPage);
            AddRange(items);
        }

        public static PageList<T> CreateAsyncWithoutPagination(
            IEnumerable<T> source
        )
        {
            var count = source.Count();
            var items = source.ToList();

            return new PageList<T>(items, count);
        }

        public static PageList<T> CreateAsyncWithPagination(
            IEnumerable<T> source, 
            int currentPage, 
            int itemsPerPage
        )
        {
            var count = source.Count();
            var items = source.Skip((currentPage - 1) * itemsPerPage)
                                    .Take(itemsPerPage)
                                    .ToList();
            return new PageList<T>(items, count, currentPage, itemsPerPage);
        }
    }
}
