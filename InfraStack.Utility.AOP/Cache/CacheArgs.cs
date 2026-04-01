using System;
using System.Threading.Tasks;

namespace InfraStack.Utility.AOP
{
    internal class CacheArgs : MethodArgs
    {
        public CacheEnum? CacheEnum { get; set; }
        public string Key { get; set; } = null!;
        public ICacheRepository Repository { get; set; } = null!;
        public Type ReturnType { get; set; } = null!;

        public Task RepoAddAsync(object? Value, int Duration) => Repository.AddAsync(Key, Value, Duration, ReturnType);
        public Task<object?> RepoGetAsync() => Repository.GetAsync(Key, ReturnType);
        public Task<bool> RepoDeleteAsync() => Repository.DeleteAsync(Key);
    }
}
