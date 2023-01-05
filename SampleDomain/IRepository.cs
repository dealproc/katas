namespace SampleDomain {
    using System;
    using System.Threading.Tasks;

    public interface IRepository {
        Task Save(IEventSource source);

        Task<T> Load<T>(Guid id) where T : class, IEventSource;
    }
}