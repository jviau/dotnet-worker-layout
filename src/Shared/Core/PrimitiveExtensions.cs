// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Globalization;

namespace Microsoft.Azure.Functions;

/// <summary>
/// Extensions for primitive types.
/// </summary>
internal static class PrimitiveExtensions
{
    /// <summary>
    /// Converts an <see cref="int" /> to <see cref="string" /> with the <see cref="CultureInfo.InvariantCulture" />.
    /// </summary>
    /// <param name="i">The int to convert.</param>
    /// <returns>The int as a string.</returns>
    public static string ToStringInvariant(this int i)
        => i.ToString(CultureInfo.InvariantCulture);
}
