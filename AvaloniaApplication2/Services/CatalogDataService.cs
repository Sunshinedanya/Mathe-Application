using AvaloniaApplication2.Data;
using AvaloniaApplication2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AvaloniaApplication2.Services;

public sealed class CatalogDataService
{
    public async Task InitializeDatabaseAsync()
    {
        await using var dbContext = new AppDbContext();
        await dbContext.Database.MigrateAsync();
    }

    public async Task<PagedResult<ServiceListItem>> GetServicesAsync(string? searchText, bool sortAscending, int page, int pageSize)
    {
        await using var dbContext = new AppDbContext();

        var query = dbContext.Services
            .AsNoTracking()
            .Include(item => item.collection)
            .ThenInclude(item => item!.direction)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var normalizedSearchText = searchText.Trim().ToLower();

            query = query.Where(item =>
                item.name.ToLower().Contains(normalizedSearchText) ||
                item.description.ToLower().Contains(normalizedSearchText) ||
                (item.collection != null && item.collection.name.ToLower().Contains(normalizedSearchText)) ||
                (item.collection != null && item.collection.direction != null && item.collection.direction.name.ToLower().Contains(normalizedSearchText)));
        }

        query = sortAscending
            ? query.OrderBy(item => item.name)
            : query.OrderByDescending(item => item.name);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(GetSkip(page, pageSize))
            .Take(pageSize)
            .Select(item => new ServiceListItem
            {
                id = item.Id,
                name = item.name,
                description = item.description,
                durationMinutes = item.durationMinutes,
                basePrice = item.basePrice,
                isActive = item.isActive,
                collectionId = item.collectionId,
                collectionName = item.collection != null ? item.collection.name : string.Empty,
                directionName = item.collection != null && item.collection.direction != null
                    ? item.collection.direction.name
                    : string.Empty,
                imagePath = item.imagePath,
                lastModifiedAt = item.lastModifiedAt
            })
            .ToListAsync();

        return new PagedResult<ServiceListItem>(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<CollectionListItem>> GetCollectionsAsync(string? searchText, bool sortAscending, int page, int pageSize)
    {
        await using var dbContext = new AppDbContext();

        var query = dbContext.Collections
            .AsNoTracking()
            .Include(item => item.direction)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var normalizedSearchText = searchText.Trim().ToLower();

            query = query.Where(item =>
                item.name.ToLower().Contains(normalizedSearchText) ||
                item.description.ToLower().Contains(normalizedSearchText) ||
                (item.direction != null && item.direction.name.ToLower().Contains(normalizedSearchText)));
        }

        query = sortAscending
            ? query.OrderBy(item => item.name)
            : query.OrderByDescending(item => item.name);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(GetSkip(page, pageSize))
            .Take(pageSize)
            .Select(item => new CollectionListItem
            {
                id = item.Id,
                name = item.name,
                description = item.description,
                directionId = item.directionId,
                directionName = item.direction != null ? item.direction.name : string.Empty,
                lastModifiedAt = item.lastModifiedAt
            })
            .ToListAsync();

        return new PagedResult<CollectionListItem>(items, totalCount, page, pageSize);
    }

    public async Task<PagedResult<DirectionListItem>> GetDirectionsAsync(string? searchText, bool sortAscending, int page, int pageSize)
    {
        await using var dbContext = new AppDbContext();

        var query = dbContext.Directions
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var normalizedSearchText = searchText.Trim().ToLower();

            query = query.Where(item =>
                item.name.ToLower().Contains(normalizedSearchText) ||
                item.description.ToLower().Contains(normalizedSearchText));
        }

        query = sortAscending
            ? query.OrderBy(item => item.name)
            : query.OrderByDescending(item => item.name);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip(GetSkip(page, pageSize))
            .Take(pageSize)
            .Select(item => new DirectionListItem
            {
                id = item.Id,
                name = item.name,
                description = item.description,
                lastModifiedAt = item.lastModifiedAt
            })
            .ToListAsync();

