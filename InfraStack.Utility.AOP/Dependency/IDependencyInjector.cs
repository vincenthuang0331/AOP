using System;

namespace InfraStack.Utility.AOP.Dependency
{
    public interface IDependencyInjector
    {
        /// <summary>find all "readonly" fields which are registered in container, and resolve them.</summary>
        /// <param name="This">the object to be injected</param>
        void Inject(object This);

        /// <summary>resolve Type by constructor injection, and then by field injection("readonly" field)</summary>
        object? Resolve(Type Type);
    }

    public static class IDependencyInjectorExtension
    {
        public static T? Resolve<T>(this IDependencyInjector Di) => (T?)Di.Resolve(typeof(T));
    }
}
