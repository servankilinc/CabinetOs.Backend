using CabinetOs.Business;
using CabinetOs.Business.Abstract;
using CabinetOs.Business.Concrete;
using CabinetOs.Business.Mappings;
using CabinetOs.Core;
using CabinetOs.Core.Utils;
using CabinetOs.Core.Utils.Auth;
using CabinetOs.DataAccess;
using CabinetOs.DataAccess.Contexts;
using CabinetOs.Model.Dtos.Cabinet.Commands;
using CabinetOs.Model.Entities;
using CabinetOs.WebAPI.BackgroundServices;
using CabinetOs.WebAPI.ExceptionHandler;
using CabinetOs.WebAPI.Hubs;
using CabinetOs.WebAPI.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);


#region ------- CORS -------
string[] allowedOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("policy_cors", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowCredentials()
            .AllowAnyMethod()
            .AllowAnyHeader()
            //.WithHeaders("Content-Type", "Authorization")
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});
#endregion


#region ------- Rate Limiter -------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Varsayilan davranis GOVDESIZ bir 429 dondurmekti; burada govdeye ayristirilabilir
    // bir ProblemDetails konuyor ki istemci kullaniciya anlamli bir mesaj gosterebilsin.
    //
    // GERI CEKILME SURESI BILEREK GONDERILMIYOR: ne `Retry-After` header'i ne de
    // govdede `retryAfterSeconds`. Istemci 429'u OTOMATIK TEKRAR DENEMIYOR (bkz.
    // Frontend `use-diagram-save.ts`) - tekrar denemeye kullanici karar veriyor.
    // Kullanilmayan bir sureyi yayinlamak, olmayan bir otomatizmi ima ederdi.
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        var problemDetails = new ProblemDetails
        {
            Type = $"problems/{nameof(StatusCodes.Status429TooManyRequests)}",
            Title = "Cok fazla istek gonderildi. Lutfen biraz bekleyip tekrar deneyin.",
            Status = StatusCodes.Status429TooManyRequests,
            Detail = string.Empty
        };
        problemDetails.Extensions["code"] = StatusCodes.Status429TooManyRequests;
        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    };

    options.AddPolicy("policy_rate_limiter", httpContext =>
    {
        string partitionKey = httpContext.User.Identity?.IsAuthenticated == true
            ? $"user:{httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? httpContext.User.Identity.Name}"
            : $"ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

        return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 50,
            Window = TimeSpan.FromSeconds(10),
            SegmentsPerWindow = 4,
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });

    // SCADA ingest'i AYRI bir politika ister ve bu politikanin VAR OLMASI sarttir:
    // ScadaController'daki [EnableRateLimiting("policy_scada_ingest")] tanimsiz bir
    // ada isaret ederse middleware InvalidOperationException atar ve uc her istekte
    // 500 doner. (Tam olarak bu olmustu — politika Program.cs yeniden yazilirken
    // dusmus, oznitelik yerinde kalmisti; ingest sessizce tamamen kirilmisti.)
    //
    // Neden ayri: varsayilan politika 50 istek/10 sn ve IP'ye gore partition ediyor.
    // SCADA tek bir sunucudur, dolayisiyla TUM kabinlerin telemetrisi tek partition'a
    // duser ve saniyede 5 istekte bogulur. Limit kabin sayisina gore olceklenmis.
    //
    // Partition yine IP: ingest [AllowAnonymous] oldugu icin kullanici kimligi yok.
    // Govdedeki cabinetId'ye gore bolumlendirmek, sahte Guid'lerle sinirsiz butce
    // uretmek demek olurdu.
    //
    // Butce appsettings'ten okunur (`Scada:RateLimitPermitsPer10Seconds`) —
    // sozlesme dokumaninda kabin sayisina gore olceklenebilir olarak ilan edilmis.
    int scadaPermitLimit = builder.Configuration.GetValue<int?>("Scada:RateLimitPermitsPer10Seconds") ?? 600;

    options.AddPolicy("policy_scada_ingest", httpContext =>
    {
        string partitionKey = $"scada-ip:{httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";

        return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = scadaPermitLimit,
            Window = TimeSpan.FromSeconds(10),
            SegmentsPerWindow = 4,
            // Kuyruk YOK: bekletilen bir telemetri paketi, reddedilenden kotudur —
            // sirasi geldiginde tasidigi deger coktan bayatlamis olur.
            QueueLimit = 0,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    });
});
#endregion


