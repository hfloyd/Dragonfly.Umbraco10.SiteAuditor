namespace Dragonfly.SiteAuditor.Models
{
	using System;
	using Newtonsoft.Json;

	public class RawMediaWithCrops
	{
		[JsonProperty("key")] 
		public string Key { get; set; } = "";

		[JsonProperty("mediaKey")]
		public string MediaKey { get; set; }= "";

		public Guid GetMediaKey()
		{
			return Guid.Parse(MediaKey);
		}
	}
}
