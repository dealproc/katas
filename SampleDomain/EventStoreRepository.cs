namespace SampleDomain {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    using EventStore.Client;

    public class EventStoreRepository : IRepository {
        private readonly EventStoreClient _client;

        public EventStoreRepository(EventStoreClient client) {
            _client = client;
        }

        public Task Save(IEventSource source) {
            var eventData = new List<EventData>();

            foreach (var @event in source.TakeEvents()) {
                eventData.Add(@event.Serialize());
            }

            var stream = $"{source.Name.ToLowerInvariant()}-{source.Id:N}";

            // need to discuss this one.  not sure if there's a better way to do this.
            return _client.ConditionalAppendToStreamAsync(
                streamName: stream,
                expectedState: StreamState.Any,
                eventData
            );
        }

        public async Task<T> Load<T>(Guid id) where T : class, IEventSource {
            var writer = (T)FormatterServices.GetUninitializedObject(typeof(T));

            var streamName = $"{writer.Name.ToLowerInvariant().ToLowerInvariant()}-{id:N}";

            EventStoreClient.ReadStreamResult reader;
            var events = new List<IEvent>();

            reader = _client.ReadStreamAsync(
                direction: Direction.Forwards, 
                streamName: streamName, 
                revision: 0,
                resolveLinkTos: true);

            if ((await reader.ReadState) == ReadState.StreamNotFound) {
                throw new Exception($"Stream not found {streamName}");
            }

            await reader.ForEachAsync((resolved) => {
                var e = resolved.Deserialize() as IEvent;
                if (e != null) {
                    events.Add(e);
                }
            });

            writer.Hydrate(events);

            return writer;
        }
    }
}