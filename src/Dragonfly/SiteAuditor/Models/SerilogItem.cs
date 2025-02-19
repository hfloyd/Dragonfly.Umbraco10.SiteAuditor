namespace Dragonfly.SiteAuditor.Models;

using System;
using System.Collections.Generic;

using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog.Events;
using Serilog.Parsing;

public partial class SerilogItem
{
	public string FileName { get; set; }
	public DateTime? FileDate { get; set; }


	[JsonProperty("@t")]
	public DateTimeOffset Timestamp { get; set; }

	[JsonProperty("@mt")]
	public string MessageTemplate { get; set; }

	public IReadOnlyDictionary<string, LogEventPropertyValue> AllProperties { get; set; }

	public IEnumerable<KeyValuePair<string, LogEventPropertyValue>> OtherProperties { get; set; }

	public Exception? LoggedException { get; set; }

	
	[JsonProperty("Log4NetLevel")]
	public string Log4NetLevel { get; set; }


	[JsonProperty("RequestPath", NullValueHandling = NullValueHandling.Ignore)]
	public string RequestPath { get; set; }


	[JsonProperty("Message", NullValueHandling = NullValueHandling.Ignore)]
	public string Message { get; set; }

	[JsonProperty("NodeId", NullValueHandling = NullValueHandling.Ignore)]
	public long? NodeId { get; set; }

	[JsonProperty("NodeName", NullValueHandling = NullValueHandling.Ignore)]
	public string NodeName { get; set; }

	[JsonProperty("RequestUrl", NullValueHandling = NullValueHandling.Ignore)]
	public string RequestUrl { get; set; }

	//[JsonProperty("SourceContext")]
	//public string SourceContext { get; set; }
	
	//[JsonProperty("MachineName")]
	//public string MachineName { get; set; }

	//[JsonProperty("ActionName", NullValueHandling = NullValueHandling.Ignore)]
	//public string ActionName { get; set; }



	public SerilogItem(LogEvent LogEvent)
	{
		Timestamp = LogEvent.Timestamp;
		MessageTemplate = LogEvent.MessageTemplate.Text;
		Message = LogEvent.RenderMessage();
		Log4NetLevel = LogEvent.Level.ToString();
		LoggedException = LogEvent.Exception;
		AllProperties = LogEvent.Properties;
		
		NodeId = LogEvent.Properties.ContainsKey("NodeId") ? (long)Convert.ToInt64(LogEvent.Properties["NodeId"].ToString()) : 0;
		NodeName = LogEvent.Properties.ContainsKey("NodeName") ? LogEvent.Properties["NodeName"].ToString() : "";
		RequestUrl = LogEvent.Properties.ContainsKey("RequestUrl") ? LogEvent.Properties["RequestUrl"].ToString() : "";
		RequestPath = LogEvent.Properties.ContainsKey("RequestPath") ? LogEvent.Properties["RequestPath"].ToString() : "";

		//SourceContext = LogEvent.Properties.ContainsKey("SourceContext") ? LogEvent.Properties["SourceContext"].ToString() : "";
		//MachineName = LogEvent.Properties.ContainsKey("MachineName") ? LogEvent.Properties["MachineName"].ToString() : "";
		//ActionName = LogEvent.Properties.ContainsKey("ActionName") ? LogEvent.Properties["ActionName"].ToString() : "";

		var excludeProps = new List<string> { "NodeId", "NodeName", "RequestUrl", "RequestPath",  "Log4NetLevel" };
		OtherProperties = AllProperties.Where(n => !excludeProps.Contains(n.Key));

	}

	public SerilogItem() { }
	public static SerilogItem FromJson(string json) => JsonConvert.DeserializeObject<SerilogItem>(json, Dragonfly.SiteAuditor.Models.Converter.Settings);

	public static List<SerilogItem> ListFromJson(string json) => JsonConvert.DeserializeObject<List<SerilogItem>>(json, Dragonfly.SiteAuditor.Models.Converter.Settings);


}

