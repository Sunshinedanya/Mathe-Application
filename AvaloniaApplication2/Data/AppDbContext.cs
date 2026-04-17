using AvaloniaApplication2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AvaloniaApplication2.Data;

public sealed class AppDbContext : DbContext
{
    public DbSet<Role> Roles => Set<Role>();

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<Direction> Directions => Set<Direction>();

    public DbSet<Collection> Collections => Set<Collection>();

    public DbSet<ShopService> Services => Set<ShopService>();

    public DbSet<QualificationLevel> QualificationLevels => Set<QualificationLevel>();

    public DbSet<MasterProfile> MasterProfiles => Set<MasterProfile>();

    public DbSet<MasterService> MasterServices => Set<MasterService>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<Wallet> Wallets => Set<Wallet>();

    public DbSet<PaymentCard> PaymentCards => Set<PaymentCard>();

    public DbSet<BalanceTopUp> BalanceTopUps => Set<BalanceTopUp>();

    public DbSet<QualificationApplication> QualificationApplications => Set<QualificationApplication>();

    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
        {
            return;
        }

        var connectionString =
            "Host=localhost;Port=5432;Database=matheDb;Username=postgres;Password=44122144";

        optionsBuilder.UseNpgsql(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.code)
                .HasMaxLength(64)
                .IsRequired();

            entity.Property(item => item.name)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(item => item.createdAt)
                .IsRequired();

            entity.Property(item => item.lastModifiedAt)
                .IsRequired();

