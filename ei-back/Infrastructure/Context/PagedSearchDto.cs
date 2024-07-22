using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace ei_back.Infrastructure.Context
{
    public class PagedSearchDto<T>
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }
        public string SortDirections { get; set; }
        public List<T> List { get; set; }

        public PagedSearchDto() { }

        public PagedSearchDto(int currentPage, int pageSize, string sortDirections)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            SortDirections = sortDirections;
        }

        public int GetCurrentPage()
        {
            return CurrentPage == 0 ? 2 : CurrentPage;
        }

        public int GetPageSize()
        {
            return PageSize == 0 ? 10 : PageSize;
        }

        public string ValidateSort(string sortDirection)
        {
            return (!string.IsNullOrWhiteSpace(sortDirection) && !sortDirection.Equals("desc")) ? "asc" : "desc";
        }

        public int ValidateSize(int pageSize)
        {
            return (pageSize < 1) ? 10 : pageSize;
        }

        public int ValidateOffset(int page, int size)
        {
            return page > 0 ? (page - 1) * ValidateSize(size) : 0;
        }
    }
}
