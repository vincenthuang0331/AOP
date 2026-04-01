using InfraStack.Utility.AOP.Internals;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace InfraStack.Utility.AOP.Dependency
{
    public class DefaultDependencyInjector : IDependencyInjector, IDisposable
    {
        private readonly IRegistration _Registration;
        private readonly HashSet<int> _Hashes = new();
        private SemaphoreSlim? _HashLocker = new(1, 1);
        private System.Timers.Timer? _HashResetTimer;

        public DefaultDependencyInjector(IRegistration Registration)
        {
            _Registration = Registration;
            _HashResetTimer = new() { Interval = TimeSpan.FromHours(1).TotalMilliseconds };
            _HashResetTimer.Elapsed += delegate { ResetHashes(); };
            _HashResetTimer.Start();
        }

        public void ResetHashes() => _HashLocker?.Lock(_Hashes.Clear);

        private readonly ConcurrentDictionary<Type, FieldInfo[]> _CachedFieldsMap = new();

        public void Inject(object This)
        {
            foreach (var Fd in _CachedFieldsMap.GetOrAdd(This.GetType(), t => EnumerateRegisteredFields(t).ToArray()))
            {
                if (Fd.FieldType.IsValueType || Fd.GetValue(This) != null) continue;
                Fd.SetValue(This, Resolve(Fd.FieldType));
            }
        }

        public object? Resolve(Type Type)
        {
            if (!_Registration.IsRegistered(Type)) return null;

            var Obj = _Registration.Resolve(Type)!;
            var HashCode = Obj.GetHashCode();
            if (_Hashes.Contains(HashCode)) return Obj;

            Inject(Obj);
            _HashLocker?.Lock(() =>
            {
                if (!_Hashes.Contains(HashCode))
                {
                    _Hashes.Add(HashCode);
                }
            });
            return Obj;
        }

        private IEnumerable<FieldInfo> EnumerateRegisteredFields(Type Type)
        {
            const BindingFlags TargetFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var fd in Type.GetFields(TargetFlags))
            {
                if (!fd.IsInitOnly || !_Registration.IsRegistered(fd.FieldType)) continue;
                yield return fd;
            }

            foreach (var p in Type.GetProperties(TargetFlags))
            {
                if (!p.CanRead || p.CanWrite || !_Registration.IsRegistered(p.PropertyType)) continue;
                var NullableBackingField = Type.BaseType?.GetBackingField(p.Name);
                if (NullableBackingField != null) yield return NullableBackingField;
            }
        }

        public void Dispose()
        {
            if (_HashResetTimer != null!)
            {
                _HashResetTimer.Stop();
                _HashResetTimer.Dispose();
                _HashResetTimer = null;
            }

            if (_HashLocker != null!)
            {
                _HashLocker.Dispose();
                _HashLocker = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}
