
namespace ELProject.Shared.DTOs
{
    public class PagedRequest
    {

        private int _pagedSize = 10;
        public int PagedSize
        {
            get => _pagedSize;
            set => _pagedSize = value switch
            {
                < 1 => 1,
                > 50 => 50,
                _ => value
            };
        }

        private int _pagedNumber = 1;

        public int PagedNumber
        {
            get => _pagedNumber;
            set => _pagedNumber = value < 1 ? 1 : value;
        }
    }

}