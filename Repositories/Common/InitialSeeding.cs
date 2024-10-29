using Microsoft.Extensions.DependencyInjection;
using Repositories.Entities;

namespace Repositories.Common;

/// <summary>
///     This class is used to insert initial data
/// </summary>
public static class InitialSeeding
{
    private static readonly List<Role> Roles =
    [
        new() { Name = Enums.Role.Admin.ToString() },
        new() { Name = Enums.Role.User.ToString() }
    ];

    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        foreach (var role in Roles)
            if (!context.Roles.Any(x => x.Name == role.Name))
            {
                role.CreationDate = DateTime.Now;
                context.Roles.Add(role);
            }

        await context.SaveChangesAsync();
    }
}