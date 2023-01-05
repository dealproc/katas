namespace Toolkit;

public class Breaker<T> {
    private readonly IClock _clock;
    internal int _failedAttempts = 0;
    internal long _halfOpenAt = 0;
    internal bool _circuitIsFaulted => _failedAttempts >= TripAfterFailuresCount;
    internal bool _circuitIsHalfOpen => _halfOpenAt < _clock.UtcNow.Ticks && _halfOpenAt != 0;

    public int TripAfterFailuresCount { get; set; } = 3;
    public int TryCloseAfterSeconds { get; set; } = 15;
    public Task<T> WhenOpen { get; set; } = Task.FromException<T>(new NotImplementedException());

    public Breaker(IClock clock) {
        _clock = clock;
    }

    public async Task<T> Execute(Func<Task<T>> action) {
        if (_circuitIsHalfOpen) {
            Console.WriteLine("Circuit is half-open");
            try {
                var response = await action().TimeoutAfter(TimeSpan.FromSeconds(1));
                CloseCircuit();
                return response;
            } catch {
                ExtendOpenTimeout();
            }
        } else if (_circuitIsFaulted) {
            Console.WriteLine("Circuit is faulted.");
        } else {
            try {
                Console.WriteLine("Circuit is closed.");
                return await action().TimeoutAfter(TimeSpan.FromSeconds(1));
            } catch {
                IncrementFailureCount();
            }
        }

        return await WhenOpen;
    }

    private void ExtendOpenTimeout() {
        _halfOpenAt = _clock.UtcNow.Ticks + TimeSpan.FromSeconds(TryCloseAfterSeconds).Ticks;
        Console.WriteLine($"Circuit opened due to too many failures.  Will try to close at {new DateTime(_halfOpenAt)}");
    }

    private void OpenCircuit() {
        _halfOpenAt = _clock.UtcNow.Ticks + TimeSpan.FromSeconds(TryCloseAfterSeconds).Ticks;
        Console.WriteLine($"Circuit opened due to too many failures.  Will try to close at {new DateTime(_halfOpenAt)}");
    }

    private void CloseCircuit() {
        Console.WriteLine("Circuit has been closed.");
        _halfOpenAt = 0;
        _failedAttempts = 0;
    }

    private void IncrementFailureCount() {
        _failedAttempts += 1;

        Console.WriteLine($"Failure Count: {_failedAttempts} - {TripAfterFailuresCount}");

        if (_circuitIsFaulted) {
            OpenCircuit();
            return;
        }
    }
}

static class Extensions {
    public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan timeout, CancellationTokenSource cancellationTokenSource = default) {
        if (task == await Task.WhenAny(task, Task.Delay(timeout)))
            return await task;
        else {
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();

            throw new TimeoutException();
        }
    }
}