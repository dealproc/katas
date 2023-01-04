using System;

namespace Toolkit.Tests;

internal class TestClock : IClock {
    DateTime _currentTime = DateTime.UtcNow;

    public DateTime UtcNow => _currentTime;

    public void Advance(TimeSpan time) {
        _currentTime = _currentTime.Add(time);
    }
}