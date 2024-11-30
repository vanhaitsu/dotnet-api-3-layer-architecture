﻿using AutoMapper;
using Repositories.Entities;
using Repositories.Models.AccountModels;
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
        CreateMap<AccountUpdateModel, Account>();
        
        // Conversation
        CreateMap<ConversationAddModel, Conversation>();
    }
}