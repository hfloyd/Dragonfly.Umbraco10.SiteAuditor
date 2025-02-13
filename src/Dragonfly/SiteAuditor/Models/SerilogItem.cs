namespace Dragonfly.SiteAuditor.Models;

    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class SerilogItem
    {
        [JsonProperty("@t")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("@mt")]
        public string MessageTemplate { get; set; }

        [JsonProperty("SourceContext")]
        public string SourceContext { get; set; }

        [JsonProperty("ProcessId")]
        public long ProcessId { get; set; }

        [JsonProperty("ProcessName")]
        public string ProcessName { get; set; }

        [JsonProperty("ThreadId")]
        public long ThreadId { get; set; }

        [JsonProperty("ApplicationId")]
        public string ApplicationId { get; set; }

        [JsonProperty("MachineName")]
        public string MachineName { get; set; }

        [JsonProperty("Log4NetLevel")]
        public Log4NetLevel Log4NetLevel { get; set; }

        [JsonProperty("busInstance", NullValueHandling = NullValueHandling.Ignore)]
        public string BusInstance { get; set; }

        [JsonProperty("SignalSource", NullValueHandling = NullValueHandling.Ignore)]
        public string SignalSource { get; set; }

        [JsonProperty("StartMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string StartMessage { get; set; }

        [JsonProperty("TimingId", NullValueHandling = NullValueHandling.Ignore)]
        public string TimingId { get; set; }

        [JsonProperty("EndMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string EndMessage { get; set; }

        [JsonProperty("Duration", NullValueHandling = NullValueHandling.Ignore)]
        public long? Duration { get; set; }

        [JsonProperty("job", NullValueHandling = NullValueHandling.Ignore)]
        public string Job { get; set; }

        [JsonProperty("MigrationName", NullValueHandling = NullValueHandling.Ignore)]
        public string MigrationName { get; set; }

        [JsonProperty("OrigState", NullValueHandling = NullValueHandling.Ignore)]
        public string OrigState { get; set; }

        [JsonProperty("edition", NullValueHandling = NullValueHandling.Ignore)]
        public string Edition { get; set; }

        [JsonProperty("flag", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Flag { get; set; }

        [JsonProperty("EnvName", NullValueHandling = NullValueHandling.Ignore)]
        public string EnvName { get; set; }

        [JsonProperty("ContentRoot", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentRoot { get; set; }

        [JsonProperty("@tr", NullValueHandling = NullValueHandling.Ignore)]
        public string Tr { get; set; }

        [JsonProperty("@sp", NullValueHandling = NullValueHandling.Ignore)]
        public string Sp { get; set; }

        [JsonProperty("LocalContentDbExists", NullValueHandling = NullValueHandling.Ignore)]
        public bool? LocalContentDbExists { get; set; }

        [JsonProperty("LocalMediaDbExists", NullValueHandling = NullValueHandling.Ignore)]
        public bool? LocalMediaDbExists { get; set; }

        [JsonProperty("RequestId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? RequestId { get; set; }

        [JsonProperty("RequestPath", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestPath { get; set; }

        [JsonProperty("HttpRequestId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? HttpRequestId { get; set; }

        [JsonProperty("HttpRequestNumber", NullValueHandling = NullValueHandling.Ignore)]
        public long? HttpRequestNumber { get; set; }

        [JsonProperty("HttpSessionId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? HttpSessionId { get; set; }

        [JsonProperty("@l", NullValueHandling = NullValueHandling.Ignore)]
        public L? L { get; set; }

        [JsonProperty("Message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("NodeId", NullValueHandling = NullValueHandling.Ignore)]
        public long? NodeId { get; set; }

        [JsonProperty("NodeName", NullValueHandling = NullValueHandling.Ignore)]
        public string NodeName { get; set; }

        [JsonProperty("RequestUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri RequestUrl { get; set; }

        [JsonProperty("ActionId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? ActionId { get; set; }

        [JsonProperty("ActionName", NullValueHandling = NullValueHandling.Ignore)]
        public string ActionName { get; set; }

        [JsonProperty("Username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username { get; set; }

        [JsonProperty("IPAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string IpAddress { get; set; }

        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
        public long? Count { get; set; }

        [JsonProperty("@x", NullValueHandling = NullValueHandling.Ignore)]
        public string X { get; set; }

        [JsonProperty("EventId", NullValueHandling = NullValueHandling.Ignore)]
        public EventId EventId { get; set; }
    }

    public partial class EventId
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
    }

  
    public enum L { Error, Warning };

    public enum Log4NetLevel { Error, Info, Warn };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                LConverter.Singleton,
                Log4NetLevelConverter.Singleton,
              new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class LConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(L) || t == typeof(L?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "Error":
                    return L.Error;
                case "Warning":
                    return L.Warning;
            }
            throw new Exception("Cannot unmarshal type L");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (L)untypedValue;
            switch (value)
            {
                case L.Error:
                    serializer.Serialize(writer, "Error");
                    return;
                case L.Warning:
                    serializer.Serialize(writer, "Warning");
                    return;
            }
            throw new Exception("Cannot marshal type L");
        }

        public static readonly LConverter Singleton = new LConverter();
    }


    internal class Log4NetLevelConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Log4NetLevel) || t == typeof(Log4NetLevel?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "ERROR":
                    return Log4NetLevel.Error;
                case "INFO ":
                    return Log4NetLevel.Info;
                case "WARN ":
                    return Log4NetLevel.Warn;
            }
            throw new Exception("Cannot unmarshal type Log4NetLevel");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Log4NetLevel)untypedValue;
            switch (value)
            {
                case Log4NetLevel.Error:
                    serializer.Serialize(writer, "ERROR");
                    return;
                case Log4NetLevel.Info:
                    serializer.Serialize(writer, "INFO ");
                    return;
                case Log4NetLevel.Warn:
                    serializer.Serialize(writer, "WARN ");
                    return;
            }
            throw new Exception("Cannot marshal type Log4NetLevel");
        }

        public static readonly Log4NetLevelConverter Singleton = new Log4NetLevelConverter();
    }


//Code to handle Serilization/Deserialization
    public partial class SerilogItem
    {
        public SerilogItem() { }
        public static SerilogItem FromJson(string json) => JsonConvert.DeserializeObject<SerilogItem>(json, Dragonfly.SiteAuditor.Models.Converter.Settings);

	    public static List<SerilogItem> ListFromJson(string json) => JsonConvert.DeserializeObject<List<SerilogItem>>(json, Dragonfly.SiteAuditor.Models.Converter.Settings);
    }

    public static class Serialize
    {
	    public static string ToJson(this List<SerilogItem> self) => JsonConvert.SerializeObject(self, Dragonfly.SiteAuditor.Models.Converter.Settings);
    }