#region ------- Layer Registrations -------
builder.Services.AddCoreServices(builder);
builder.Services.AddDataAccessServices(builder.Configuration);
builder.Services.AddBusinessServices(builder.Configuration);
#endregion


#region ------- IDENTITY -------
builder.Services
    .AddIdentity<User, Role>(options =>
    {
        // Default Lockout settings.
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        options.SignIn.RequireConfirmedEmail = false;

        options.Password.RequiredLength = 4;
        options.Password.RequireDigit = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;

        options.User.RequireUniqueEmail = true;
        options.User.AllowedUserNameCharacters = "abcçdefgğhiıjklmnoöpqrsştuüvwxyzABCÇDEFGĞHIİJKLMNOÖPQRSŞTUÜVWXYZ0123456789-._@+/*|!,;:()&#?[] ";
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();
#endregion


#region ------- JWT Implementation -------
TokenSettings tokenSettings = builder.Configuration.GetSection("TokenSettings").Get<TokenSettings>()!;
if (string.IsNullOrWhiteSpace(tokenSettings.SecurityKey))
    throw new InvalidOperationException("TokenSettings:SecurityKey tanimli degil. 'dotnet user-secrets set \"TokenSettings:SecurityKey\" \"<64+ karakterlik gizli anahtar>\"' calistirin veya TokenSettings__SecurityKey ortam degiskenini ayarlayin.");
builder.Services.AddSingleton(tokenSettings);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidIssuer = tokenSettings.Issuer,
            ValidAudience = tokenSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(tokenSettings.SecurityKey)),
            // Default is 5 minutes of leeway, which keeps expired tokens usable for too long.
            ClockSkew = TimeSpan.FromMinutes(1),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };

        // WebSocket EL SIKISMASI Authorization HEADER'I TASIYAMAZ. Tarayicinin
        // WebSocket API'sinde ozel header verilemez; SignalR bu yuzden token'i
        // query string'e koyar. Bu kanca yazilmazsa hub SESSIZCE 401 verir ve
        // istemci tarafinda hata "baglanamadi"dan ibaret kalir.
        //
        // Yalnizca /hubs ile baslayan yollar icin: query string'deki token
        // tarayici gecmisine ve sunucu erisim loglarina duser, dolayisiyla bu
        // kabul normal API uclarina GENISLETILMEMELIDIR.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;

                return Task.CompletedTask;
            }
        };
    });
#endregion


#region ------- AutoMapper -------
builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);
#endregion


#region ------- FluentValidation -------
builder.Services.AddValidatorsFromAssembly(typeof(CabinetCreateDto).Assembly);
#endregion


builder.Services.AddExceptionHandler<ExceptionHandleMiddleware>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();

#region ------- SignalR / Canli veri -------
// Hub olay govdeleri REST yanitlariyla ayni sekilde serialize edilmek ZORUNDA:
// frontend ayni TypeScript tiplerini kullaniyor. Varsayilana birakilsaydi hub
// PascalCase, REST camelCase gonderir ve ayni alan iki adla gelirdi.
builder.Services
    .AddSignalR()
    .AddJsonProtocol(options => options.PayloadSerializerOptions.SetByProjectSettings());

// Business katmanindaki yayin portunun implementasyonu. Business AspNetCore'a
// referans vermedigi icin SignalR bilgisi bu katmanda kaliyor.
builder.Services.AddScoped<IDiagramNotifier, DiagramHubNotifier>();

// Sablon gorsellerinin diske yazilmasi. Business katmaninda DEGIL: IFormFile ve
// wwwroot barindirma detaylaridir ve DiagramService'i dosya sistemine baglamak
// onu test edilemez hale getirirdi.
builder.Services.AddSingleton<TemplateImageStore>();

