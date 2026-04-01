using InfraStack.Utility.AOP;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace InfraStack.Utility.AOP.Test
{
    internal class Memory0Repository : ICacheRepository
    {
        internal readonly MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

        public Task AddAsync<T>(string Key, T Value, int Duration)
        {
            var options = Duration switch
            {
                -1 => new MemoryCacheEntryOptions(),
                -2 => new MemoryCacheEntryOptions { AbsoluteExpiration = DateTime.Today.AddDays(1) },
                _ => new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(Duration) }
            };
            Cache.Set(Key, Value, options);
            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(string Key)
        {
            Cache.Remove(Key);
            return Task.FromResult(true);
        }

        public Task<T?> GetAsync<T>(string Key)
        {
            Cache.TryGetValue(Key, out T? value);
            return Task.FromResult(value);
        }
    }
    internal class Memory1Repository : Memory0Repository
    {

    }

}
