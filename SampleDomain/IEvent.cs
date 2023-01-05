namespace SampleDomain {
    public interface IEvent : IMessage {
    }

    public class Event : Message, IEvent {
    }
}