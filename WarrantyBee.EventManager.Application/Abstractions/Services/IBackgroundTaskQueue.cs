namespace WarrantyBee.EventManager.Application.Abstractions.Services;

/// <summary>
/// Defines a thread-safe background task queue for offloading non-critical operations.
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// Enqueues an asynchronous work item to the background queue.
    /// </summary>
    /// <param name="workItem">The asynchronous function representing the work item.</param>
    /// <returns>A value task representing the asynchronous operation.</returns>
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);

    /// <summary>
    /// Dequeues an asynchronous work item from the background queue.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the dequeue operation.</param>
    /// <returns>The dequeued work item.</returns>
    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}
