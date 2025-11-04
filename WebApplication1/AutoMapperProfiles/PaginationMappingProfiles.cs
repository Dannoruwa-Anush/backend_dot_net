//PaginationMappingProfile

using AutoMapper;
using WebApplication1.DTOs.ResponseDto.Common;

namespace WebApplication1.AutoMapperProfiles
{
    public class PaginationMappingProfiles : Profile
    {
        public PaginationMappingProfiles()
        {
            // Map generic PaginationResultDto<TSource> -> PaginationResultDto<TDestination>
            CreateMap(typeof(PaginationResultDto<>), typeof(PaginationResultDto<>));
        }
    }
}