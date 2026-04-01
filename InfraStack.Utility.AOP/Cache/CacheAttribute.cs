using InfraStack.Utility.AOP.Internals;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static InfraStack.Utility.AOP.IServiceCollectionExtension;

namespace InfraStack.Utility.AOP
{
    public sealed class CacheAttribute : MethodBoundaryAttribute
    {
        private int _Duration;

        /// <summary>
        /// 持續多久(秒)、-1:永久、-2:存到半夜12點<br/>
        /// 預設快取的全域生命週期請參考<see cref="IServiceCollectionExtension.ConfigureCacheRepository{RepositoryT}(IServiceCollection, int, int)"/>
        /// </summary>
        public int Duration
        {
            get => _Duration == 0 ? RepoDurationCollection[CacheRepositoryEnum] : _Duration;
            set => _Duration = value;
        }

        /// <summary>手動輸入產生靜態Key，亦可以輸入{傳入的參數名稱}，ex:Prefix{Param1}，動態產生Key</summary>
        public string? Key { get; set; }

        private bool _QueryParamAsKey;

        /// <summary>自動產生Key值</summary>
        public bool QueryParamAsKey
        {
            get => _QueryParamAsKey || string.IsNullOrWhiteSpace(Key);
            set => _QueryParamAsKey = value;
        }

        /// <summary>當沒有快取時，是否鎖定多執行序(預設為是)</summary>
        public bool IsLock { get; set; } = true;

        private int _CacheRepositoryEnum;

        /// <summary>指定快取容器</summary>
        public int CacheRepositoryEnum
        {
            get => _CacheRepositoryEnum == 0 ? DefaultCacheRepositoryEnum : _CacheRepositoryEnum;
            set => _CacheRepositoryEnum = value;
        }

        public override MethodArgs ConfigureArgs() => new CacheArgs();

        public override async Task OnEntryAsync(MethodArgs Args)
        {
            if (RepoTypeCollection.IsEmpty)
                throw new Exception(
                    "There is no CacheRepository in CacheAttribtue. Please call 'InfraStack.Utility.Standard.AOP.IServiceCollectionExtension.ConfigureCacheRepository' method to add.");

            var CacheArgs = (CacheArgs)Args;
            CacheArgs.CacheEnum = (CacheEnum?)Args.Arguments.FirstOrDefault(o => o is CacheEnum);
            CacheArgs.Key = QueryParamAsKey
                ? CacheArgs.GetKeyFromMethodInfo()
                : CacheArgs.GetKeyByArgumentsInterpolated(Key!);
            CacheArgs.Repository = ICacheRepository.Resolve(CacheRepositoryEnum);
            CacheArgs.ReturnType = TaskStatic.GetResultType(Args.Method.ReturnType);

            switch (CacheArgs.CacheEnum)
            {
                case CacheEnum.ForceUpdate:
                    {
                        if (IsLock) await LockAsync(Args);
                        break;
                    }
                case CacheEnum.Expire:
                    {
                        await CacheArgs.RepoDeleteAsync();
                        Args.Return(CacheArgs.ReturnType.Default());
                        break;
                    }
                case CacheEnum.AlwaysFromCache:
                    {
                        Args.Return(await CacheArgs.RepoGetAsync());
                        break;
                    }
                default:
                    {
                        var HasData = await GetFromRepoAsync();
                        if (!HasData && IsLock)
                        {
                            await LockAsync(Args);
                            await GetFromRepoAsync();
                        }

                        break;

                        async Task<bool> GetFromRepoAsync()
                        {
                            var Result = await CacheArgs.RepoGetAsync();
                            if (CacheArgs.ReturnType.IsDefault(Result)) return false;
                            Args.Return(Result);
                            return true;
                        }
                    }
            }
        }

        public override async Task OnSuccessAsync(MethodArgs Args)
        {
            var CacheArgs = (CacheArgs)Args;

            // ForceUpdate的值若為空陣列，則不會更新快取
            if (CacheArgs.CacheEnum == CacheEnum.ForceUpdate &&
                Args.ReturnValue is ICollection { Count: 0 } &&
                await CacheArgs.RepoGetAsync() is ICollection { Count: > 0 })
                return;

            await CacheArgs.RepoAddAsync(Args.ReturnValue, Duration);
        }

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _LockerDictionary = new();
        private async Task LockAsync(MethodArgs Args)
        {
            var CacheArgs = (CacheArgs)Args;
            var Locker = _LockerDictionary.GetOrAdd(CacheArgs.Key + CacheRepositoryEnum, _ => new SemaphoreSlim(1, 1));
            await Locker.WaitAsync();
            Args.OnFinally(() =>
            {
                Locker.Release();
                return Task.CompletedTask;
            });
        }

        private static readonly FieldInfo _WaitCountField =
            typeof(SemaphoreSlim).GetField("m_waitCount", BindingFlags.Instance | BindingFlags.NonPublic)!;

        /// <summary>取得被lock而在等待的Thread數</summary>
        public static IEnumerable<(string CacheKey, int WaitCount)> GetWaitCount() =>
            _LockerDictionary.Select(o => (o.Key, (int)_WaitCountField.GetValue(o.Value)!));
    }
}
