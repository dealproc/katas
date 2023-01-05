namespace SampleDomain {
    public interface ICommand : IMessage {
    }

    public class Command : Message, ICommand {
    }
}