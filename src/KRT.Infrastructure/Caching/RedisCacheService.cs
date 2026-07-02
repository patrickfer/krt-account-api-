using KRT.Application.Common.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace KRT.Infrastructure.Caching;

public sealed class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;

    public RedisCacheService(IConnectionMultiplexer redis) => _db = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var value = await _db.StringGetAsync(key);
        return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<T>(value.ToString());
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
        => await _db.StringSetAsync(key, JsonSerializer.Serialize(value), expiration);

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => await _db.KeyDeleteAsync(key);
}
