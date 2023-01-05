namespace SampleDomain {
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using EventStore.Client;

    using Microsoft.Toolkit.HighPerformance;

    public static class Serializer {
        private static JsonSerializerOptions _options = new JsonSerializerOptions() {
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = false,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            PropertyNameCaseInsensitive = true,
            IncludeFields = true
        };
        static Serializer() {
            _options.Converters.Add(new EmptyGuidConverter());
            _options.Converters.Add(new JsonStringEnumConverter());
        }

        private static class KeyNames {
            public const string ClrName = "clrName";
            public const string ClrAssemblyQualifiedName = "clrAssemblyQualifiedName";
            public const string EventName = "eventName";
            public const string CorrelationId = "$correlationId";
            public const string CausationId = "$causationId";
            public const string MessageId = "messageId";
        }

        public static object Deserialize(this ResolvedEvent resolvedEvent) {
            var metaData = JsonSerializer.Deserialize<Dictionary<string, string>>(resolvedEvent.Event.Metadata.AsStream(), _options) ?? new();
            var clrQualifiedName = metaData.ContainsKey(KeyNames.ClrAssemblyQualifiedName)
                ? metaData[KeyNames.ClrAssemblyQualifiedName]
                : metaData.ContainsKey(KeyNames.ClrName)
                    ? metaData[KeyNames.ClrName]
                    : throw new InvalidDataException();
            var clrName = metaData.ContainsKey(KeyNames.ClrName)
                ? metaData[KeyNames.ClrName]
                : throw new InvalidDataException();

            var messageType = Type.GetType(clrQualifiedName);
            if (messageType == null && clrQualifiedName != clrName) {
                messageType = Type.GetType(clrName);
            }

            if (messageType == null) throw new InvalidDataException("Unknown data type for message.");

            var message = JsonSerializer.Deserialize(resolvedEvent.Event.Data.AsStream(), messageType, _options);

            if (message is IMessage msg) {
                msg.MessageId = metaData.ContainsKey(KeyNames.MessageId) ? Guid.Parse(metaData[KeyNames.MessageId]) : Guid.Empty;
                msg.CausationId = metaData.ContainsKey(KeyNames.CausationId) ? Guid.Parse(metaData[KeyNames.CausationId]) : Guid.Empty;
                msg.CorrelationId = metaData.ContainsKey(KeyNames.CorrelationId) ? Guid.Parse(metaData[KeyNames.CorrelationId]) : Guid.Empty;
            }

            //todo: how do we map the key/value dict to properties on the constructed event that may be implemented explicitly?

            return message;
        }

        public static EventData Serialize(this IEvent obj) {
            var eventType = obj.GetType();
            var eventName = (eventType.FullName ?? string.Empty).ResolveEventName();

            var dataBytes = JsonSerializer.SerializeToUtf8Bytes(obj, eventType, _options);

            var metaData = new Dictionary<string, string>() {
                { KeyNames.CorrelationId, obj.CorrelationId.ToString("N")},
                { KeyNames.CausationId, obj.CausationId.ToString("N") },
                { KeyNames.MessageId, obj.MessageId.ToString("N") },
                { KeyNames.EventName, eventName },
                { KeyNames.ClrName, eventType.FullName ?? string.Empty },
                { KeyNames.ClrAssemblyQualifiedName, eventType.AssemblyQualifiedName ?? string.Empty }
            };
            //todo: how can additional metadata items be pulled from the passed IEvent?
            var metaDataBytes = JsonSerializer.SerializeToUtf8Bytes(metaData, _options);

            return new EventData(Uuid.NewUuid(), eventName, dataBytes, metaDataBytes);
        }

        public static string ResolveEventName(this string eventName) {
            if (string.IsNullOrWhiteSpace(eventName)) return string.Empty;

            if (eventName.Contains("+")) return eventName.Substring(eventName.IndexOf("+") + 1);

            if (eventName.Contains(".")) return eventName.Substring(eventName.LastIndexOf(".") + 1);

            return eventName;
        }
    }
}