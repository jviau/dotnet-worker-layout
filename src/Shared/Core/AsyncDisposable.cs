// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Functions;

/// <summary>
/// A struct for calling a simple delegate on dispose.
/// </summary>
internal struct AsyncDisposable : IAsyncDisposable
{
    private Func<ValueTask>? _callback;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncDisposable"/> struct.
    /// </summary>
    /// <param name="callback">The callback to invoke on disposal.</param>
    public AsyncDisposable(Func<ValueTask> callback)
    {
        _callback = callback;
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        Func<ValueTask>? callback = Interlocked.Exchange(ref _callback, null);
        return callback?.Invoke() ?? default;
    }
}
