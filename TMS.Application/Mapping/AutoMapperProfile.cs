using AutoMapper;
using TMS.Domain.Secrets;

namespace TMS.Application.Mapping
{
    public sealed class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<VaultResponse<ArcgisVaultResponse>, ArcgisSecret>()
                .ForMember(dest => dest.ApiKey, opt => opt.MapFrom(src => src.Data.Data.ApiKey));
        }
    }
}