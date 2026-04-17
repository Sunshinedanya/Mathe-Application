using AvaloniaApplication2.Data;
using AvaloniaApplication2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AvaloniaApplication2.Services;

public sealed class CatalogDataService
{
    public async Task InitializeDatabaseAsync()
    {
        await using var dbContext = new AppDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        if (await dbContext.Directions.AnyAsync())
        {
            return;
        }

        await SeedDemoDataAsync(dbContext);
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

    private static async Task SeedDemoDataAsync(AppDbContext dbContext)
    {
        var userRole = new Role { code = "user", name = "Пользователь" };
        var moderatorRole = new Role { code = "moderator", name = "Модератор" };
        var adminRole = new Role { code = "admin", name = "Администратор" };
        var masterRole = new Role { code = "master", name = "Мастер" };

        var juniorLevel = new QualificationLevel { name = "Junior", sortOrder = 1 };
        var middleLevel = new QualificationLevel { name = "Middle", sortOrder = 2 };
        var seniorLevel = new QualificationLevel { name = "Senior", sortOrder = 3 };

        var animeDirection = new Direction
        {
            name = "Аниме",
            description = "Коллекции и услуги, вдохновленные популярными аниме-вселенными."
        };

        var newYearDirection = new Direction
        {
            name = "Новый год",
            description = "Праздничные образы, декор и тематические костюмы к новогоднему сезону."
        };

        var halloweenDirection = new Direction
        {
            name = "Хэллоуин",
            description = "Темные, мистические и хоррор-образы для осенних событий."
        };

        var cyberpunkDirection = new Direction
        {
            name = "Киберпанк",
            description = "Неоновые, технологичные и футуристичные решения."
        };

        var noirDirection = new Direction
        {
            name = "Нуар",
            description = "Мрачная эстетика, контраст и ретро-атмосфера."
        };

        var collections = new List<Collection>
        {
            new()
            {
                direction = animeDirection,
                name = "Наруто",
                description = "Образы и аксессуары по мотивам вселенной Наруто."
            },
            new()
            {
                direction = animeDirection,
                name = "Атака титанов",
                description = "Костюмы и косплей-решения в стилистике разведкорпуса."
            },
            new()
            {
                direction = newYearDirection,
                name = "Зимняя сказка",
                description = "Праздничные наряды и новогодняя кастомизация."
            },
            new()
            {
                direction = halloweenDirection,
                name = "Тыквенная ночь",
                description = "Хэллоуинские костюмы и декоративные элементы."
            },
            new()
            {
                direction = cyberpunkDirection,
                name = "Night City",
                description = "Неоновые коллекции для футуристичных образов."
            },
            new()
            {
                direction = noirDirection,
                name = "Черный бархат",
                description = "Нуарные костюмы и аксессуары."
            }
        };

        var services = new List<ShopService>
        {
            new()
            {
                collection = collections[0],
                name = "Кастомизация плаща Акацуки",
                description = "Индивидуальная подгонка плаща, вышивка и декоративные элементы.",
                durationMinutes = 180,
                basePrice = 4500m,
                isActive = true
            },
            new()
            {
                collection = collections[1],
                name = "Пошив плаща разведкорпуса",
                description = "Создание плаща и нанесение символики подразделения.",
                durationMinutes = 240,
                basePrice = 6800m,
                isActive = true
            },
            new()
            {
                collection = collections[2],
                name = "Новогодний семейный образ",
                description = "Комплексная праздничная кастомизация для семейной фотосессии.",
                durationMinutes = 300,
                basePrice = 8200m,
                isActive = true
            },
            new()
            {
                collection = collections[3],
                name = "Хэллоуинский грим и костюм",
                description = "Подбор образа, грим и подготовка костюма для вечеринки.",
                durationMinutes = 210,
                basePrice = 5900m,
                isActive = true
            },
            new()
            {
                collection = collections[4],
                name = "Неоновый косплей под заказ",
                description = "Разработка и создание футуристичного костюма по референсу.",
                durationMinutes = 480,
                basePrice = 15400m,
                isActive = true
            },
            new()
            {
                collection = collections[5],
                name = "Нуарный сценический образ",
                description = "Создание образа для съемки или тематического мероприятия.",
                durationMinutes = 260,
                basePrice = 7600m,
                isActive = true
            }
        };

        var adminUser = new ApplicationUser
        {
            email = "admin@mathie.local",
            passwordHash = "demo-hash",
            fullName = "Администратор Матье",
            phone = "+7-900-000-00-01",
            isActive = true
        };

        var moderatorUser = new ApplicationUser
        {
            email = "moderator@mathie.local",
            passwordHash = "demo-hash",
            fullName = "Модератор каталога",
            phone = "+7-900-000-00-02",
            isActive = true
        };

        var masterUser = new ApplicationUser
        {
            email = "master@mathie.local",
            passwordHash = "demo-hash",
            fullName = "Мастер Виктория",
            phone = "+7-900-000-00-03",
            isActive = true
        };

        var clientUser = new ApplicationUser
        {
            email = "client@mathie.local",
            passwordHash = "demo-hash",
            fullName = "Клиент Анна",
            phone = "+7-900-000-00-04",
            isActive = true
        };

        var wallet = new Wallet
        {
            user = clientUser,
            currencyCode = "RUB"
        };

        var paymentCard = new PaymentCard
        {
            user = clientUser,
            maskedPan = "2200********1234",
            paymentToken = "demo-card-token",
            holderName = "ANNA CLIENT",
            expMonth = 12,
            expYear = 2028,
            isDefault = true
        };

        var topUp = new BalanceTopUp
        {
            wallet = wallet,
            card = paymentCard,
            amount = 5000m,
            status = Domain.Enums.TopUpStatus.Completed,
            externalPaymentId = "demo-topup-1",
            completedAt = DateTime.UtcNow
        };

        var masterProfile = new MasterProfile
        {
            user = masterUser,
            qualificationLevel = middleLevel,
            bio = "Специалист по косплею, кастомизации и праздничным коллекциям.",
            experienceYears = 5,
            isActive = true
        };

        var masterService = new MasterService
        {
            masterProfile = masterProfile,
            service = services[0],
            customPrice = 4900m,
            isActive = true
        };

        var appointment = new Appointment
        {
            clientUser = clientUser,
            masterService = masterService,
            startsAt = DateTime.UtcNow.AddDays(2),
            endsAt = DateTime.UtcNow.AddDays(2).AddHours(3),
            status = Domain.Enums.AppointmentStatus.Confirmed,
            clientComment = "Нужен образ к тематическому фестивалю."
        };

        var review = new Review
        {
            appointment = appointment,
            authorUser = clientUser,
            rating = 5,
            text = "Очень понравилось качество подготовки и внимательность мастера.",
            status = Domain.Enums.ReviewStatus.Published
        };

        var qualificationApplication = new QualificationApplication
        {
            masterProfile = masterProfile,
            requestedLevel = seniorLevel,
            reviewedByUser = adminUser,
            status = Domain.Enums.QualificationApplicationStatus.Pending,
            comment = "Прошу рассмотреть повышение квалификации после завершения серии заказов."
        };

        dbContext.Roles.AddRange(userRole, moderatorRole, adminRole, masterRole);
        dbContext.QualificationLevels.AddRange(juniorLevel, middleLevel, seniorLevel);
        dbContext.Directions.AddRange(animeDirection, newYearDirection, halloweenDirection, cyberpunkDirection, noirDirection);
        dbContext.Collections.AddRange(collections);
        dbContext.Services.AddRange(services);
        dbContext.Users.AddRange(adminUser, moderatorUser, masterUser, clientUser);
        dbContext.Wallets.Add(wallet);
        dbContext.PaymentCards.Add(paymentCard);
        dbContext.BalanceTopUps.Add(topUp);
        dbContext.MasterProfiles.Add(masterProfile);
        dbContext.MasterServices.Add(masterService);
        dbContext.Appointments.Add(appointment);
        dbContext.Reviews.Add(review);
        dbContext.QualificationApplications.Add(qualificationApplication);

        dbContext.UserRoles.AddRange(
            new UserRole { user = adminUser, role = adminRole },
            new UserRole { user = moderatorUser, role = moderatorRole },
            new UserRole { user = masterUser, role = masterRole },
            new UserRole { user = clientUser, role = userRole });

        await dbContext.SaveChangesAsync();
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
