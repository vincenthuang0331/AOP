using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace InfraStack.Utility.AOP
{
    internal interface IOnInvoke
    {
        void Run(OnInvokeModel Model);

        private static readonly IOnInvoke _Sync = new OnInvokeSync();
        private static readonly IOnInvoke _Task = new OnInvokeTask();
        private static readonly ConcurrentDictionary<Type, IOnInvoke> _TaskResult = new();

        public static IOnInvoke Create(Type ReturnType) =>
            ReturnType switch
            {
                var t when t == typeof(Task) => _Task,
                var t when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Task<>) =>
                    GetTaskResult(t),
                _ => _Sync
            };

        private static IOnInvoke GetTaskResult(Type ReturnType) =>
            _TaskResult.GetOrAdd(ReturnType, t => (IOnInvoke)Activator.CreateInstance(
                typeof(OnInvokeTaskResult<>).MakeGenericType(t.GetGenericArguments()[0]))!);
    }
}
