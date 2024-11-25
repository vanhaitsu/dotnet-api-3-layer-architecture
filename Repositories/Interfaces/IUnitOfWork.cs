namespace Repositories.Interfaces;

public interface IUnitOfWork
{
    AppDbContext Context { get; }

    public Task<int> SaveChangeAsync();

    #region Repository

    IAccountRepository AccountRepository { get; }
    IAccountConversationRepository AccountConversationRepository { get; }
    IAccountRoleRepository AccountRoleRepository { get; }
    IConversationRepository ConversationRepository { get; }
    IMessageRepository MessageRepository { get; }
    IMessageRecipientRepository MessageRecipientRepository { get; }
    IRefreshTokenRepository RefreshTokenRepository { get; }
    IRoleRepository RoleRepository { get; }

    #endregion
}