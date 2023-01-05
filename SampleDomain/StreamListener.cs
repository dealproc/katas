namespace SampleDomain {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using EventStore.Client;

    public interface IStreamListener {
        Task Start<T>(ulong startPosition);
    }

    public class StreamListener : IStreamListener, IDisposable {
        private readonly EventStoreClient _client;
        private readonly EventBus _bus;
        private readonly List<StreamSubscription> _subscriptions = new();

        public StreamListener(EventStoreClient client, EventBus bus) {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public async Task Start<T>(ulong startPosition) {
            var s = await _client.SubscribeToStreamAsync(
                streamName: $"$ce-{typeof(T).Name.ToLowerInvariant()}",
                start: startPosition > 0 ? FromStream.After(StreamPosition.FromStreamRevision(startPosition)) : FromStream.Start,
                eventAppeared: (subscription, @event, token) => {
                    if (@event.Event.EventType.StartsWith("$")) return Task.CompletedTask;

                    var deserialized = @event.Deserialize() as IEvent;
                    if (deserialized != null) {
                        _bus.Publish(deserialized, new Checkpoint(@event.Event.EventStreamId, @event.OriginalEventNumber.ToUInt64()));
                    }
                    return Task.CompletedTask;
                },
                resolveLinkTos: true
            );
            _subscriptions.Add(s);
        }

        public void Dispose() {
            _subscriptions.ForEach(s => s.Dispose());
            _subscriptions.Clear();
        }
    }
}
