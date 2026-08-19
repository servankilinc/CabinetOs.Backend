using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CabinetOs.Business.Abstract;
using CabinetOs.Business.Concrete;
using CabinetOs.Business.Utils.TokenService;

namespace CabinetOs.Business
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();

            #region ENTITY SERVICES
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<ICabinetService, CabinetService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IRolePermissionService, RolePermissionService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<IDeviceCommandService, DeviceCommandService>();
            services.AddScoped<IConnectionService, ConnectionService>();
            services.AddScoped<IIoChannelService, IoChannelService>();
            services.AddScoped<IPinService, PinService>();
            services.AddScoped<ICanvasSettingsService, CanvasSettingsService>();
            services.AddScoped<IComponentTemplateService, ComponentTemplateService>();
            services.AddScoped<IComponentTemplatePinService, ComponentTemplatePinService>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IDiagramAnnotationService, DiagramAnnotationService>();
            services.AddScoped<IDeviceStatusService, DeviceStatusService>();
            services.AddScoped<IDeviceTypeService, DeviceTypeService>();
            #endregion
            return services;
        }
    }
}