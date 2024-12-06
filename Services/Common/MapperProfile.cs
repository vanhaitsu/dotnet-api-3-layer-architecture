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
        CreateMap<AccountSignUpModel, Account>();
        CreateMap<Account, AccountModel>()
            .ForMember(dest => dest.Roles,
                opt => opt.MapFrom(src =>
                    Enumerable.Select(src.AccountRoles, accountRole => accountRole.Role.Name).Select(Enum.Parse<Role>)))
            .ForMember(dest => dest.RoleNames,
                opt => opt.MapFrom(src => Enumerable.Select(src.AccountRoles, accountRole => accountRole.Role.Name)));
        CreateMap<AccountUpdateModel, Account>();

        // AccountConversation
        // CreateMap<AccountConversation, ConversationModel>()
        //     .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Conversation.Name))
        //     .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Conversation.Image))
        //     .ForMember(dest => dest.IsRestricted, opt => opt.MapFrom(src => src.Conversation.IsRestricted))
        //     .ForMember(dest => dest.LatestMessage, opt => opt.MapFrom(src => src.Conversation.IsRestricted))

        // Message
    }
}