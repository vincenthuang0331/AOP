using InfraStack.Utility.Dependency;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace InfraStack.Utility.AOP
{
    public interface ICacheRepository
    {
        Task<T?> GetAsync<T>(string Key);

        /// <summary>儲存特定物件的資料</summary>
        /// <param name="Value">物件資料</param>
        /// <param name="Key">唯一KEY</param>
        /// <param name="Duration">持續時間(秒)，-1存永久，-2存到半夜</param>
        Task AddAsync<T>(string Key, T Value, int Duration);

        Task<bool> DeleteAsync(string Key);

        private static readonly Lazy<IServiceProvider> _LazyServiceProvider =
            new(() => IServiceCollectionExtension.ServiceCollection.BuildServiceProvider());
        internal static ICacheRepository Resolve(int Enum)
        {
            var RepoType = IServiceCollectionExtension.RepoTypeCollection[Enum];
            var Repository = DependencyInjector.IsContainerRegistered
                ? DependencyInjector.Resolve(RepoType)
                : _LazyServiceProvider.Value.GetService(RepoType);
            return (ICacheRepository)Repository!;
        }
    }

    internal static class ICacheRepositoryExtension
    {
        private static readonly ConcurrentDictionary<Type, MethodInfo> _GetMap = new();
        public static async Task<object?> GetAsync(this ICacheRepository Repository, string Key, Type ReturnType)
        {
            var Method = _GetMap.GetOrAdd(ReturnType, t => typeof(ICacheRepository).GetMethod(nameof(ICacheRepository.GetAsync))!.MakeGenericMethod(t));
            var InvokeResult = Method.Invoke(Repository, [Key])!;
            await (Task)InvokeResult;
            return InvokeResult.GetType().GetProperty("Result")!.GetValue(InvokeResult);
        }

        private static readonly ConcurrentDictionary<Type, MethodInfo> _AddMap = new();
        public static Task AddAsync(this ICacheRepository Repository, string Key, object? Value, int Duration, Type ReturnType)
        {
            var Method = _AddMap.GetOrAdd(ReturnType, t => typeof(ICacheRepository).GetMethod(nameof(ICacheRepository.AddAsync))!.MakeGenericMethod(t));
            return (Task)Method.Invoke(Repository, [Key, Value, Duration])!;
        }
    }
}
