using System.Net.Sockets;
using Polly;
using Polly.Retry;

namespace QueryLibrary.Resiliency;

internal static class ResilienceUtil
{
    private const int RetryIntervalMilliseconds = 166;
    private const int MaxRetryCount = 3;
        
    private static AsyncRetryPolicy GetPolicy(Action<Exception, TimeSpan, int>? onRetry = null)
    {
        return Policy
            .Handle<TimeoutException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(
                MaxRetryCount,
                _ => TimeSpan.FromMilliseconds(RetryIntervalMilliseconds),
                (exception, timespan, retryCount, _) =>
                {
                    // Invoke onRetry delegate if it is not null to enable logging on retry, for example.
                    onRetry?.Invoke(exception, timespan, retryCount);
                }
            );
    }

    public static async Task<TResult> ExecuteSafelyAsync<TResult>(Func<Task<TResult>> del, Action<Exception, TimeSpan, int>? onRetry = null)
    {
        var policy = GetPolicy(onRetry);
        return await policy.ExecuteAsync(del);
    }
}