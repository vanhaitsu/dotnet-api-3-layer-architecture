using System.Diagnostics;
using System.Text;
using API.Middlewares;
using API.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Repositories.Common;
using Repositories.Interfaces;
using Repositories.Repositories;
using Services.Common;
using Services.Helpers;
using Services.Interfaces;
using Services.Services;
using StackExchange.Redis;

namespace API;

public static class Configuration
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services,
        ConfigurationManager configuration)
    {
        #region Configuartion

        // Local database
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("LocalDb"));
        });

        // Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:Configuration"];
        });
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!));

        // JWT
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidAudience = configuration["JWT:ValidAudience"],
                ValidIssuer = configuration["URL:Server"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"]!))
            };
        });

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("cors",
                corsPolicyBuilder =>
                {
                    corsPolicyBuilder
                        .WithOrigins(configuration["URL:Client"]!)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        #endregion

        #region Middleware

        services.AddScoped<AccountStatusMiddleware>();
        services.AddSingleton<GlobalExceptionMiddleware>();
        services.AddSingleton<PerformanceMiddleware>();
        services.AddSingleton<Stopwatch>();

        #endregion

        #region Common

        services.AddHttpContextAccessor();
        services.AddAutoMapper(typeof(MapperProfile).Assembly);
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<IRedisHelper, RedisHelper>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddTransient<IEmailService, EmailService>();

        #endregion

        #region Dependency Injection

        // Account
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAccountRepository, AccountRepository>();

        // AccountConversation
        services.AddScoped<IAccountConversationRepository, AccountConversationRepository>();

        // AccountRole
        services.AddScoped<IAccountRoleRepository, AccountRoleRepository>();

        // Conversation
        services.AddScoped<IConversationRepository, ConversationRepository>();

        // Message
        services.AddScoped<IMessageRepository, MessageRepository>();

        // MessageRecipient
        services.AddScoped<IMessageRecipientRepository, MessageRecipientRepository>();

        // RefreshToken
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        // Role
        services.AddScoped<IRoleRepository, RoleRepository>();

        #endregion

        return services;
    }
}