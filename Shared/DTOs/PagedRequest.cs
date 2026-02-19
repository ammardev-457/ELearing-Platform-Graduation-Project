
namespace ELProject.Shared.DTOs
{
    public class PaginationParameters
    {

        private int _pageSize = 10;
        public int PagedSize
        {
            get => _pageSize;
            set => _pageSize = value switch
            {
                < 1 => 1,
                > 50 => 50,
                _ => value
            };
        }

        private int _pageNumber = 1;

        public int PagedNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }
    }

}