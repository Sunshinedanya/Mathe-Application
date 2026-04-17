using AvaloniaApplication2.Data;
using AvaloniaApplication2.Domain.Entities;
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
        await appDbContext.Database.EnsureCreatedAsync(cancellationToken);

        if (await appDbContext.Directions.AnyAsync(cancellationToken))
        {
            return;
        }

        await SeedDemoDataAsync(cancellationToken);
    }

    private async Task SeedDemoDataAsync(CancellationToken cancellationToken)
    {
        var moderatorRole = new Role
        {
            code = "moderator",
            name = "Модератор"
        };

        var userRole = new Role
        {
            code = "user",
            name = "Пользователь"
        };

        var adminRole = new Role
        {
            code = "admin",
            name = "Администратор"
        };

        var masterRole = new Role
        {
            code = "master",
            name = "Мастер"
        };

        var juniorLevel = new QualificationLevel
        {
            name = "Junior",
            sortOrder = 1
        };

        var middleLevel = new QualificationLevel
        {
            name = "Middle",
            sortOrder = 2
        };

        var seniorLevel = new QualificationLevel
        {
            name = "Senior",
            sortOrder = 3
        };

        var animeDirection = new Direction
        {
            name = "Аниме",
            description = "Коллекции и услуги в стилистике популярных аниме-вселенных."
        };

        var newYearDirection = new Direction
        {
            name = "Новый год",
            description = "Праздничные образы, аксессуары и тематические кастомные решения."
        };

        var halloweenDirection = new Direction
        {
            name = "Хэллоуин",
            description = "Косплей, декор и образы для хэллоуинских мероприятий."
        };

        var cyberpunkDirection = new Direction
        {
            name = "Киберпанк",
            description = "Неоновые коллекции, футуристический реквизит и стилизация."
        };

        var noirDirection = new Direction
        {
            name = "Нуар",
            description = "Темные, атмосферные и стилизованные коллекции в жанре нуар."
        };

        var animeCollection = new Collection
        {
            name = "Shonen Heroes",
            description = "Коллекция ярких аниме-образов для фестивалей и фотосессий.",
            direction = animeDirection
        };

        var newYearCollection = new Collection
        {
            name = "Winter Sparkle",
            description = "Новогодняя коллекция костюмов, аксессуаров и реквизита.",
            direction = newYearDirection
        };

        var halloweenCollection = new Collection
        {
            name = "Night Parade",
            description = "Коллекция хэллоуинских образов и пугающих персонажей.",
            direction = halloweenDirection
        };

        var cyberpunkCollection = new Collection
        {
            name = "Neon City",
            description = "Коллекция футуристических костюмов и элементов кастомизации.",
            direction = cyberpunkDirection
        };

        var noirCollection = new Collection
        {
            name = "Black Velvet",
            description = "Коллекция изысканных и контрастных нуар-образов.",
            direction = noirDirection
        };

        var costumeTailoringService = new ShopService
        {
            name = "Пошив кастомного косплея",
            description = "Индивидуальный пошив косплей-костюма по референсам клиента.",
            durationMinutes = 4320,
            basePrice = 18000m,
            isActive = true,
            collection = animeCollection
        };

        var accessoryCustomizationService = new ShopService
        {
            name = "Кастомизация аксессуаров",
            description = "Окрашивание, доработка и стилизация аксессуаров под образ.",
            durationMinutes = 480,
            basePrice = 4500m,
            isActive = true,
            collection = cyberpunkCollection
        };

        var holidayLookService = new ShopService
        {
            name = "Создание праздничного образа",
            description = "Комплексная подготовка костюма и деталей к празднику.",
            durationMinutes = 720,
            basePrice = 8500m,
            isActive = true,
            collection = newYearCollection
        };

        var halloweenPropsService = new ShopService
        {
            name = "Подготовка хэллоуинского реквизита",
            description = "Изготовление и кастомизация реквизита для тематических мероприятий.",
            durationMinutes = 960,
            basePrice = 9200m,
            isActive = true,
            collection = halloweenCollection
        };

        var noirStylingService = new ShopService
        {
            name = "Нуар-стилизация костюма",
            description = "Доработка костюма, тканей и деталей под эстетику нуара.",
            durationMinutes = 600,
            basePrice = 6700m,
            isActive = true,
            collection = noirCollection
        };

        var moderatorUser = new ApplicationUser
        {
            email = "moderator@mathie.local",
            passwordHash = "demo_hash_moderator",
            fullName = "Марина Модератор",
            phone = "+79990000001",
            isActive = true
        };

        var adminUser = new ApplicationUser
        {
            email = "admin@mathie.local",
            passwordHash = "demo_hash_admin",
            fullName = "Александр Администратор",
            phone = "+79990000002",
            isActive = true
        };

        var masterUser = new ApplicationUser
        {
            email = "master@mathie.local",
            passwordHash = "demo_hash_master",
            fullName = "Ева Мастер",
            phone = "+79990000003",
            isActive = true
        };

        var clientUser = new ApplicationUser
        {
            email = "client@mathie.local",
            passwordHash = "demo_hash_client",
            fullName = "Анна Клиент",
            phone = "+79990000004",
            isActive = true
        };

        var moderatorUserRole = new UserRole
        {
            user = moderatorUser,
            role = moderatorRole
        };

        var adminUserRole = new UserRole
        {
            user = adminUser,
            role = adminRole
        };

        var masterUserRole = new UserRole
        {
            user = masterUser,
            role = masterRole
        };

        var clientUserRole = new UserRole
        {
            user = clientUser,
            role = userRole
        };

        var masterProfile = new MasterProfile
        {
            user = masterUser,
            qualificationLevel = middleLevel,
            bio = "Специалист по созданию косплея и тематической кастомизации.",
            experienceYears = 4,
            isActive = true
        };

        var masterServiceAnime = new MasterService
        {
            masterProfile = masterProfile,
            service = costumeTailoringService,
            customPrice = 19500m,
            isActive = true
        };

        var masterServiceCyberpunk = new MasterService
        {
            masterProfile = masterProfile,
            service = accessoryCustomizationService,
            customPrice = 4900m,
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
            maskedPan = "2200 **** **** 1234",
            paymentToken = "demo-token-001",
            holderName = "ANNA CLIENT",
            expMonth = 12,
            expYear = DateTime.UtcNow.Year + 2,
            isDefault = true
        };

        var balanceTopUp = new BalanceTopUp
        {
            wallet = wallet,
            card = paymentCard,
            amount = 5000m,
            status = Domain.Enums.TopUpStatus.Completed,
            externalPaymentId = "demo-topup-001",
            completedAt = DateTime.UtcNow
        };

        var appointment = new Appointment
        {
            clientUser = clientUser,
            masterService = masterServiceAnime,
            startsAt = DateTime.UtcNow.AddDays(3),
            endsAt = DateTime.UtcNow.AddDays(3).AddHours(2),
            status = Domain.Enums.AppointmentStatus.Confirmed,
            clientComment = "Нужен образ для фестиваля аниме-культуры."
        };

        var review = new Review
        {
            appointment = appointment,
            authorUser = clientUser,
            rating = 5,
            text = "Очень внимательный мастер, помог подобрать детали образа.",
            status = Domain.Enums.ReviewStatus.Published
        };

        var qualificationApplication = new QualificationApplication
        {
            masterProfile = masterProfile,
            requestedLevel = seniorLevel,
            reviewedByUser = adminUser,
            status = Domain.Enums.QualificationApplicationStatus.Pending,
            comment = "Прошу рассмотреть повышение квалификации после завершенных проектов.",
            reviewedAt = null
        };

        appDbContext.Roles.AddRange(moderatorRole, userRole, adminRole, masterRole);
        appDbContext.QualificationLevels.AddRange(juniorLevel, middleLevel, seniorLevel);
        appDbContext.Directions.AddRange(
            animeDirection,
            newYearDirection,
            halloweenDirection,
            cyberpunkDirection,
            noirDirection);

        appDbContext.Collections.AddRange(
            animeCollection,
            newYearCollection,
            halloweenCollection,
            cyberpunkCollection,
            noirCollection);

        appDbContext.Services.AddRange(
            costumeTailoringService,
            accessoryCustomizationService,
            holidayLookService,
            halloweenPropsService,
            noirStylingService);

        appDbContext.Users.AddRange(moderatorUser, adminUser, masterUser, clientUser);
        appDbContext.UserRoles.AddRange(
            moderatorUserRole,
            adminUserRole,
            masterUserRole,
            clientUserRole);

        appDbContext.MasterProfiles.Add(masterProfile);
        appDbContext.MasterServices.AddRange(masterServiceAnime, masterServiceCyberpunk);
        appDbContext.Wallets.Add(wallet);
        appDbContext.PaymentCards.Add(paymentCard);
        appDbContext.BalanceTopUps.Add(balanceTopUp);
        appDbContext.Appointments.Add(appointment);
        appDbContext.Reviews.Add(review);
        appDbContext.QualificationApplications.Add(qualificationApplication);

        await appDbContext.SaveChangesAsync(cancellationToken);
    }
}