// Push-only modelde "veri gelmiyor" durumunu yalnizca zaman tespit edebilir.
builder.Services.AddHostedService<StaleDeviceSweeper>();
#endregion


#region ------- Kumanda / SCADA istemcisi -------
// Adres GOVDEDE degil, cagri basina veriliyor: her kabinin kendi ScadaBaseUrl'i
// var, dolayisiyla BaseAddress ayarlanamaz.
//
// Timeout SONSUZ birakiliyor ve zaman asimini ScadaCommandGateway kendi
// CancellationTokenSource'uyla uyguluyor. Sebep ayirt edilebilirlik:
// HttpClient.Timeout da TaskCanceledException firlatir ve "SCADA yavas" ile
// "istek iptal edildi" ayni istisnaya duserdi — CommandStatus.NoResponse tam
// olarak bu ayrimin uzerine kurulu.
//
// Resilience/retry handler'i BILEREK TAKILI DEGIL: tekrarlanan bir role darbesi
// basarisiz bir komuttan daha kotudur (kilidi iki kez acar). Eklenmesi sessiz bir
// davranis degisikligi olur.
builder.Services.AddHttpClient(ScadaCommandGateway.HttpClientName, client =>
{
    client.Timeout = Timeout.InfiniteTimeSpan;
});
#endregion

// JSON davranisi framework varsayilanina birakilmiyor, ACIKCA sabitleniyor. Ayrintili gerekce ve frontend karsiliklari: CabinetOs.Core/Utils/ApiJsonOptions.cs
builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.SetByProjectSettings());

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<ScalarSecuritySchemeTransformer>();
});

var app = builder.Build();


app.UseExceptionHandler();

// Sablon arka plan gorselleri wwwroot altindan servis edilir.
//
// GUVENLIK — yuklenen SVG'ler icin: SVG script tasiyabilir ve uygulamayla AYNI
// origin'den servis edilmesi, dosyaya dogrudan gidildiginde depolanmis XSS
// demektir. Node'lar gorseli <img> ile ciziyor ve <img> icindeki SVG script
// calistirmaz; asil risk kullanicinin URL'ye dogrudan gitmesi. Asagidaki iki
// baslik tam olarak onu kapatiyor:
//   - CSP default-src 'none' : dosya tarayicida acilsa bile hicbir sey yukleyemez
//   - nosniff                : icerik tipi tahmin edilerek HTML gibi calistirilamaz
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.ContentSecurityPolicy = "default-src 'none'; style-src 'unsafe-inline'; sandbox";
        context.Context.Response.Headers.XContentTypeOptions = "nosniff";
    }
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("policy_cors");

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

// Varsayilan politika, KENDI politikasini belirtmemis her uca uygulanir.
//
// Duz `MapControllers().RequireRateLimiting(...)` yazilamaz: o cagri politikayi
// endpoint metadata'sina KONVANSIYON olarak ekler ve konvansiyonlar
// oznitelilerden SONRA calisir. Rate limiting middleware'i son
// EnableRateLimitingAttribute'u sectigi icin, ScadaController'daki
// [EnableRateLimiting("policy_scada_ingest")] sessizce EZILIRDI — ingest ucu
// 600/10 sn yerine 50/10 sn ile calisir ve tam da onlenmek istenen bogulma olurdu.
app.MapControllers().Add(endpointBuilder =>
{
    bool hasOwnPolicy = endpointBuilder.Metadata.Any(m => m is EnableRateLimitingAttribute or DisableRateLimitingAttribute);
    if (hasOwnPolicy) return;

    endpointBuilder.Metadata.Add(new EnableRateLimitingAttribute("policy_rate_limiter"));
});

// Hub RATE LIMIT ALTINDA DEGIL: tek bir uzun omurlu baglantidir, istek sayisiyla
// olculmez. Varsayilan politikaya sokmak, yeniden baglanma firtinasinda
// istemcileri kalicı olarak disarida birakirdi.
app.MapHub<DiagramHub>("/hubs/diagram");

app.MapHealthChecks("/health").RequireHost("localhost").AllowAnonymous();

app.Run();
