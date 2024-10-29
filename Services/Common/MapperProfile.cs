using AutoMapper;
using Repositories.Entities;
using Repositories.Models.AccountModels;
using Services.Models.AccountModels;
using Role = Repositories.Enums.Role;

namespace Services.Common;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        // Account
        CreateMap<AccountRegisterModel, Account>();
        CreateMap<Account, AccountModel>()
            .ForMember(dest => dest.Roles,
                opt => opt.MapFrom(src => src.AccountRoles.Select(x => x.Role.Name).Select(Enum.Parse<Role>)))
            .ForMember(dest => dest.RoleNames,
                opt => opt.MapFrom(src => src.AccountRoles.Select(x => x.Role.Name)));
        CreateMap<AccountUpdateModel, Account>();
    }
}