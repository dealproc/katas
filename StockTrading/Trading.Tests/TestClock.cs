namespace Trading.Tests {
    class TestClock : IClock {
        private DateTime _start = DateTime.UtcNow;

        public DateTime GetUtc => _start;

        public void Advance(TimeSpan time) {
            _start = _start.Add(time);
        }
    }
}
