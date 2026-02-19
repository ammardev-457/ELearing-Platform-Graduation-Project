namespace ELProject.Shared.DTOs
{
    public class PaginationResult<T> where T : class
    {
        public List<T> Items { get; set; } = [];
        public int TotalCount { get; set; }
    }
}