# Dragonfly Umbraco 10 SiteAuditor #

A collection of tools to extract data about an Umbraco 10 site created by [Heather Floyd](https://www.HeatherFloyd.com).


[![Dragonfly Website](https://img.shields.io/badge/Dragonfly-Website-A84492)](https://DragonflyLibraries.com/umbraco-packages/site-auditor/) [![Umbraco Marketplace](https://img.shields.io/badge/Umbraco-Marketplace-3544B1?logo=Umbraco&logoColor=white)](https://marketplace.umbraco.com/package/Dragonfly.Umbraco10.SiteAuditor) [![Nuget Downloads](https://buildstats.info/nuget/Dragonfly.Umbraco10.SiteAuditor)](https://www.nuget.org/packages/Dragonfly.Umbraco10.SiteAuditor/) [![GitHub](https://img.shields.io/badge/GitHub-Sourcecode-blue?logo=github)](https://github.com/hfloyd/Dragonfly.Umbraco10.SiteAuditor)


## Versions ##
This package is designed to work with Umbraco 10+. [View all available versions](https://DragonflyLibraries.com/umbraco-packages/site-auditor/#Versions).

## Installation ##

[![Nuget Downloads](https://buildstats.info/nuget/Dragonfly.Umbraco10.SiteAuditor)](https://www.nuget.org/packages/Dragonfly.Umbraco10.SiteAuditor/)


```
PM>   Install-Package Dragonfly.Umbraco10.SiteAuditor


dotnet add package Dragonfly.Umbraco10.SiteAuditor

```

## Configuration ##

Configuration is optional. The default values will work for most use cases. You can, however, control certain aspects of the tool via an appSettings section:

````json
{
  "Umbraco": {
    //...  Umbraco settings
  },
  "DragonflySiteAuditor": {
    "LimitProcessingLogsLargerThanBytes": 314572800, // 300 MB
    "ExcludeLevelsToManageLargeLogs": [ "Verbose", "Debug", "Information" ],
    "NeverProcessLogsLargerThanBytes": 1048576000, //1 GB
    "PluginPath": "~/App_Plugins/Dragonfly.SiteAuditor/",
    "DataPath": "~/App_Data/DragonflySiteAuditor/"
  }
}
```

the first three settings control the size of the logs that are processed by the tools. In cases where log files have become too large, trying to read them can cause Timeouts or 'Out Of Memory' exceptions. Log files which exceed the `LimitProcessingLogsLargerThanBytes` will be processed, but will exclude the levels specified in `ExcludeLevelsToManageLargeLogs`  If a log file exceeds the `NeverProcessLogsLargerThanBytes`, it will not be processed at all.

## Usage ##
The Tools are accessed via the Dashboard added to the back-office Settings section.


*NOTE: You need to be logged-in to the Umbraco back-office in order to access the tools.

You can edit some of the tools' output via the Views installed in /App_Plugins/Dragonfly.SiteAuditor
