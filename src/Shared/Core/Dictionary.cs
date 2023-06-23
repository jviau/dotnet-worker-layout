// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.ObjectModel;

namespace Microsoft.Azure.Functions;

/// <summary>
/// Dictionary helpers.
/// </summary>
internal static class Dictionary
{
    /// <summary>
    /// Merges two dictionaries together into a new dictionary. Conflicts are resolved by taking the key/value pairs
    /// from the <paramref name="right" /> dictionary.
    /// </summary>
    /// <typeparam name="TDictionary">The dictionary type to merge.</typeparam>
    /// <typeparam name="TKey">The key type held by the dictionaries.</typeparam>
    /// <typeparam name="TValue">The value type held by the dictionaries.</typeparam>
    /// <param name="left">The left dictionary to merge.</param>
    /// <param name="right">The right enumerable to merge with.</param>
    /// <returns>
    /// A new dictionary of type <typeparamref name="TDictionary" /> with key/values of both <paramref name="left" />
    /// and <paramref name="right" />.
    /// </returns>
    public static TDictionary MergeLeft<TDictionary, TKey, TValue>(
        this TDictionary left, IEnumerable<KeyValuePair<TKey, TValue>> right)
        where TDictionary : class, IDictionary<TKey, TValue>, new()
        where TKey : notnull
    {
        Check.NotNull(left);
        Check.NotNull(right);

        TDictionary result = new();
        AddAll(result, left);
        AddAll(result, right);
        return result;
    }

    /// <summary>
    /// Merges two dictionaries together into a new dictionary. Conflicts are resolved by taking the key/value pairs
    /// from the <paramref name="right" /> dictionary.
    /// </summary>
    /// <typeparam name="TKey">The key type held by the dictionaries.</typeparam>
    /// <typeparam name="TValue">The value type held by the dictionaries.</typeparam>
    /// <param name="left">The left dictionary to merge.</param>
    /// <param name="right">The right enumerable to merge with.</param>
    /// <returns>
    /// A new dictionary with key/values of both <paramref name="left" />
    /// and <paramref name="right" />.
    /// </returns>
    public static IDictionary<TKey, TValue> MergeLeft<TKey, TValue>(
        this IDictionary<TKey, TValue> left, IEnumerable<KeyValuePair<TKey, TValue>> right)
        where TKey : notnull
    {
        Check.NotNull(left);
        Check.NotNull(right);

        Dictionary<TKey, TValue> result = new();
        AddAll(result, left);
        AddAll(result, right);
        return result;
    }

    /// <summary>
    /// Appends all values from <paramref name="right" /> into <paramref name="left" />. Conflicts are replaced.
    /// </summary>
    /// <typeparam name="TKey">The key type held by the dictionaries.</typeparam>
    /// <typeparam name="TValue">The value type held by the dictionaries.</typeparam>
    /// <param name="left">The dictionary to append into.</param>
    /// <param name="right">The enumerable to append from.</param>
    public static void AddAll<TKey, TValue>(
        this IDictionary<TKey, TValue> left, IEnumerable<KeyValuePair<TKey, TValue>> right)
    {
        Check.NotNull(left);
        Check.NotNull(right);
        foreach ((TKey key, TValue value) in right)
        {
            left[key] = value;
        }
    }

    /// <summary>
    /// Read-only dictionary helpers.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    public static class ReadOnly<TKey, TValue>
            where TKey : notnull
    {
        /// <summary>
        /// Gets the empty readonly dictionary.
        /// </summary>
        public static readonly IReadOnlyDictionary<TKey, TValue> Empty
            = new ReadOnlyDictionary<TKey, TValue>(new Dictionary<TKey, TValue>());
    }
}
