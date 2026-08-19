using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CabinetOs.Model.Entities;
using CabinetOs.Model.ProjectEntities;

namespace CabinetOs.DataAccess.Contexts
{
    public class AppDbContext : IdentityDbContext<User, Role, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Cabinet> Cabinets { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public  override  DbSet < User > Users { get; set; }
        public  override  DbSet < Role > Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
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
                c.HasMany(c => c.Users).WithOne(u => u.Comany).HasForeignKey(u => u.CompanyId).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Cabinet>(c =>
            {
                c.ToTable("Cabinet");
                c.HasKey(c => c.Id);
                c.HasMany(c => c.Devices).WithOne(d => d.Cabinet).HasForeignKey(d => d.CabinetId).OnDelete(DeleteBehavior.Restrict);
                c.HasMany(c => c.DiagramAnnotations).WithOne(d => d.Cabinet).HasForeignKey(d => d.CabinetId).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<AuditLog>(a =>
            {
                a.ToTable("AuditLog");
                a.HasKey(a => a.Id);
            });
            modelBuilder.Entity<User>(u =>
            {
                u.ToTable("User");
                u.HasKey(u => u.Id);
                u.HasMany(u => u.UserRoles).WithOne(u => u.User).HasForeignKey(u => u.UserId).OnDelete(DeleteBehavior.Cascade);
                u.HasMany(u => u.DeviceCommands).WithOne(d => d.RequesterUser).HasForeignKey(d => d.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
                u.HasMany(u => u.RefreshTokens).WithOne(r => r.User).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Role>(r =>
            {
                r.ToTable("Role");
                r.HasKey(r => r.Id);
                r.HasMany(r => r.UserRoles).WithOne(u => u.Role).HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Cascade);
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
            modelBuilder.Entity<UserRole>(u =>
            {
                u.ToTable("UserRole");
                u.HasKey(u => new { u.UserId, u.RoleId });
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
                i.HasMany(i => i.Pins).WithOne(p => p.IoChanel).HasForeignKey(p => p.IoChannelId).OnDelete(DeleteBehavior.Restrict);
                i.HasMany(i => i.DeviceCommands).WithOne(d => d.IoChanel).HasForeignKey(d => d.IoChannelId).OnDelete(DeleteBehavior.Restrict);
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
                d.HasMany(d => d.IoChanels).WithOne(i => i.Device).HasForeignKey(i => i.DeviceId).OnDelete(DeleteBehavior.Restrict);
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
                d.HasMany(d => d.Cabinets).WithOne(c => c.DeviceStatus).HasForeignKey(c => c.DeviceStatusId).OnDelete(DeleteBehavior.Restrict);
                d.HasMany(d => d.Devices).WithOne(d => d.DeviceStatus).HasForeignKey(d => d.DeviceStatusId).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<DeviceType>(d =>
            {
                d.ToTable("DeviceType");
                d.HasKey(d => d.Id);
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
            }); modelBuilder . Entity < Archive > ( a  =>  { a . ToTable ( "ProjectArchives" ) ;  a . HasKey ( a  =>  a . Id ) ;  } ) ; 
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
        }
    }
}