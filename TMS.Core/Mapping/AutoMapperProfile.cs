using AutoMapper;
using TMS.Models.Secrets;

namespace TMS.Core.Mapping
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