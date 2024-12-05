using AutoMapper;
using Repositories.Entities;
using Repositories.Models.AccountModels;
using Repositories.Models.ConversationModels;
using Services.Models.AccountModels;
using Services.Models.ConversationModels;
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
                    Enumerable.Select(src.AccountRoles, accountRole => accountRole.Role.Name).Select(Enum.Parse<Role>)))
            .ForMember(dest => dest.RoleNames,
                opt => opt.MapFrom(src => Enumerable.Select(src.AccountRoles, accountRole => accountRole.Role.Name)));
        CreateMap<Account, MemberModel>();
        CreateMap<AccountUpdateModel, Account>();

        // Conversation
        CreateMap<ConversationAddModel, Conversation>();
        CreateMap<Conversation, ConversationModel>();
        
        // Message
        CreateMap<MessageAddModel, Message>();
    }
}