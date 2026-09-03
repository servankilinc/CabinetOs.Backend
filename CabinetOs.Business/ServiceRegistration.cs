using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using CabinetOs.Business.Abstract;
using CabinetOs.Business.Concrete;
using CabinetOs.Business.Utils.TokenService;
using CabinetOs.Business.Utils.CameraProtocolProfile;
using CabinetOs.Business.Utils.ClipCaptureQueue;
using CabinetOs.Business.Utils.SnapshotGateway;
using CabinetOs.Business.Utils.ScadaCommandGateway;
using CabinetOs.Business.Utils.ScadaService;
using CabinetOs.Business.Utils.Diagram;

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

            services.AddScoped<IDiagramService, DiagramService>();
            services.AddScoped<IChannelEventService, ChannelEventService>();
            services.AddScoped<ICameraService, CameraService>();
            services.AddScoped<IScadaCommandGateway, ScadaCommandGateway>();

            #region KAMERA / MEDYA
            // Ayarlar baglanip SINGLETON olarak kaydediliyor — TokenSettings ve
            // CacheSettings ile ayni desen. IOptions<T> kod tabaninda hic
            // kullanilmiyor; tek bir yerde acmak "hangisi dogru" sorusunu kalici
            // hale getirirdi.
            services.AddSingleton(
                configuration.GetSection(Settings.MediaMtxSettings.SectionName).Get<Settings.MediaMtxSettings>()
                ?? new Settings.MediaMtxSettings());

            services.AddSingleton(
                configuration.GetSection(Settings.CameraCaptureSettings.SectionName).Get<Settings.CameraCaptureSettings>()
                ?? new Settings.CameraCaptureSettings());

            // Marka basina URL sablonlari. IEnumerable olarak cozulur: ikinci bir
            // marka geldiginde tek satirlik bir kayit yeter, cagri yerlerine
            // dokunulmaz (bkz. Camera.Manufacturer XML dokumani).
            services.AddSingleton<ICameraProtocolProfile, HikvisionProtocolProfile>();
            services.AddSingleton<ICameraProtocolProfileResolver, CameraProtocolProfileResolver>();

            services.AddScoped<ISnapshotGateway, IsapiSnapshotGateway>();

            // Klip kuyrugu SINGLETON olmak zorunda: uc onu doldurur, hosted
            // service bosaltir. Scoped olsaydi ikisi ayri kuyruk gorurdu.
            services.AddSingleton<IClipCaptureQueue, ClipCaptureQueue>();
            #endregion

            return services;
        }
    }
}