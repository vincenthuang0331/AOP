using InfraStack.Utility.AOP;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace InfraStack.Utility.AOP.Test
{
    public class CacheAttributeTest
    {
        static CacheAttributeTest()
        {
            new ServiceCollection()
                 .ConfigureCacheRepository<Memory0Repository>(0, 3)
                 .ConfigureCacheRepository<Memory1Repository>(1, 3);

        }
        private readonly string _TestValue = "Test";
        internal static TimeSpan _ExecutionTime = TimeSpan.FromSeconds(3);

        [Fact]
        public async void Execution_Time_Of_Method_Should_Be_Less_Than_Five_Seconds_If_CacheEnum_Is_Normal()
        {
            var Wath = new Stopwatch();
            Wath.Start();
            var ActualValueFromMethodBody = GetNormal(CacheEnum.Normal);
            Assert.True(Wath.Elapsed >= _ExecutionTime, $"實際耗時:{Wath.Elapsed.Milliseconds}毫秒");
            Assert.Equal(_TestValue, ActualValueFromMethodBody);

            Wath.Restart();
            var ActualValueFromCache = GetNormal(CacheEnum.Normal);
            Assert.True(Wath.Elapsed < TimeSpan.FromSeconds(0.1));
            Assert.Equal(_TestValue, ActualValueFromCache);

            await Task.Delay(_ExecutionTime);

            Wath.Restart();
            ActualValueFromMethodBody = GetNormal();
            Assert.True(Wath.Elapsed >= _ExecutionTime);
            Assert.Equal(_TestValue, ActualValueFromMethodBody);

            Wath.Restart();
            var Obj = new GenericClass<string>();
            var Result = await Obj.GetAsync();
            Assert.True(Wath.Elapsed >= _ExecutionTime);
            var Result2 = await Obj.GetAsync();
            Assert.Equal(Result, Result2);

            Wath.Restart();
            Result = await GetGenericAsync<string>();
            Assert.True(Wath.Elapsed >= _ExecutionTime);
            Wath.Restart();
            Result2 = await GetGenericAsync<string>();
            Assert.True(Wath.Elapsed < TimeSpan.FromSeconds(0.1));
            Assert.Equal(Result, Result2);

            Wath.Restart();
            var TupleResult = await GetTupleAsync();
            Assert.True(Wath.Elapsed >= _ExecutionTime);
            Wath.Restart();
            TupleResult = await GetTupleAsync();
            Assert.True(Wath.Elapsed < TimeSpan.FromSeconds(0.1));
            Assert.Equal(TupleResult.Item1, _TestValue);
            Assert.Equal(TupleResult.Item2, _TestValue);
        }

        [Fact]
        public void Execution_Time_Of_Method_Should_Be_More_Than_Five_Seconds_If_CacheEnum_Is_Expired()
        {
            GetExpire(CacheEnum.Normal);
            // 呼叫 CacheEnum.Expire 會刪除cache值
            GetExpire(CacheEnum.Expire);

            // 再次呼叫 CacheEnum.Normal 會執行方法!
            var Wath = new Stopwatch();
            Wath.Start();
            var ActualValueFromMethodBody = GetExpire(CacheEnum.Normal);
            Assert.True(Wath.Elapsed >= _ExecutionTime);
            Assert.Equal(_TestValue, ActualValueFromMethodBody);
        }

        [Fact]
        public void Execution_Time_Of_Method_Should_Be_Greater_Than_Or_Equal_To_Five_Seconds_If_CacheEnum_Is_ForceUpdate()
        {
            var Wath = new Stopwatch();
            Wath.Start();
            var ActualValueFromMethodBody = GetForceUpdate(CacheEnum.ForceUpdate);
            Assert.True(Wath.Elapsed >= _ExecutionTime);
            Assert.Equal(_TestValue, ActualValueFromMethodBody);

            Wath.Restart();
            ActualValueFromMethodBody = GetForceUpdate(CacheEnum.ForceUpdate);
            Assert.True(Wath.Elapsed >= _ExecutionTime);
            Assert.Equal(_TestValue, ActualValueFromMethodBody);
        }

        [Fact]
        public void Execution_Time_Of_Method_Should_Be_Less_Than_Five_Seconds_And_Value_Should_Be_Null_If_CacheEnum_Is_AlwaysFromCache()
        {
            var Wath = new Stopwatch();
            Wath.Start();
            var FirstActualValue = GetAlwaysFromCache(CacheEnum.AlwaysFromCache);
            Assert.True(Wath.Elapsed < TimeSpan.FromSeconds(3));
            Assert.Null(FirstActualValue);
        }

        [Fact]
        public async void 測試花括號()
        {
            var Wath = new Stopwatch();
            Wath.Start();

            var Actual = await Get(_TestValue, "1");
            Assert.True(Wath.Elapsed >= _ExecutionTime);
            Assert.Equal(_TestValue, Actual);
            Wath.Restart();
            Actual = await Get(_TestValue, "1");
            Assert.True(Wath.Elapsed < _ExecutionTime);
            Assert.Equal(_TestValue, Actual);

            Wath.Restart();
            Actual = await Get("1", "2");
            Assert.True(Wath.Elapsed >= _ExecutionTime);
            Assert.Equal("1", Actual);
        }

        [Fact]
        public async Task 測試Lock()
        {
            var Tasks = Enumerable.Range(0, 5).Select(o => TestLockAsync());
            var Result = await Task.WhenAll(Tasks);
            Assert.True(Result.All(o => o == 1));
        }

        [Fact]
        public async Task 測試沒有Lock()
        {
            var Tasks = Enumerable.Range(0, 5).Select(o => TestNoLockAsync());
            var Result = await Task.WhenAll(Tasks);
            Assert.Equal(5, Result.Max());
        }

        [Cache(Key = "CurlyBraces{Param1}{Param2}")]
        private Task<string> Get(string Param1, string Param2)
        {
            Thread.Sleep(_ExecutionTime);
            return Task.FromResult(Param1);
        }

        [Cache(QueryParamAsKey = true)]
        private string GetNormal(CacheEnum CacheEnum = CacheEnum.Normal)
        {
            Thread.Sleep(_ExecutionTime);
            return _TestValue;
        }

        [Cache]
        private string GetExpire(CacheEnum CacheEnum = CacheEnum.Normal)
        {
            Thread.Sleep(_ExecutionTime);
            return _TestValue;
        }

        [Cache(QueryParamAsKey = true)]
        private string GetForceUpdate(CacheEnum CacheEnum = CacheEnum.ForceUpdate)
        {
            Thread.Sleep(_ExecutionTime);
            return _TestValue;
        }

        [Cache(QueryParamAsKey = true)]
        private string GetAlwaysFromCache(CacheEnum CacheEnum = CacheEnum.AlwaysFromCache)
        {
            Thread.Sleep(5000);
            return _TestValue;
        }

        [Cache]
        public async Task<List<ReturnT>> GetGenericAsync<ReturnT>()
        {
            await Task.Delay(_ExecutionTime);
            return new();
        }

        [Cache]
        public async Task<(string, string)> GetTupleAsync()
        {
            await Task.Delay(_ExecutionTime);
            return (_TestValue, _TestValue);
        }

        private static int _LockCounter = 0;

        [Cache]
        public async Task<int> TestLockAsync()
        {
            await Task.Delay(_ExecutionTime);
            _LockCounter++;
            return _LockCounter;
        }

        private static int _NoLockCounter = 0;

        [Cache(IsLock = false)]
        public async Task<int> TestNoLockAsync()
        {
            await Task.Delay(_ExecutionTime);
            _NoLockCounter++;
            return _NoLockCounter;
        }

        [Fact]
        public void 兩個Repository快取互相隔離()
        {
            var Wath = new Stopwatch();

            // Repo0 寫入快取
            Wath.Start();
            var R0First = GetFromRepo0("IsolationKey");
            Assert.True(Wath.Elapsed >= _ExecutionTime, "Repo0 第一次應執行方法");

            // Repo0 命中快取
            Wath.Restart();
            var R0Cached = GetFromRepo0("IsolationKey");
            Assert.True(Wath.Elapsed < TimeSpan.FromSeconds(0.1), "Repo0 第二次應從快取取得");
            Assert.Equal(R0First, R0Cached);

            // 相同參數打 Repo1，應執行方法（兩個 MemoryCache 各自獨立）
            Wath.Restart();
            var R1First = GetFromRepo1("IsolationKey");
            Assert.True(Wath.Elapsed >= _ExecutionTime, "Repo1 第一次應執行方法，不應命中 Repo0 的快取");

            // Expire Repo1，不影響 Repo0 的快取
            GetFromRepo1("IsolationKey", CacheEnum.Expire);

            Wath.Restart();
            var R0AfterRepo1Expire = GetFromRepo0("IsolationKey");
            Assert.True(Wath.Elapsed < TimeSpan.FromSeconds(0.1), "Repo1 Expire 後，Repo0 快取應仍有效");
            Assert.Equal(R0First, R0AfterRepo1Expire);
        }

        [Cache(QueryParamAsKey = true, CacheRepositoryEnum = 0, Duration = -1)]
        private string GetFromRepo0(string Id, CacheEnum CacheEnum = CacheEnum.Normal)
            => DoSlowWork(Id);

        [Cache(QueryParamAsKey = true, CacheRepositoryEnum = 1, Duration = -1)]
        private string GetFromRepo1(string Id, CacheEnum CacheEnum = CacheEnum.Normal)
            => DoSlowWork(Id);

        private string DoSlowWork(string Id)
        {
            Thread.Sleep(_ExecutionTime);
            return Id;
        }
    }

    class GenericClass<ReturnT>
    {
        [Cache]
        public async Task<List<ReturnT>> GetAsync()
        {
            await Task.Delay(CacheAttributeTest._ExecutionTime);
            return new();
        }
    }
}
