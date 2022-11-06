namespace Gate;

public class Breaker {

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

    public Task<T> Execute<T>(Func<Task<T>> action) {
        return action.Invoke();
    }
}