        return new PagedResult<DirectionListItem>(items, totalCount, page, pageSize);
    }

    public async Task<List<LookupItem>> GetDirectionLookupAsync()
    {
        await using var dbContext = new AppDbContext();

        return await dbContext.Directions
            .AsNoTracking()
            .OrderBy(item => item.name)
            .Select(item => new LookupItem
            {
                id = item.Id,
                name = item.name
            })
            .ToListAsync();
    }

    public async Task<List<LookupItem>> GetCollectionLookupAsync()
    {
        await using var dbContext = new AppDbContext();

        return await dbContext.Collections
            .AsNoTracking()
            .OrderBy(item => item.name)
            .Select(item => new LookupItem
            {
                id = item.Id,
                name = item.name
            })
            .ToListAsync();
    }

    public async Task<ServiceEditorModel?> GetServiceByIdAsync(int id)
    {
        await using var dbContext = new AppDbContext();

        return await dbContext.Services
            .AsNoTracking()
            .Select(item => new ServiceEditorModel
            {
                id = item.Id,
                collectionId = item.collectionId,
                name = item.name,
                description = item.description,
                durationMinutes = item.durationMinutes,
                basePrice = item.basePrice,
                isActive = item.isActive,
                imagePath = item.imagePath,
                lastModifiedAt = item.lastModifiedAt
            })
            .FirstOrDefaultAsync(item => item.id == id);
    }

    public async Task<CollectionEditorModel?> GetCollectionByIdAsync(int id)
    {
        await using var dbContext = new AppDbContext();

        return await dbContext.Collections
            .AsNoTracking()
            .Select(item => new CollectionEditorModel
            {
                id = item.Id,
                directionId = item.directionId,
                name = item.name,
                description = item.description,
                lastModifiedAt = item.lastModifiedAt
            })
            .FirstOrDefaultAsync(item => item.id == id);
    }

    public async Task<DirectionEditorModel?> GetDirectionByIdAsync(int id)
    {
        await using var dbContext = new AppDbContext();

        return await dbContext.Directions
            .AsNoTracking()
            .Select(item => new DirectionEditorModel
            {
                id = item.Id,
                name = item.name,
                description = item.description,
                lastModifiedAt = item.lastModifiedAt
            })
            .FirstOrDefaultAsync(item => item.id == id);
    }

    public async Task<int> SaveServiceAsync(ServiceEditorModel model)
    {
        await using var dbContext = new AppDbContext();

        ShopService entity;

        if (model.id == 0)
        {
            entity = new ShopService();
            dbContext.Services.Add(entity);
        }
        else
        {
            entity = await dbContext.Services.FirstAsync(item => item.Id == model.id);
        }

        entity.collectionId = model.collectionId;
        entity.name = model.name.Trim();
        entity.description = model.description.Trim();
        entity.durationMinutes = model.durationMinutes;
        entity.basePrice = model.basePrice;
        entity.isActive = model.isActive;
        entity.imagePath = string.IsNullOrWhiteSpace(model.imagePath) ? null : model.imagePath.Trim();

        await dbContext.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<int> SaveCollectionAsync(CollectionEditorModel model)
    {
        await using var dbContext = new AppDbContext();

        Collection entity;

        if (model.id == 0)
        {
            entity = new Collection();
            dbContext.Collections.Add(entity);
        }
        else
        {
            entity = await dbContext.Collections.FirstAsync(item => item.Id == model.id);
        }

        entity.directionId = model.directionId;
        entity.name = model.name.Trim();
        entity.description = model.description.Trim();

        await dbContext.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<int> SaveDirectionAsync(DirectionEditorModel model)
    {
        await using var dbContext = new AppDbContext();

        Direction entity;

        if (model.id == 0)
        {
            entity = new Direction();
            dbContext.Directions.Add(entity);
        }
        else
        {
            entity = await dbContext.Directions.FirstAsync(item => item.Id == model.id);
        }

        entity.name = model.name.Trim();
        entity.description = model.description.Trim();

        await dbContext.SaveChangesAsync();
        return entity.Id;
    }

    public async Task DeleteServiceAsync(int id)
    {
        await using var dbContext = new AppDbContext();
        var entity = await dbContext.Services.FirstOrDefaultAsync(item => item.Id == id);

        if (entity is null)
        {
            return;
        }

        dbContext.Services.Remove(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteCollectionAsync(int id)
    {
        await using var dbContext = new AppDbContext();
        var entity = await dbContext.Collections.FirstOrDefaultAsync(item => item.Id == id);

        if (entity is null)
        {
            return;
        }

        dbContext.Collections.Remove(entity);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteDirectionAsync(int id)
    {
        await using var dbContext = new AppDbContext();
        var entity = await dbContext.Directions.FirstOrDefaultAsync(item => item.Id == id);

        if (entity is null)
        {
            return;
        }

        dbContext.Directions.Remove(entity);
        await dbContext.SaveChangesAsync();
    }

    private static int GetSkip(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        return (normalizedPage - 1) * pageSize;
    }
}

public sealed record PagedResult<T>(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
{
    public int totalPages => pageSize <= 0 ? 0 : (int)Math.Ceiling(totalCount / (double)pageSize);
}

public sealed class LookupItem
{
    public int id { get; set; }

    public string name { get; set; } = string.Empty;
}

public sealed class ServiceListItem
{
    public int id { get; set; }

    public string name { get; set; } = string.Empty;

    public string description { get; set; } = string.Empty;

    public int durationMinutes { get; set; }

    public decimal basePrice { get; set; }

    public bool isActive { get; set; }

    public int collectionId { get; set; }

    public string collectionName { get; set; } = string.Empty;

    public string directionName { get; set; } = string.Empty;

    public string? imagePath { get; set; }

    public DateTime lastModifiedAt { get; set; }
}

public sealed class CollectionListItem
{
    public int id { get; set; }

    public string name { get; set; } = string.Empty;

    public string description { get; set; } = string.Empty;

    public int directionId { get; set; }

    public string directionName { get; set; } = string.Empty;

    public DateTime lastModifiedAt { get; set; }
}

public sealed class DirectionListItem
{
    public int id { get; set; }

    public string name { get; set; } = string.Empty;

    public string description { get; set; } = string.Empty;

    public DateTime lastModifiedAt { get; set; }
}

public sealed class ServiceEditorModel
{
    public int id { get; set; }

    public int collectionId { get; set; }

    public string name { get; set; } = string.Empty;

    public string description { get; set; } = string.Empty;

    public int durationMinutes { get; set; }

    public decimal basePrice { get; set; }

    public bool isActive { get; set; } = true;

    public string? imagePath { get; set; }

    public DateTime? lastModifiedAt { get; set; }
}

public sealed class CollectionEditorModel
{
    public int id { get; set; }

    public int directionId { get; set; }

    public string name { get; set; } = string.Empty;

    public string description { get; set; } = string.Empty;

    public DateTime? lastModifiedAt { get; set; }
}

public sealed class DirectionEditorModel
{
    public int id { get; set; }

    public string name { get; set; } = string.Empty;

    public string description { get; set; } = string.Empty;

    public DateTime? lastModifiedAt { get; set; }
}
