namespace Dragonfly.SiteAuditor
{
	using System.Collections.Generic;


	//const long MaxLogFileSizeInBytes = 10485760; // 10 MB

	//  const long MaxLogFileSizeInBytes =	524288000; //500 MB
	public class DragonflySiteAuditorConfig
	{
		//As per https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0#bind-hierarchical-configuration-data-using-the-options-pattern-1

		//AppSettings Config Section name
		public const string DragonflySiteAuditor = "DragonflySiteAuditor";

		#region Properties

		public string PluginPath { get; set; } = "~/App_Plugins/Dragonfly.SiteAuditor/";
		
		public string DataPath { get; set; } = "~/App_Data/DragonflySiteAuditor/";
		
		public long LimitProcessingLogsLargerThanBytes { get; set; }
		
		public long NeverProcessLogsLargerThanBytes { get; set; }
		
		public List<string> ExcludeLevelsToManageLargeLogs { get; set; } = new List<string>();

		#endregion

		public void SetDefaults()
		{
			LimitProcessingLogsLargerThanBytes = 314572800; // 300 MB
			NeverProcessLogsLargerThanBytes = 1048576000; //1 GB
			ExcludeLevelsToManageLargeLogs= new List<string>
			{
				"Verbose",
				"Debug",
				"Information"
			};
		}
	}

}
