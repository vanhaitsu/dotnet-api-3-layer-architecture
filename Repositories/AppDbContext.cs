using Microsoft.EntityFrameworkCore;
using Repositories.Entities;

namespace Repositories;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        // These lines will fix the error "Cannot write DateTime with Kind=Local to PostgreSQL type 'timestamp with time zone', only UTC is supported"
        // AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        // AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<AccountConversation> AccountConversations { get; set; }
    public DbSet<AccountRole> AccountRoles { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageRecipient> MessageRecipients { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.Property(account => account.FirstName).HasMaxLength(50);
            entity.Property(account => account.LastName).HasMaxLength(50);
            entity.HasIndex(account => account.Username).IsUnique();
            entity.Property(account => account.Username).HasMaxLength(50);
            entity.HasIndex(account => account.Email).IsUnique();
            entity.Property(account => account.Email).HasMaxLength(256);
            entity.Property(account => account.PhoneNumber).HasMaxLength(15);
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.Property(conversation => conversation.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(role => role.Name).HasMaxLength(50);
            entity.Property(role => role.Description).HasMaxLength(256);
        });
    }
}