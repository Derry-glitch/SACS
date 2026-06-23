using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SACS.Application.Common.Interfaces;
using SACS.Infrastructure.Caching;
using SACS.Infrastructure.Identity;
using SACS.Infrastructure.Messaging;
using SACS.Infrastructure.Notifications;
using SACS.Infrastructure.Storage;
using SACS.Infrastructure.BackgroundJobs;
using SACS.Infrastructure.Services;
using StackExchange.Redis;

namespace SACS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind JWT Configuration settings
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        // Register StackExchange.Redis ConnectionMultiplexer
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));
        services.AddScoped<ICacheService, RedisCacheService>();

        // Register Azure Blob Storage ServiceClient
        var blobConnectionString = configuration.GetConnectionString("AzureBlobStorage") ?? "UseDevelopmentStorage=true";
        services.AddSingleton(sp => new BlobServiceClient(blobConnectionString));
        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

        // Register Azure Service Bus Client
        var serviceBusConnectionString = configuration.GetConnectionString("AzureServiceBus") ?? "Endpoint=sb://localhost/;SharedAccessKeyName=Placeholder;SharedAccessKey=Placeholder";
        services.AddSingleton(sp => new ServiceBusClient(serviceBusConnectionString));
        services.AddScoped<IEventBus, AzureServiceBusEventBus>();

        // Register Notification Providers (Provider Pattern)
        services.AddScoped<INotificationProvider, FirebaseNotificationProvider>();
        services.AddScoped<INotificationProvider, EmailNotificationProvider>();
        services.AddScoped<INotificationProvider, SmsNotificationProvider>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        // Register JWT Services
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Register Background Job Service
        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

        // Register Python AI Client
        services.AddHttpClient<IAiServiceClient, PythonAiServiceClient>(client =>
        {
            var baseUrl = configuration["AIService:BaseUrl"] ?? "http://localhost:8000";
            client.BaseAddress = new Uri(baseUrl);
        });

        return services;
    }
}
