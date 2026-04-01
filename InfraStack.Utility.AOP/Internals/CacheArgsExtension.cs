using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using InfraStack.Utility.AOP;

namespace InfraStack.Utility.AOP.Internals
{
    internal static class CacheArgsExtension
    {
        private static readonly ConcurrentDictionary<string, List<(string Name, int Index)>> _BracesWithIndexDict = new();

        public static string GetKeyFromMethodInfo(this CacheArgs Args) => string.Join(",", Enumerable.Empty<string>()
                .Append(Args.ClassType?.FullName)
                .Append(Args.Method.Name)
                .Concat(Args.Arguments
                    .Where(o => o != null && o is not CacheEnum)
                    .Select(ConvertArgumentToString)));

        public static string GetKeyByArgumentsInterpolated(this CacheArgs Args, string Key)
        {
            var BracesWithIndex = _BracesWithIndexDict.GetOrAdd(Key, k =>
            {
                var Names = Regex.Matches(k, "{(.+?)}").Select(m => m.Groups[1].Value).ToHashSet();
                return Names.Count == 0 ? new List<(string Name, int Index)>() :
                    Args.Method.GetParameters()
                        .Select((Par, Index) => (Name: Par.Name!, Index))
                        .Where(o => Names.Contains(o.Name))
                        .ToList();
            });

            foreach (var (Name, Index) in BracesWithIndex)
            {
                var ValueString = ConvertArgumentToString(Args.Arguments[Index]);
                Key = Key.Replace($"{{{Name}}}", ValueString);
            }
            return Key;
        }

        private static string? ConvertArgumentToString(object? o) => o switch
        {
            null => null,
            ValueType or string => o.ToString(),
            _ => JsonSerializer.Serialize(o)
        };
    }
}
