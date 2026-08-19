using CabinetOs.Model.Entities;
using CabinetOs.Model.Enums;
using CabinetOs.Model.ProjectEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CabinetOs.DataAccess.Contexts;

public class AppDbContext : IdentityDbContext<User, Role, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies { get; set; }
    public DbSet<Cabinet> Cabinets { get; set; }
    public override DbSet<User> Users { get; set; }
    public override DbSet<Role> Roles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<DeviceCommand> DeviceCommands { get; set; }
    public DbSet<Connection> Connections { get; set; }
    public DbSet<IoChannel> IoChannels { get; set; }
    public DbSet<Pin> Pins { get; set; }
    public DbSet<CanvasSettings> CanvasSettings { get; set; }
    public DbSet<ComponentTemplate> ComponentTemplates { get; set; }
    public DbSet<ComponentTemplatePin> ComponentTemplatePins { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<DiagramAnnotation> DiagramAnnotations { get; set; }
    public DbSet<DeviceStatus> DeviceStatuses { get; set; }
    public DbSet<DeviceType> DeviceTypes { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Log> Logs { get; set; }
    public DbSet<Archive> Archives { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Company>(c =>
        {
            c.ToTable("Company");
            c.HasKey(c => c.Id);
            c.HasMany(c => c.Cabinets).WithOne(c => c.Company).HasForeignKey(c => c.CompanyId).OnDelete(DeleteBehavior.Restrict);
            c.HasMany(c => c.Users).WithOne(u => u.Company).HasForeignKey(u => u.CompanyId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<Cabinet>(c =>
        {
            c.ToTable("Cabinet");
            c.HasKey(c => c.Id);
            c.HasMany(c => c.Devices).WithOne(d => d.Cabinet).HasForeignKey(d => d.CabinetId).OnDelete(DeleteBehavior.Restrict);
            c.HasMany(c => c.DiagramAnnotations).WithOne(d => d.Cabinet).HasForeignKey(d => d.CabinetId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<User>(u =>
        {
            u.ToTable("User");
            u.HasKey(u => u.Id);
            u.HasMany(u => u.DeviceCommands).WithOne(d => d.RequesterUser).HasForeignKey(d => d.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
            u.HasMany(u => u.RefreshTokens).WithOne(r => r.User).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Role>(r =>
        {
            r.ToTable("Role");
            r.HasKey(r => r.Id);
            r.HasMany(r => r.RolePermissions).WithOne(r => r.Role).HasForeignKey(r => r.RoleId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<RolePermission>(r =>
        {
            r.ToTable("RolePermission");
            r.HasKey(r => new { r.RoleId, r.PermissionId });
        });
        modelBuilder.Entity<Permission>(p =>
        {
            p.ToTable("Permission");
            p.HasKey(p => p.Id);
            p.HasMany(p => p.RolePermissions).WithOne(r => r.Permission).HasForeignKey(r => r.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<DeviceCommand>(d =>
        {
            d.ToTable("DeviceCommand");
            d.HasKey(d => d.Id);
            d.HasQueryFilter(f => !f.IsDeleted);
        });
        modelBuilder.Entity<Connection>(c =>
        {
            c.ToTable("Connection");
            c.HasKey(c => c.Id);
            c.HasQueryFilter(f => !f.IsDeleted);
        });
        modelBuilder.Entity<IoChannel>(i =>
        {
            i.ToTable("IoChannel");
            i.HasKey(i => i.Id);
            i.HasMany(i => i.Pins).WithOne(p => p.IoChannel).HasForeignKey(p => p.IoChannelId).OnDelete(DeleteBehavior.Restrict);
            i.HasMany(i => i.DeviceCommands).WithOne(d => d.IoChannel).HasForeignKey(d => d.IoChannelId).OnDelete(DeleteBehavior.Restrict);
            i.HasQueryFilter(f => !f.IsDeleted);
        });
        modelBuilder.Entity<Pin>(p =>
        {
            p.ToTable("Pin");
            p.HasKey(p => p.Id);
            p.HasMany(p => p.SourcePinConnections).WithOne(c => c.SourcePin).HasForeignKey(c => c.SourcePinId).OnDelete(DeleteBehavior.Restrict);
            p.HasMany(p => p.TargetPinConnections).WithOne(c => c.TargetPin).HasForeignKey(c => c.TargetPinId).OnDelete(DeleteBehavior.Restrict);
            p.HasQueryFilter(f => !f.IsDeleted);
        });
        modelBuilder.Entity<CanvasSettings>(c =>
        {
            c.ToTable("CanvasSettings");
            c.HasKey(c => c.Id);
        });
        modelBuilder.Entity<ComponentTemplate>(c =>
        {
            c.ToTable("ComponentTemplate");
            c.HasKey(c => c.Id);
            c.HasMany(c => c.ComponentTemplatePins).WithOne(c => c.ComponentTemplate).HasForeignKey(c => c.ComponentTemplateId).OnDelete(DeleteBehavior.Cascade);
            c.HasMany(c => c.Devices).WithOne(d => d.ComponentTemplate).HasForeignKey(d => d.ComponentTemplateId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ComponentTemplatePin>(c =>
        {
            c.ToTable("ComponentTemplatePin");
            c.HasKey(c => c.Id);
        });
        modelBuilder.Entity<Device>(d =>
        {
            d.ToTable("Device");
            d.HasKey(d => d.Id);
            d.HasMany(d => d.IoChannels).WithOne(i => i.Device).HasForeignKey(i => i.DeviceId).OnDelete(DeleteBehavior.Restrict);
            d.HasMany(d => d.Pins).WithOne(p => p.Device).HasForeignKey(p => p.DeviceId).OnDelete(DeleteBehavior.Restrict);
            d.HasMany(d => d.DeviceCommands).WithOne(d => d.Device).HasForeignKey(d => d.DeviceId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<DiagramAnnotation>(d =>
        {
            d.ToTable("DiagramAnnotation");
            d.HasKey(d => d.Id);
        });
        modelBuilder.Entity<DeviceStatus>(d =>
        {
            d.ToTable("DeviceStatus");
            d.HasKey(d => d.Id);
            // Id'ler EntityEnums.DeviceStatus degerlerine sabitlenmistir; IDENTITY uretmemelidir.
            d.Property(d => d.Id).ValueGeneratedNever();
            d.HasMany(d => d.Cabinets).WithOne(c => c.DeviceStatus).HasForeignKey(c => c.DeviceStatusId).OnDelete(DeleteBehavior.Restrict);
            d.HasMany(d => d.Devices).WithOne(d => d.DeviceStatus).HasForeignKey(d => d.DeviceStatusId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<DeviceType>(d =>
        {
            d.ToTable("DeviceType");
            d.HasKey(d => d.Id);
            // Id'ler EntityEnums.DeviceType degerlerine sabitlenmistir; IDENTITY uretmemelidir.
            d.Property(d => d.Id).ValueGeneratedNever();
            d.HasMany(d => d.ComponentTemplates).WithOne(c => c.DeviceType).HasForeignKey(c => c.DeviceTypeId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<RefreshToken>(r =>
        {
            r.HasKey(r => r.Id);
            r.HasOne(r => r.User).WithMany(u => u.RefreshTokens).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Log>(l =>
        {
            l.ToTable("ProjectLogs");
            l.HasKey(l => l.Id);
        });
        modelBuilder.Entity<Archive>(a =>
        {
            a.ToTable("ProjectArchives");
            a.HasKey(a => a.Id);
        });
        modelBuilder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("UserClaims");
        });
        modelBuilder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("UserLogins");
        });
        modelBuilder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("RoleClaims");
        });
        modelBuilder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("UserRoles");
        });
        modelBuilder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("UserTokens");
        });

        SeedData(modelBuilder);
    }


    private static void SeedData(ModelBuilder modelBuilder)
    {
        #region DEVICE STATUS
        // Renk ve ikon frontend'in rozet/durum gostergesini cizebilmesi icindir.
        modelBuilder.Entity<DeviceStatus>().HasData(
            new DeviceStatus
            {
                Id = (int)EntityEnums.DeviceStatus.Offline,
                Name = nameof(EntityEnums.DeviceStatus.Offline),
                Color = "#6B7280",
                Icon = "wifi-off",
                Description = "Cihaza ulasilamiyor."
            },
            new DeviceStatus
            {
                Id = (int)EntityEnums.DeviceStatus.Online,
                Name = nameof(EntityEnums.DeviceStatus.Online),
                Color = "#22C55E",
                Icon = "wifi",
                Description = "Cihaz calisiyor ve haberlesiyor."
            },
            new DeviceStatus
            {
                Id = (int)EntityEnums.DeviceStatus.Warning,
                Name = nameof(EntityEnums.DeviceStatus.Warning),
                Color = "#F59E0B",
                Icon = "alert-triangle",
                Description = "Cihaz calisiyor ancak dikkat gerektiren bir durum var."
            },
            new DeviceStatus
            {
                Id = (int)EntityEnums.DeviceStatus.Critical,
                Name = nameof(EntityEnums.DeviceStatus.Critical),
                Color = "#EF4444",
                Icon = "alert-octagon",
                Description = "Kritik ariza; mudahale gerekiyor."
            },
            new DeviceStatus
            {
                Id = (int)EntityEnums.DeviceStatus.Maintenance,
                Name = nameof(EntityEnums.DeviceStatus.Maintenance),
                Color = "#3B82F6",
                Icon = "wrench",
                Description = "Bakim modunda; alarmlari bastirilir."
            }
        );
        #endregion


        #region DEVICE TYPE
        // Category, Toolbox'ta cihazlarin hangi grup altinda listelenecegini belirler.
        modelBuilder.Entity<DeviceType>().HasData(
            new DeviceType { Id = (int)EntityEnums.DeviceType.ControlModule, Name = nameof(EntityEnums.DeviceType.ControlModule), Category = "Module" },
            new DeviceType { Id = (int)EntityEnums.DeviceType.InputModule, Name = nameof(EntityEnums.DeviceType.InputModule), Category = "Module" },
            new DeviceType { Id = (int)EntityEnums.DeviceType.OutputModule, Name = nameof(EntityEnums.DeviceType.OutputModule), Category = "Module" },
            new DeviceType { Id = (int)EntityEnums.DeviceType.LedModule, Name = nameof(EntityEnums.DeviceType.LedModule), Category = "Module" },
            new DeviceType { Id = (int)EntityEnums.DeviceType.TerminalBlock, Name = nameof(EntityEnums.DeviceType.TerminalBlock), Category = "Passive" },
            new DeviceType { Id = (int)EntityEnums.DeviceType.Sensor, Name = nameof(EntityEnums.DeviceType.Sensor), Category = "Field" },
            new DeviceType { Id = (int)EntityEnums.DeviceType.Peripheral, Name = nameof(EntityEnums.DeviceType.Peripheral), Category = "Field" },
            new DeviceType { Id = (int)EntityEnums.DeviceType.PowerSupply, Name = nameof(EntityEnums.DeviceType.PowerSupply), Category = "Power" },
            new DeviceType { Id = (int)EntityEnums.DeviceType.MeasurementDevice, Name = nameof(EntityEnums.DeviceType.MeasurementDevice), Category = "Measurement" },
            new DeviceType { Id = (int)EntityEnums.DeviceType.CardReader, Name = nameof(EntityEnums.DeviceType.CardReader), Category = "Field" },
            new DeviceType { Id = (int)EntityEnums.DeviceType.Mains, Name = nameof(EntityEnums.DeviceType.Mains), Category = "Power" },
            new DeviceType { Id = (int)EntityEnums.DeviceType.CircuitBreaker, Name = nameof(EntityEnums.DeviceType.CircuitBreaker), Category = "Power" }
        );
        #endregion


        #region PERMISSION
        modelBuilder.Entity<Permission>().HasData(
            new Permission
            {
                Id = (int)EntityEnums.Permission.ViewDiagram,
                Code = nameof(EntityEnums.Permission.ViewDiagram),
                DisplayName = "Diyagrami goruntule",
                Category = "Diagram"
            },
            new Permission
            {
                Id = (int)EntityEnums.Permission.EditDiagram,
                Code = nameof(EntityEnums.Permission.EditDiagram),
                DisplayName = "Diyagrami duzenle",
                Category = "Diagram"
            },
            new Permission
            {
                Id = (int)EntityEnums.Permission.ControlOutput,
                Code = nameof(EntityEnums.Permission.ControlOutput),
                DisplayName = "Cikis sur (role / kilit / siren)",
                Category = "Control"
            },
            new Permission
            {
                Id = (int)EntityEnums.Permission.AcknowledgeAlarm,
                Code = nameof(EntityEnums.Permission.AcknowledgeAlarm),
                DisplayName = "Alarm kabul et",
                Category = "Alarm"
            },
            new Permission
            {
                Id = (int)EntityEnums.Permission.ManageUsers,
                Code = nameof(EntityEnums.Permission.ManageUsers),
                DisplayName = "Kullanici yonet",
                Category = "Admin"
            },
            new Permission
            {
                Id = (int)EntityEnums.Permission.ConfigureSystem,
                Code = nameof(EntityEnums.Permission.ConfigureSystem),
                DisplayName = "Sistem ayarlarini yapilandir",
                Category = "Admin"
            },
            new Permission
            {
                Id = (int)EntityEnums.Permission.ViewCamera,
                Code = nameof(EntityEnums.Permission.ViewCamera),
                DisplayName = "Kamera goruntule",
                Category = "Diagram"
            },
            new Permission
            {
                Id = (int)EntityEnums.Permission.ExportData,
                Code = nameof(EntityEnums.Permission.ExportData),
                DisplayName = "Veri disari aktar",
                Category = "Data"
            },
            new Permission
            {
                Id = (int)EntityEnums.Permission.ManageWorkflow,
                Code = nameof(EntityEnums.Permission.ManageWorkflow),
                DisplayName = "Is akisi yonet",
                Category = "Admin"
            },
            new Permission
            {
                Id = (int)EntityEnums.Permission.ManageAccessCards,
                Code = nameof(EntityEnums.Permission.ManageAccessCards),
                DisplayName = "Gecis kartlarini yonet",
                Category = "Access"
            }
        );
        #endregion
    }
}
