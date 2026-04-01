using System;

namespace InfraStack.Utility.AOP.Dependency
{
    public static class DependencyInjector
    {
        private static DefaultDependencyInjector? _Instance;
        private static DefaultDependencyInjector Instance =>
            _Instance ?? throw new Exception("請先設定自動注入`DependencyInjector.SetRegistration(...);`");

        public static bool IsContainerRegistered => _Instance != null!;
        public static void SetRegistration(IRegistration Registration)
        {
            _Instance?.Dispose();
            _Instance = new(Registration);
        }

        /// <summary>
        /// find all "readonly" fields which are registered in container, and resolve them.
        /// </summary>
        /// <param name="This">the object to be injected</param>
        /// <exception cref="Exception">throw if SetRegistration method has not been called</exception>
        public static void Inject(object This) => Instance.Inject(This);

        /// <summary>
        /// resolve `T` by constructor injection, and then by field injection("readonly" field)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T? Resolve<T>() => Instance.Resolve<T>();

        /// <summary>
        /// resolve Type by constructor injection, and then by field injection("readonly" field)
        /// </summary>
        /// <param name="Type"></param>
        /// <returns></returns>
        public static object? Resolve(Type Type) => Instance.Resolve(Type);
    }
}
