using Repositories.Interfaces;

namespace Repositories.Common;

public class UnitOfWork : IUnitOfWork
{
    public UnitOfWork(AppDbContext context, IAccountRepository accountRepository,
        IAccountRoleRepository accountRoleRepository, IRefreshTokenRepository refreshTokenRepository,
        IRoleRepository roleRepository)
    {
        Context = context;
        AccountRepository = accountRepository;
        AccountRoleRepository = accountRoleRepository;
        RefreshTokenRepository = refreshTokenRepository;
        RoleRepository = roleRepository;
    }

    public AppDbContext Context { get; }
    public IAccountRepository AccountRepository { get; }
    public IAccountRoleRepository AccountRoleRepository { get; }
    public IRefreshTokenRepository RefreshTokenRepository { get; }
    public IRoleRepository RoleRepository { get; }

    public async Task<int> SaveChangeAsync()
    {
        return await Context.SaveChangesAsync();
    }
}