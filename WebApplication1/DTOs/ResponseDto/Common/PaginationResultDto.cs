namespace WebApplication1.DTOs.ResponseDto.Common
{
    //Generic API Pagination Response
    public class PaginationResultDto<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}