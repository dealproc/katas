namespace SampleDomain {
    using System;

    using EventStore.Client;

    public sealed class AllStreamPublisher : IDisposable {
        EventStoreClient _conn;

        private readonly EventBus _eventBus;


        private StreamSubscription _subscription;

        public AllStreamPublisher(EventStoreClient conn, EventBus eventBus) {
            _conn = conn;
            _eventBus = eventBus;
        }

        public async Task StartAsync() {
            _subscription = await _conn.SubscribeToAllAsync(
                FromAll.Start,
                (sub, evt, token) => {
                    if (evt.Event.Data.Length <= 0 || !evt.Event.ContentType.Contains("json", StringComparison.OrdinalIgnoreCase) || evt.Event.EventType.StartsWith("$"))
                        return Task.CompletedTask;

                    var deserializedEvent = evt.Deserialize();
                    if (deserializedEvent is IEvent message) {
                        _eventBus.Publish(message, new Checkpoint(evt.Event.EventStreamId, evt.Event.Position.CommitPosition));
                    }

                    return Task.CompletedTask;
                },
                true);
        }

        public void Dispose() {
            _subscription?.Dispose();
            _subscription = null;
        }
    }
}