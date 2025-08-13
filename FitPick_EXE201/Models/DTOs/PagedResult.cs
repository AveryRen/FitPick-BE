    namespace FitPick_EXE201.Models.DTOs
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