//public partial class EventId
//{
//	[JsonProperty("Id")]
//	public long Id { get; set; }

//	[JsonProperty("Name")]
//	public string Name { get; set; }
//}


// public enum L { Error, Warning };

//public enum Log4NetLevel
//{
//	Error, Info, Warn, Fatal, Debug,
//	Unknown
//};


internal static class Converter
{
	public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	{
		MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
		DateParseHandling = DateParseHandling.None,
		Converters =
			{
               // LConverter.Singleton,
             //   Log4NetLevelConverter.Singleton,
			  new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
			},
	};
}

public static class Serialize
{
	public static string ToJson(this SerilogItem self) => JsonConvert.SerializeObject(self, Dragonfly.SiteAuditor.Models.Converter.Settings);
	public static string ToJson(this List<SerilogItem> self) => JsonConvert.SerializeObject(self, Dragonfly.SiteAuditor.Models.Converter.Settings);
}

internal static class SerilogHelpers
{


}

//internal class LConverter : JsonConverter
//{
//    public override bool CanConvert(Type t) => t == typeof(L) || t == typeof(L?);

//public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
//{
//    if (reader.TokenType == JsonToken.Null) return null;
//    var value = serializer.Deserialize<string>(reader);
//    switch (value)
//    {
//        case "Error":
//            return L.Error;
//        case "Warning":
//            return L.Warning;
//    }
//    throw new Exception("Cannot unmarshal type L");

//}

//public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
//    {
//        if (untypedValue == null)
//        {
//            serializer.Serialize(writer, null);
//            return;
//        }
//        var value = (L)untypedValue;
//        switch (value)
//        {
//            case L.Error:
//                serializer.Serialize(writer, "Error");
//                return;
//            case L.Warning:
//                serializer.Serialize(writer, "Warning");
//                return;
//        }
//        throw new Exception("Cannot marshal type L");
//    }

//    public static readonly LConverter Singleton = new LConverter();
//}


//internal class Log4NetLevelConverter : JsonConverter
//{
//	public override bool CanConvert(Type t) => t == typeof(Log4NetLevel) || t == typeof(Log4NetLevel?);

//	public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
//	{
//		if (reader.TokenType == JsonToken.Null) return Log4NetLevel.Unknown;

//		var value = serializer.Deserialize<string>(reader);
//		if (string.IsNullOrEmpty(value))
//		{
//			return Log4NetLevel.Unknown;
//		}

//		Log4NetLevel result;
//		value = value.Replace(" ", "");
//		bool matchFound = Log4NetLevel.TryParse(value, true, out result);
//		if (!matchFound)
//		{
//			switch (value)
//			{
//				case "ERROR":
//					result = Log4NetLevel.Error;
//					break;

//				case "INFO":
//					result = Log4NetLevel.Info;
//					break;
//				case "WARN":
//					result = Log4NetLevel.Warn;
//					break;
//				case "FATAL":
//					result = Log4NetLevel.Fatal;
//					break;
//				case "DEBUG":
//					result = Log4NetLevel.Debug;
//					break;
//				default:
//					result = Log4NetLevel.Unknown;
//					break;
//			}
//		}

//		return result;
//	}



//	public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
//	{
//		if (untypedValue == null)
//		{
//			serializer.Serialize(writer, null);
//			return;
//		}
//		var value = (Log4NetLevel)untypedValue;
//		switch (value)
//		{
//			case Log4NetLevel.Error:
//				serializer.Serialize(writer, "ERROR");
//				return;
//			case Log4NetLevel.Info:
//				serializer.Serialize(writer, "INFO ");
//				return;
//			case Log4NetLevel.Warn:
//				serializer.Serialize(writer, "WARN ");
//				return;
//		}
//		throw new Exception("Cannot marshal type Log4NetLevel");
//	}

//	public static readonly Log4NetLevelConverter Singleton = new Log4NetLevelConverter();
//}


//Code to handle Serilization/Deserialization


