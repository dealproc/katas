namespace SampleDomain {
    using System.Reflection;

    public class EventDrivenStateMachine {
        public void Apply(IEvent @event) {
            //NOTE: May need to add (this as dynamic) before the GetType() call.
            var apply = GetType().GetMethod("Apply", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {@event.GetType()}, null);
            apply?.Invoke(this, new object[] { @event });
        }
    }
}
