// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

#nullable enable
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Azure.Functions;

/// <summary>
/// Helpers for <see cref="InvalidOperationException" /> assertions.
/// </summary>
internal class Verify
{
    /// <summary>
    /// Verify some argument is not null, throwing <see cref="InvalidOperationException" /> if it is.
    /// </summary>
    /// <typeparam name="T">The type of the argument.</typeparam>
    /// <param name="argument">The argument to verify.</param>
    /// <param name="message">The optional exception message.</param>
    /// <returns>The provided <paramref name="argument" />.</returns>
    [return: NotNullIfNotNull(nameof(argument))]
    public static T NotNull<T>([NotNull] T? argument, string? message = default)
        where T : class
    {
        if (argument is null)
        {
            throw message is null
                ? new InvalidOperationException()
                : new InvalidOperationException(message);
        }

        return argument;
    }
}
