using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace InfraStack.Utility.AOP.Internals
{
    internal static class ReflectionExtension
    {
        public static FieldInfo? GetBackingField(this Type TheType, string PropertyName)
        {
            var FieldName = $"<{PropertyName}>k__BackingField";
            const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

            for (var Current = TheType; Current != null; Current = Current.BaseType)
            {
                var Field = Current.GetField(FieldName, FieldFlags);
                if (Field != null) return Field;
            }

            return null;
        }

        private static readonly ConcurrentDictionary<Type, object> _ValueTypeDefaultCache = new();
        private static object ValueTypeDefault(Type ValueType) =>
            _ValueTypeDefaultCache.GetOrAdd(ValueType, t => Activator.CreateInstance(t)!);

        public static object? Default(this Type TheType) => TheType.IsValueType switch
        {
            true => ValueTypeDefault(TheType),
            false => null
        };

        public static bool IsDefault(this Type TheType, object? Value) => TheType.IsValueType switch
        {
            true => ValueTypeDefault(TheType).Equals(Value),
            false => Value == null
        };
    }
}
