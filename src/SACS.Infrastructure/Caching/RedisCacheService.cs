using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Redis;
using SACS.Application.Common.Interfaces;

namespace SACS.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
    }

    private IDatabase Database => _connectionMultiplexer.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await Database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(value!);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Redis Caching Warning] Failed to get key '{key}' from Redis. Bypassing cache: {ex.Message}");
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value);
            if (expiration.HasValue)
            {
                await Database.StringSetAsync(key, serialized, expiration.Value);
            }
            else
            {
                await Database.StringSetAsync(key, serialized);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Redis Caching Warning] Failed to set key '{key}' in Redis. Bypassing cache: {ex.Message}");
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await Database.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Redis Caching Warning] Failed to remove key '{key}' from Redis: {ex.Message}");
        }
    }
}
