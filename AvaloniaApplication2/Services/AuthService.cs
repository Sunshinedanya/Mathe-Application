using AvaloniaApplication2.Data;
using AvaloniaApplication2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AvaloniaApplication2.Services;

public sealed class AuthService
{
    public async Task InitializeAsync()
    {
        await using var db = new AppDbContext();
        await db.Database.MigrateAsync();
        await EnsureAdminUserAsync(db);
    }

    public async Task<ApplicationUser?> LoginAsync(string email, string password)
    {
        await using var db = new AppDbContext();

        return await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.email == email &&
                u.passwordHash == password &&
                u.isActive);
    }

    private static async Task EnsureAdminUserAsync(AppDbContext db)
    {
        var exists = await db.Users.AnyAsync(u => u.email == "admin@mathieu.ru");
        if (exists)
        {
            return;
        }

        db.Users.Add(new ApplicationUser
        {
            email = "admin@mathieu.ru",
            passwordHash = "admin",
            fullName = "Администратор",
            phone = string.Empty,
            isActive = true
        });

        await db.SaveChangesAsync();
    }
}
