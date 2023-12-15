using System;
using System.Collections.Generic;

namespace Xieyi.ORM.Core.Extensions;

internal static class GenericExtensions
{
    public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key))
            dictionary[key] = value;
        else
            dictionary.Add(key, value);
    }

    public static void Foreach<T>(this IEnumerable<T> dataSource, Action<T> func)
    {
        foreach (var dataItem in dataSource) func(dataItem);
    }
}