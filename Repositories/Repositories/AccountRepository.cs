﻿using Microsoft.EntityFrameworkCore;
using Repositories.Entities;
using Repositories.Interfaces;

namespace Repositories.Repositories;

public class AccountRepository : GenericRepository<Account>, IAccountRepository
{
    public AccountRepository(AppDbContext context, IClaimService claimService) : base(context, claimService)
    {
    }

    public async Task<Account?> FindByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(account => account.Email == email);
    }

    public async Task<Account?> FindByUsernameAsync(string username)
    {
        return await _dbSet.FirstOrDefaultAsync(account => account.Username == username);
    }
}