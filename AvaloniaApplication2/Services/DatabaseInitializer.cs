using AvaloniaApplication2.Data;
using Microsoft.EntityFrameworkCore;

namespace AvaloniaApplication2.Services;

public sealed class DatabaseInitializer
{
    private readonly AppDbContext appDbContext;

    public DatabaseInitializer(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await appDbContext.Database.MigrateAsync(cancellationToken);
    }
}
