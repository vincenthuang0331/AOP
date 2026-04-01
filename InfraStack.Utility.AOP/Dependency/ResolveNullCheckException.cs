using System;

namespace InfraStack.Utility.AOP.Dependency
{
    public class ResolveNullCheckException : Exception
    {
        public ResolveNullCheckException(string Message) : base(Message) { }

        public static ResolveNullCheckException CreateForResolveInternal(Type Type) =>
            new($"DependencyInjector.Resolve<T>()生成物件失敗({Type.FullName})");
    }
}
