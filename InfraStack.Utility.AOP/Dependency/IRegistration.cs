using System;

namespace InfraStack.Utility.AOP.Dependency
{
    /// <summary>
    /// For mapping type to instance
    /// </summary>
    public interface IRegistration
    {
        bool IsRegistered(Type type);

        object? Resolve(Type type);
    }
}
