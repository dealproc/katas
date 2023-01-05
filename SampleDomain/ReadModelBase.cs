namespace SampleDomain {
    using System;

    using EventStore.Client;

    public class ReadModelBase {
        protected EventBus Bus { get; private set; }
        private readonly EventStoreClient _client;
        private readonly IStreamListener _listener;

        public ReadModelBase(EventBus bus, EventStoreClient client, IStreamListener listener) {
            Bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _listener = listener ?? throw new ArgumentNullException(nameof(listener));
        }

        protected ulong Read<TAggregate>() {
            var streamName = $"$ce-{typeof(TAggregate).Name.ToLowerInvariant()}";
            var reader = _client.ReadStreamAsync(
                direction: Direction.Forwards,
                streamName: streamName,
                revision: StreamPosition.Start,
                resolveLinkTos: true);

            if (AsyncHelper.RunSync(() => reader.ReadState) == ReadState.StreamNotFound) return 0;

            Checkpoint? check = default;
            AsyncHelper.RunSync(() => reader.ForEachAsync((resolved) => {
                if (resolved.Event.EventType.StartsWith("$")) return;

                var @event = resolved.Deserialize() as IEvent;
                check = new Checkpoint(streamName, resolved.OriginalEventNumber.ToUInt64());

                if (@event == null) return;

                Bus.Publish(@event, check);
            }));

            return check?.Position ?? 0;
        }

        protected Task Start<TAggregate>(ulong startingPosition = 0) => _listener.Start<TAggregate>(startingPosition);
    }
}
