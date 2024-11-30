using AutoMapper;
using Repositories.Entities;
using Repositories.Models.AccountConversationModels;
using Repositories.Models.AccountModels;
using Repositories.Models.ConversationModels;
using Services.Models.AccountModels;
using Services.Models.ConversationModels;
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
        CreateMap<Account, SimpleAccountModel>();
        CreateMap<AccountUpdateModel, Account>();

        // AccountConversation
        CreateMap<AccountConversation, AccountConversationModel>()
            .ForMember(dest => dest.Account, opt => opt.MapFrom(src => src.Account));

        // Conversation
        CreateMap<ConversationAddModel, Conversation>();
        CreateMap<Conversation, ConversationModel>()
            .ForMember(dest => dest.NumberOfMembers,
                opt => opt.MapFrom(src =>
                    Enumerable.Count(src.AccountConversations,
                        accountConversation =>
                            !accountConversation.IsDeleted && !accountConversation.Account.IsDeleted)));
    }
}