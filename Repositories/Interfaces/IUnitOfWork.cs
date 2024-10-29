namespace Repositories.Interfaces;

public interface IUnitOfWork
{
    AppDbContext Context { get; }

    public Task<int> SaveChangeAsync();

    #region Repository

    IAccountRepository AccountRepository { get; }
    IAccountRoleRepository AccountRoleRepository { get; }
    IRefreshTokenRepository RefreshTokenRepository { get; }
    IRoleRepository RoleRepository { get; }

    #endregion
}