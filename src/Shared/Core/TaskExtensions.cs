// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Azure.Functions;

/// <summary>
/// Extensions for tasks and value-tasks.
/// </summary>
internal static class TaskExtensions
{
    /// <summary>
    /// Helper to 'consume' a task to avoid analyzer warnings for a non-awaited task.
    /// </summary>
    /// <param name="task">The task to forget.</param>
    public static void Forget(this Task task)
    {
        Check.NotNull(task);
    }

    /// <summary>
    /// Helper to 'consume' a task to avoid analyzer warnings for a non-awaited task.
    /// </summary>
    /// <param name="task">The task to forget.</param>
    public static void Forget(this ValueTask task)
    {
        // no op.
    }

    /// <summary>
    /// Helper to 'consume' a task to avoid analyzer warnings for a non-awaited task.
    /// </summary>
    /// <typeparam name="T">The type held by the value task.</typeparam>
    /// <param name="task">The task to forget.</param>
    public static void Forget<T>(this ValueTask<T> task)
        where T : class
    {
        // no op.
    }

    /// <summary>
    /// Converts a <see cref="ValueTask{T}" /> to a <see cref="ValueTask" /> without a state machine or allocations (in
    /// most cases).
    /// </summary>
    /// <typeparam name="T">The type held by the original value task.</typeparam>
    /// <param name="valueTask">The value task to convert.</param>
    /// <returns>A value task.</returns>
    public static ValueTask AsValueTask<T>(this ValueTask<T> valueTask)
    {
        if (valueTask.IsCompleted)
        {
            valueTask.GetAwaiter().GetResult(); // rethrow any exceptions.
            return default;
        }

        return new ValueTask(valueTask.AsTask());
    }

    /// <summary>
    /// Tries to retrieve the exception from a task. This will be the same exception <c>GetAwaiter().GetResult()</c>
    /// would throw.
    /// </summary>
    /// <param name="task">The task to try and retrieve an exception from.</param>
    /// <param name="exception">Out param. The exception if available, null otherwise.</param>
    /// <returns>True if exception available, false otherwise.</returns>
    public static bool TryGetException(this Task task, [NotNullWhen(true)] out Exception? exception)
    {
        Check.NotNull(task);
        if (!task.IsFaulted)
        {
            exception = null;
            return false;
        }

        exception = task.Exception?.InnerExceptions?.FirstOrDefault();
        return exception is not null;
    }

    /// <summary>
    /// Gets the result of a task, assuming it has completed successfully.
    /// </summary>
    /// <typeparam name="T">The result type of the task.</typeparam>
    /// <param name="task">The task to get the result from.</param>
    /// <returns>The result of the task.</returns>
    public static T GetResultAssumesCompleted<T>(this Task<T> task)
    {
        Check.NotNull(task);
        if (task.Status != TaskStatus.RanToCompletion)
        {
            throw new InvalidOperationException($"Cannot get result for task state of '{task.Status}'.");
        }

        return task.Result;
    }
}
