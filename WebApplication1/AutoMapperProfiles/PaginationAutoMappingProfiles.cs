using AutoMapper;
using WebApplication1.DTOs.ResponseDto.Common;

namespace WebApplication1.AutoMapperProfiles
{
    public class PaginationAutoMappingProfiles : Profile
    {
        //This a Generic map between any two paginated DTOs
        public PaginationAutoMappingProfiles()
        {
            // Map generic PaginationResultDto<TSource> -> PaginationResultDto<TDestination>
            CreateMap(typeof(PaginationResultDto<>), typeof(PaginationResultDto<>));
        }
    }
}