            entity.HasIndex(item => item.code)
                .IsUnique();
        });

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.email)
                .HasMaxLength(256)
                .IsRequired();

            entity.Property(item => item.passwordHash)
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(item => item.fullName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(item => item.phone)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(item => item.isActive)
                .IsRequired();

            entity.Property(item => item.createdAt)
                .IsRequired();

            entity.Property(item => item.lastModifiedAt)
                .IsRequired();

            entity.HasIndex(item => item.email)
                .IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.createdAt)
                .IsRequired();

            entity.Property(item => item.lastModifiedAt)
                .IsRequired();

            entity.HasIndex(item => new { item.userId, item.roleId })
                .IsUnique();

            entity.HasOne(item => item.user)
                .WithMany(item => item.userRoles)
                .HasForeignKey(item => item.userId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.role)
                .WithMany(item => item.userRoles)
                .HasForeignKey(item => item.roleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Direction>(entity =>
        {
            entity.ToTable("directions");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.name)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(item => item.description)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(item => item.createdAt)
                .IsRequired();

            entity.Property(item => item.lastModifiedAt)
                .IsRequired();

            entity.HasIndex(item => item.name)
                .IsUnique();
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.ToTable("collections");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.name)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(item => item.description)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(item => item.createdAt)
                .IsRequired();

            entity.Property(item => item.lastModifiedAt)
                .IsRequired();

            entity.HasIndex(item => new { item.directionId, item.name })
                .IsUnique();

            entity.HasOne(item => item.direction)
                .WithMany(item => item.collections)
                .HasForeignKey(item => item.directionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShopService>(entity =>
        {
            entity.ToTable("services");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.name)
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(item => item.description)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(item => item.durationMinutes)
                .IsRequired();

            entity.Property(item => item.basePrice)
                .HasColumnType("numeric(10,2)")
                .IsRequired();

            entity.Property(item => item.isActive)
                .IsRequired();

            entity.Property(item => item.imagePath)
                .HasMaxLength(400);

            entity.Property(item => item.createdAt)
                .IsRequired();

            entity.Property(item => item.lastModifiedAt)
                .IsRequired();

            entity.HasIndex(item => new { item.collectionId, item.name })
                .IsUnique();

            entity.HasOne(item => item.collection)
                .WithMany(item => item.services)
                .HasForeignKey(item => item.collectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QualificationLevel>(entity =>
        {
            entity.ToTable("qualification_levels");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.name)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(item => item.sortOrder)
                .IsRequired();

            entity.Property(item => item.createdAt)
                .IsRequired();

            entity.Property(item => item.lastModifiedAt)
                .IsRequired();

            entity.HasIndex(item => item.name)
                .IsUnique();
        });

        modelBuilder.Entity<MasterProfile>(entity =>
        {
            entity.ToTable("master_profiles");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.bio)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(item => item.experienceYears)
                .IsRequired();

            entity.Property(item => item.isActive)
                .IsRequired();

            entity.Property(item => item.createdAt)
                .IsRequired();

            entity.Property(item => item.lastModifiedAt)
                .IsRequired();

            entity.HasIndex(item => item.userId)
                .IsUnique();

            entity.HasOne(item => item.user)
                .WithOne(item => item.masterProfile)
                .HasForeignKey<MasterProfile>(item => item.userId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.qualificationLevel)
                .WithMany(item => item.masterProfiles)
                .HasForeignKey(item => item.qualificationLevelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MasterService>(entity =>
        {
            entity.ToTable("master_services");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.customPrice)
                .HasColumnType("numeric(10,2)");

            entity.Property(item => item.isActive)
                .IsRequired();

            entity.Property(item => item.createdAt)
                .IsRequired();

            entity.Property(item => item.lastModifiedAt)
                .IsRequired();

            entity.HasIndex(item => new { item.masterProfileId, item.serviceId })
                .IsUnique();

            entity.HasOne(item => item.masterProfile)
                .WithMany(item => item.masterServices)
                .HasForeignKey(item => item.masterProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.service)
                .WithMany(item => item.masterServices)
                .HasForeignKey(item => item.serviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("appointments");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.startsAt)
                .IsRequired();

            entity.Property(item => item.endsAt)
                .IsRequired();

            entity.Property(item => item.status)
                .IsRequired();

            entity.Property(item => item.clientComment)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(item => item.createdAt)
                .IsRequired();

            entity.Property(item => item.lastModifiedAt)
                .IsRequired();

            entity.HasOne(item => item.clientUser)
                .WithMany(item => item.clientAppointments)
                .HasForeignKey(item => item.clientUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(item => item.masterService)
                .WithMany(item => item.appointments)
                .HasForeignKey(item => item.masterServiceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("reviews");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.rating)
                .IsRequired();

            entity.Property(item => item.text)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(item => item.status)
                .IsRequired();

            entity.Property(item => item.createdAt)
                .IsRequired();

            entity.Property(item => item.lastModifiedAt)
                .IsRequired();

            entity.HasIndex(item => item.appointmentId)
                .IsUnique();

            entity.HasOne(item => item.appointment)
                .WithOne(item => item.review)
                .HasForeignKey<Review>(item => item.appointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.authorUser)
                .WithMany(item => item.reviews)
                .HasForeignKey(item => item.authorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.ToTable("wallets");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.currencyCode)
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(item => item.createdAt)
                .IsRequired();

            entity.Property(item => item.lastModifiedAt)
                .IsRequired();

            entity.HasIndex(item => item.userId)
                .IsUnique();

            entity.HasOne(item => item.user)
                .WithOne(item => item.wallet)
                .HasForeignKey<Wallet>(item => item.userId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PaymentCard>(entity =>
        {
            entity.ToTable("payment_cards");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.maskedPan)
                .HasMaxLength(32)
                .IsRequired();

            entity.Property(item => item.paymentToken)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(item => item.holderName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(item => item.expMonth)
                .IsRequired();

            entity.Property(item => item.expYear)
                .IsRequired();

            entity.Property(item => item.isDefault)
                .IsRequired();

            entity.Property(item => item.createdAt)
                .IsRequired();

            entity.Property(item => item.lastModifiedAt)
                .IsRequired();

            entity.HasIndex(item => item.paymentToken)
                .IsUnique();

            entity.HasOne(item => item.user)
                .WithMany(item => item.paymentCards)
                .HasForeignKey(item => item.userId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BalanceTopUp>(entity =>
        {
            entity.ToTable("balance_topups");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.amount)
                .HasColumnType("numeric(10,2)")
                .IsRequired();

            entity.Property(item => item.status)
                .IsRequired();

            entity.Property(item => item.externalPaymentId)
                .HasMaxLength(128);

            entity.Property(item => item.completedAt);

            entity.Property(item => item.createdAt)
                .IsRequired();

            entity.Property(item => item.lastModifiedAt)
                .IsRequired();

            entity.HasOne(item => item.wallet)
                .WithMany(item => item.balanceTopUps)
                .HasForeignKey(item => item.walletId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.card)
                .WithMany(item => item.balanceTopUps)
                .HasForeignKey(item => item.cardId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<QualificationApplication>(entity =>
        {
            entity.ToTable("qualification_applications");
            entity.HasKey(item => item.Id);

            entity.Property(item => item.status)
                .IsRequired();

            entity.Property(item => item.comment)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(item => item.reviewedAt);

            entity.Property(item => item.createdAt)
                .IsRequired();

            entity.Property(item => item.lastModifiedAt)
                .IsRequired();

            entity.HasOne(item => item.masterProfile)
                .WithMany(item => item.qualificationApplications)
                .HasForeignKey(item => item.masterProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.requestedLevel)
                .WithMany(item => item.qualificationApplications)
                .HasForeignKey(item => item.requestedLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(item => item.reviewedByUser)
                .WithMany(item => item.reviewedQualificationApplications)
                .HasForeignKey(item => item.reviewedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    public override int SaveChanges()
    {
        updateAuditFields();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        updateAuditFields();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        updateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        updateAuditFields();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void updateAuditFields()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.createdAt = utcNow;
                entry.Entity.lastModifiedAt = utcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(item => item.createdAt).IsModified = false;
                entry.Entity.lastModifiedAt = utcNow;
            }
        }
    }
}
