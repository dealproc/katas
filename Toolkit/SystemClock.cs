namespace Toolkit;

public class SystemClock : IClock {
    public DateTime UtcNow => DateTime.UtcNow;
}