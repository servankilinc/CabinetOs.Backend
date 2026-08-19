using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using FluentValidation;
using CabinetOs.Core;
using CabinetOs.Core.Utils.Auth;
using CabinetOs.Model.Entities;
using CabinetOs.DataAccess;
using CabinetOs.DataAccess.Contexts;
using CabinetOs.Business;
using CabinetOs.WebAPI.ExceptionHandler;
using CabinetOs.WebAPI.Utils;

var builder = WebApplication.CreateBuilder(args);


#region ------- CORS -------
builder.Services.AddCors(options =>
{
    options.AddPolicy("policy_cors", builder =>
    {
        builder
            .AllowAnyOrigin()
            //.WithOrigins("https://www.frontend.com")
            //.AllowCredentials() // AllowAnyOrigin and AllowCredentials cannot using together so open when you use WithOrigins
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
    options.AddSlidingWindowLimiter(policyName: "policy_rate_limiter", slidingOptions =>
    {
        slidingOptions.PermitLimit = 15;
        slidingOptions.Window = TimeSpan.FromSeconds(5);
        slidingOptions.SegmentsPerWindow = 4;
        slidingOptions.QueueLimit = 5;
        slidingOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
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

//if (app.Environment.IsDevelopment())
//{
app.MapOpenApi();
app.MapScalarApiReference();
//}

app.UseHttpsRedirection();

app.UseCors("policy_cors");

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers().RequireRateLimiting("policy_rate_limiter");

app.MapHealthChecks("/health").RequireHost("localhost").AllowAnonymous();

app.Run();
