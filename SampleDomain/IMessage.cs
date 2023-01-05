namespace SampleDomain {
    public interface IMessage {
        Guid MessageId { get; internal set; }
        Guid CorrelationId { get; internal set; }
        Guid CausationId { get; internal set; }
    }

    public class Message : IMessage {
        Guid IMessage.MessageId { get; set; }
        Guid IMessage.CorrelationId { get; set; }
        Guid IMessage.CausationId { get; set; }

        public Message() {
            ((IMessage)this).MessageId = Guid.NewGuid();
        }
    }
}