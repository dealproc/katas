using Shouldly;

using Xunit;
using Xunit.Abstractions;

namespace Toolkit.Tests;

public class Circuit_breakers_should : IAsyncLifetime {
    private readonly TestClock _testClock = new TestClock();
    private readonly Breaker<bool> _breaker;
    private readonly IDisposable _logCapture;

    public Circuit_breakers_should(ITestOutputHelper testOutputHelper) {
        _breaker = new Breaker<bool>(_testClock);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() {
        _logCapture?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task be_closed_by_default() {
        (await _breaker.Execute(() => Task.FromResult<bool>(true))).ShouldBeTrue();
    }

    [Fact]
    public async Task execute_appropriate_method_when_failing() {
        _breaker.WhenOpen = Task.FromException<bool>(new BreakerTestsException());
        Should.Throw<BreakerTestsException>(() => _breaker.Execute(() => Task.FromException<bool>(new TimeoutException("Should not be thrown."))));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task increment_its_failed_attempts_for_each_failure(int numberOfFailures) {
        for (var i = 0; i < numberOfFailures; i++) {
            Should.Throw<NotImplementedException>(() => _breaker.Execute(() => Task.FromException<bool>(new TimeoutException())));
        }

        _breaker._failedAttempts.ShouldBe(numberOfFailures);
    }

    [Fact]
    public void be_opened_after_maximum_failures() {
        for (var i = 0; i < _breaker.TripAfterFailuresCount; i++) {
            Should.Throw<NotImplementedException>(() => _breaker.Execute(() => Task.FromException<bool>(new TimeoutException())));
        }

        _breaker._circuitIsFaulted.ShouldBeTrue();
    }

    [Fact]
    public void be_half_open_after_time_passes() {
        for (var i = 0; i < _breaker.TripAfterFailuresCount; i++) {
            Should.Throw<NotImplementedException>(() => _breaker.Execute(() => Task.FromException<bool>(new TimeoutException())));
        }

        _testClock.Advance(TimeSpan.FromSeconds(_breaker.TryCloseAfterSeconds).Add(TimeSpan.FromTicks(1))); // should be in half-open state.

        _breaker._circuitIsHalfOpen.ShouldBeTrue();
    }

    [Fact]
    public void return_whenopen_value_when_faulted() {
        for (var i = 0; i < _breaker.TripAfterFailuresCount; i++) {
            Should.Throw<NotImplementedException>(() => _breaker.Execute(() => Task.FromException<bool>(new TimeoutException())));
        }

        _breaker.WhenOpen = Task.FromException<bool>(new BreakerTestsException());

        Should.Throw<BreakerTestsException>(async () => await _breaker.Execute(() => Task.FromException<bool>(new TimeoutException())));
    }

    [Fact]
    public async Task reopen_after_time_passes_and_subsequent_calls_succeed() {
        for (var i = 0; i < _breaker.TripAfterFailuresCount; i++) {
            Should.Throw<NotImplementedException>(() => _breaker.Execute(() => Task.FromException<bool>(new TimeoutException())));
        }

        _testClock.Advance(TimeSpan.FromSeconds(_breaker.TryCloseAfterSeconds).Add(TimeSpan.FromTicks(1))); // should be in half-open state.

        await Should.NotThrowAsync(async () => await _breaker.Execute(() => Task.FromResult(true)));
    }

    private class BreakerTestsException : Exception { }
}