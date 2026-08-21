using CabinetOs.Business;
using CabinetOs.Core;
using CabinetOs.Core.Utils.Auth;
using CabinetOs.DataAccess;
using CabinetOs.DataAccess.Contexts;
using CabinetOs.Model.Entities;
using CabinetOs.WebAPI.ExceptionHandler;
using CabinetOs.WebAPI.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
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
                });
            #endregion
        

#region ------- AutoMapper -------
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
#endregion


#region ------- FluentValidation -------
builder.Services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
#endregion


builder.Services.AddExceptionHandler<ExceptionHandleMiddleware>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks();

builder.Services.AddControllers();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<ScalarSecuritySchemeTransformer>();
});

var app = builder.Build();


app.UseExceptionHandler();

//app.UseStaticFiles();

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

app.MapControllers().RequireRateLimiting("policy_rate_limiter");

app.MapHealthChecks("/health").RequireHost("localhost").AllowAnonymous();

app.Run();
