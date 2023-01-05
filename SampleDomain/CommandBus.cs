namespace SampleDomain {
    using System;
    using System.Collections.Generic;

    public class CommandBus {
        private readonly Dictionary<Type, Action<IMessage>> _handlers = new Dictionary<Type, Action<IMessage>>();

        public void Publish(ICommand message) {
            var mType = message.GetType();

            if (!_handlers.ContainsKey(mType)) {
                return;
            }

            _handlers[mType](message);
        }


        public void Subscribe<T>(IHandleCommand<T> subscriber) where T : ICommand {
            var cType = typeof(T);

            if (_handlers.ContainsKey(cType)) {
                throw new InvalidOperationException("Can't register multiple handlers for a command.");
            }

            _handlers[cType] = msg => subscriber.Handle((T)msg);
        }
    }
}