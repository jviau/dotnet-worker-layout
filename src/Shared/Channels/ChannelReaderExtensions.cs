// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

#if NETSTANDARD2_0
using System.Runtime.CompilerServices;

namespace System.Threading.Channels;

internal static class ChannelReaderExtensions
{
    public static async IAsyncEnumerable<T> ReadAllAsync<T>(this ChannelReader<T> reader, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            while (reader.TryRead(out T? item))
            {
                yield return item;
            }
        }
    }
}
#endif
