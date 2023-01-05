namespace SampleDomain {
    using System;
    using System.Collections.Generic;

    public class EventBus {
        private readonly Dictionary<Type, List<Action<IEvent, Checkpoint>>> _eventHandlers =
            new Dictionary<Type, List<Action<IEvent, Checkpoint>>>();


        public void Register<T>(IHandle<T> subscriber) where T : IEvent {
            var eventType = typeof(T);

            if (!_eventHandlers.ContainsKey(eventType)) {
                _eventHandlers.Add(eventType, new List<Action<IEvent, Checkpoint>>());
            }

            _eventHandlers[eventType].Add((e, c) => subscriber.Handle((T)e, c));
        }

        public void Publish(IEvent e, Checkpoint c) {
            var eType = e.GetType();

            if (!_eventHandlers.ContainsKey(eType)) {
                return;
            }

            foreach (var action in _eventHandlers[eType]) {
                action(e, c);
            }
        }
    }
}