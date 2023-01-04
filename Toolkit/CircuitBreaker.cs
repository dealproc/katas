namespace Gate;

public class Breaker<T> {
    private int _failedAttempts = 0;
    private long _halfOpenAt = 0;

    public int TripAfterFailuresCount { get; set; } = 3;
    public int TryCloseAfterSeconds { get; set; } = 15;
    public Task<T> WhenOpen { get; set; } = new Task<T>(() => throw new NotImplementedException());



    private bool CircuitIsFaulted => _failedAttempts >= TripAfterFailuresCount;
    private bool CircuitIsHalfOpen => _halfOpenAt < DateTime.UtcNow.Ticks && _halfOpenAt != 0;

    public async Task<T> Execute(Func<Task<T>> action) {
        if(CircuitIsHalfOpen) {
            Console.WriteLine("Circuit is half-open");
            try {
                var response = await action().TimeoutAfter(TimeSpan.FromSeconds(1));
                CloseCircuit();
                return response;
            } catch (TimeoutException) {
                ExtendOpenTimeout();
                return await WhenOpen;
            }
        }

        if (CircuitIsFaulted) { 
            Console.WriteLine("Circuit is faulted.");
            return await WhenOpen;
        }

        Console.WriteLine("Circuit is closed.");
        try {
            return await action().TimeoutAfter(TimeSpan.FromSeconds(1));
        } catch (TimeoutException) {
            IncrementFailureCount();
            return await WhenOpen;
        }
    }

    private void ExtendOpenTimeout() {
        _halfOpenAt = DateTime.UtcNow.Ticks + TimeSpan.FromSeconds(TryCloseAfterSeconds).Ticks;
        Console.WriteLine($"Circuit opened due to too many failures.  Will try to close at {new DateTime(_halfOpenAt)}");
    }

    private void OpenCircuit() {
        _halfOpenAt = DateTime.UtcNow.Ticks + TimeSpan.FromSeconds(TryCloseAfterSeconds).Ticks;
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

        if(CircuitIsFaulted) {
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