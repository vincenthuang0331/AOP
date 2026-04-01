using System;
using System.Threading.Tasks;

namespace InfraStack.Utility.AOP.Internals
{
    internal static class TaskStatic
    {
        public static Type GetResultType(Type MayBeTaskType) =>
            MayBeTaskType.IsGenericType && MayBeTaskType.GetGenericTypeDefinition() == typeof(Task<>)
                ? MayBeTaskType.GetGenericArguments()[0] : MayBeTaskType;
    }
}
