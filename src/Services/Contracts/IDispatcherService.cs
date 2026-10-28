using System;
using System.Threading.Tasks;

namespace BitWallpaper.Services.Contracts;

public interface IDispatcherService
{
    Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; }
    bool TryEnqueue(Action action);
    bool HasThreadAccess { get; }
    /*
    Task EnqueueAsync(Action action);
    Task<T> EnqueueAsync<T>(Func<T> func);
    Task EnqueueAsync(Func<Task> func);
    */
}
