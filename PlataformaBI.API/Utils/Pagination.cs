using Domain.Domain;
using System.Text.Json;

namespace PlataformaBI.API.Utils
{
    public static class Pagination
    {
        public static void AddPaginationWithout(this HttpResponse response, int countItems)
        {
            var pagination = new PaginationHeader(countItems);


            response.Headers.Add("countItens", pagination.CountItems.ToString());
        }

        public static void AddPagination(this HttpResponse response,
            int currentPage, int itemsPerPage, int countItems, int totalPages)
        {
            var pagination = new PaginationHeader(currentPage,
                                                  itemsPerPage,
                                                  countItems,
                                                  totalPages);

            response.Headers.Add("currentPage", pagination.CurrentPage.ToString());
            response.Headers.Add("itemsPerPage", pagination.ItemsPerPage.ToString());
            response.Headers.Add("countItems", pagination.CountItems.ToString());
            response.Headers.Add("totalPages", pagination.TotalPages.ToString());
        }
    }
}
