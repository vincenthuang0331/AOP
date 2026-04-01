using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace InfraStack.Utility.AOP
{
    public static class IServiceCollectionExtension
    {
        /// <summary>
        /// 註冊CacheRepository
        /// </summary>
        /// <param name="Repositories"></param>
        /// <exception cref="System.ArgumentException">CacheRepositoryEnum值重複</exception>
        public static IServiceCollection ConfigureCacheRepository<RepositoryT>(this IServiceCollection Collection, int CacheRepositoryEnum)
            where RepositoryT : class, ICacheRepository
        {
            if (ServiceCollection == null)
                ServiceCollection = Collection;

            RepoTypeCollection.AddOrUpdate(CacheRepositoryEnum, typeof(RepositoryT), (x, o) => typeof(RepositoryT));
            RepoDurationCollection.TryAdd(CacheRepositoryEnum, 30);
            return Collection.AddSingleton<RepositoryT>();
        }

        /// <summary>
        /// 註冊CacheRepository
        /// </summary>
        /// <typeparam name="RepositoryT"></typeparam>
        /// <param name="Collection"></param>
        /// <param name="CacheRepositoryEnum"></param>
        /// <param name="CacheDuration">預設全域快取時間(-1:永久、-2:存到半夜12點)</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">CacheRepositoryEnum值重複</exception>
        public static IServiceCollection ConfigureCacheRepository<RepositoryT>(this IServiceCollection Collection, int CacheRepositoryEnum, int CacheDuration)
            where RepositoryT : class, ICacheRepository
        {
            Collection.ConfigureCacheRepository<RepositoryT>(CacheRepositoryEnum);
            RepoDurationCollection.AddOrUpdate(CacheRepositoryEnum, CacheDuration, (x, o) => CacheDuration);
            return Collection;
        }

        public static IServiceCollection ConfigureDefaultCache(this IServiceCollection Collection, int CacheRepositoryEnum)
        {
            if (!RepoTypeCollection.ContainsKey(CacheRepositoryEnum))
                throw new Exception($"There is no CacheRepositoryEnum:{CacheRepositoryEnum} in CacheAttribtue. Please call 'InfraStack.Utility.Standard.AOP.IServiceCollectionExtension.ConfigureCacheRepository' method to add.");

            _DefaultCacheRepositoryEnum = CacheRepositoryEnum;
            return Collection;
        }
        internal static IServiceCollection ServiceCollection { get; set; }
        internal static ConcurrentDictionary<int, Type> RepoTypeCollection { get; } = new ConcurrentDictionary<int, Type>();
        private static int? _DefaultCacheRepositoryEnum;
        internal static int DefaultCacheRepositoryEnum
            => _DefaultCacheRepositoryEnum.HasValue ? _DefaultCacheRepositoryEnum.Value : RepoTypeCollection.Keys.Min(o => o);

        internal static ConcurrentDictionary<int, int> RepoDurationCollection { get; } = new ConcurrentDictionary<int, int>();
    }
}
