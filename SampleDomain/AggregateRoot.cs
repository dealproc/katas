namespace SampleDomain {
    using System;
    using System.Collections.Generic;

    public abstract class AggregateRoot : EventDrivenStateMachine, IEventSource {
        private List<IEvent> _pendingEvents = new List<IEvent>();
        protected void Raise(IEvent @event) {
            _pendingEvents.Add(@event);
            Apply(@event);
        }

        string IEventSource.Name => GetType().Name;

        protected Guid Id;
        Guid IEventSource.Id => Id;

        private long _version = long.MaxValue;
        long IEventSource.Version => _version;

        void IEventSource.Hydrate(IEnumerable<IEvent> events) {
            if (_pendingEvents == null) _pendingEvents = new List<IEvent>();
            _version = long.MaxValue;

            foreach(var @event in events) {
                Apply(@event);

                _version = _version == long.MaxValue
                    ? 0
                    : _version++;
            }
        }

        IReadOnlyList<IEvent> IEventSource.TakeEvents() {
            if(Id == Guid.Empty) {
                throw new InvalidOperationException("Aggregate must have an ID set prior to taking events.");
            }

            var pending = new List<IEvent>(_pendingEvents);
            _pendingEvents.Clear();
            return pending;
        }
    }
}