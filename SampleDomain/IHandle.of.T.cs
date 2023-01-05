namespace SampleDomain {
    public interface IHandle<in TEvent> where TEvent : IEvent {
        void Handle(TEvent e, Checkpoint checkpoint);
    }
}