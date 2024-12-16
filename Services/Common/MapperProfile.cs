using AutoMapper;
using Repositories.Entities;
using Repositories.Models.AccountModels;
using Repositories.Models.MessageModels;
using Services.Models.AccountModels;
using Services.Models.MessageModels;
using Role = Repositories.Enums.Role;

namespace Services.Common;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        // Account
        CreateMap<AccountSignUpModel, Account>();
        CreateMap<Account, AccountModel>()
            .ForMember(dest => dest.Roles,
                opt => opt.MapFrom(src =>
                    src.AccountRoles.Select(accountRole => accountRole.Role.Name).Select(Enum.Parse<Role>)))
            .ForMember(dest => dest.RoleNames,
                opt => opt.MapFrom(src => src.AccountRoles.Select(accountRole => accountRole.Role.Name)));
        CreateMap<Account, AccountLiteModel>();
        CreateMap<AccountUpdateModel, Account>();

        // Message
        CreateMap<MessageAddModel, Message>();
        CreateMap<Message, MessageModel>();
    }
}