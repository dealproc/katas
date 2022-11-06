namespace Gate;

using Microsoft.Extensions.Logging.Abstractions;

public class Breaker<T> {
    public enum States {
        /// <summary>
        /// All systems are normal, continue.
        /// </summary>
        Closed,

        /// <summary>
        /// System previously failed, and we want to test the connection.
        /// </summary>
        HalfOpen,

        /// <summary>
        /// Systems have failed, we want to stop using the connection.
        /// </summary>
        Open
    }

    private int _failedAttempts = 0;
    private States _state = States.Closed;
    private System.Timers.Timer _timer;

    public int TripAfterFailuresCount { get; set; } = 3;
    public int TryCloseAfterSeconds { get; set; } = 5;
    public Task<T> WhenOpen { get; set; } = new Task<T>(() => throw new NotImplementedException());
    public ILogger Logger { get; set; } = NullLogger.Instance;

    public async Task<T> Execute(Func<Task<T>> action) {
        Logger.LogDebug($"Breaker state: {_state}");
        switch (_state) {
            case States.Closed:
                try {
                    return await action();
                }
                catch {
                    IncrementFailureCount();
                    throw;
                }
            case States.HalfOpen:
                try {
                    var result = await action();
                    CloseCircuit();
                    return result;
                }
                catch {
                    ReopenCircuit();
                    throw;
                }
        }

        return await WhenOpen;
    }

    private void ReopenCircuit() {
        _state = States.Open;
        _timer?.Stop();
        _timer?.Start();
    }

    private void CloseCircuit() {
        _failedAttempts = 0;
        _state = States.Closed;
        Logger.LogDebug("Circuit is closed.");
    }

    private void IncrementFailureCount() {
        if (_failedAttempts < TripAfterFailuresCount) {
            _failedAttempts += 1;
            Logger.LogDebug("Current Failure Count: {@FailedAttempts}", _failedAttempts);

            if (_failedAttempts >= TripAfterFailuresCount) {
                Logger.LogDebug("Circuit opened due to too many failures.");
                _state = States.Open;

                try {
                    if (_timer == null) {
                        _timer = new System.Timers.Timer();
                        _timer.AutoReset = false;
                        _timer.Elapsed += (s, e) => {
                            Logger.LogDebug("Circuit should attempt to close.");
                            _state = States.HalfOpen;
                        };
                    }
                    _timer.Interval = TimeSpan.FromSeconds(TryCloseAfterSeconds).TotalMilliseconds;
                    _timer.Start();
                    Logger.LogDebug("Reset timer is activated");
                }
                catch (Exception exc) {
                    Logger.LogWarning(exc, "");
                }
            }
        }
    }
}