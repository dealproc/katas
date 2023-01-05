namespace SampleDomain {
    using System.Threading.Tasks;

    public interface IHandleCommand<in TCommand> where TCommand : ICommand {
        Task<bool> Handle(TCommand c);
    }
}