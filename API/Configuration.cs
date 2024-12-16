using System.Diagnostics;
using System.Text;
using API.Middlewares;
using API.Utils;
using CloudinaryDotNet;
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
        var redisConfiguration = configuration["Redis:Configuration"];
        ArgumentException.ThrowIfNullOrWhiteSpace(redisConfiguration);
        services.AddStackExchangeRedisCache(options => { options.Configuration = redisConfiguration; });
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConfiguration));

        // Cloudinary
        var cloud = configuration["Cloudinary:Cloud"];
        ArgumentException.ThrowIfNullOrWhiteSpace(cloud);
        var apiKey = configuration["Cloudinary:ApiKey"];
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        var apiSecret = configuration["Cloudinary:ApiSecret"];
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        var cloudinary = new Cloudinary(new Account { Cloud = cloud, ApiKey = apiKey, ApiSecret = apiSecret });
        services.AddSingleton<ICloudinary>(cloudinary);

        // JWT
        var secret = configuration["JWT:Secret"];
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        var issuer = configuration["JWT:ValidIssuer"];
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
        var audience = configuration["JWT:ValidAudience"];
        ArgumentException.ThrowIfNullOrWhiteSpace(audience);
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
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrWhiteSpace(accessToken) && path.StartsWithSegments("/hub"))
                        context.Token = accessToken;

                    return Task.CompletedTask;
                }
            };
        });

        // CORS
        var clientUrl = configuration["URL:Client"];
        ArgumentException.ThrowIfNullOrWhiteSpace(clientUrl);
        services.AddCors(options =>
        {
            options.AddPolicy("cors",
                corsPolicyBuilder =>
                {
                    corsPolicyBuilder
                        .WithOrigins(clientUrl)
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
        services.AddScoped<ICloudinaryHelper, CloudinaryHelper>();
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
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IConversationRepository, ConversationRepository>();

        // Message
        services.AddScoped<IMessageService, MessageService>();
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