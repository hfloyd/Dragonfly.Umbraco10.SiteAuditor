# Dragonfly Umbraco 10 SiteAuditor

A collection of tools to extract data about an Umbraco site created by [Heather Floyd](https://www.HeatherFloyd.com).

[![Dragonfly Website](https://img.shields.io/badge/Dragonfly-Website-A84492)](https://DragonflyLibraries.com/umbraco-packages/site-auditor/) [![Umbraco Marketplace](https://img.shields.io/badge/Umbraco-Marketplace-3544B1?logo=Umbraco&logoColor=white)](https://marketplace.umbraco.com/package/Dragonfly.Umbraco10.SiteAuditor) [![Nuget Downloads](https://img.shields.io/nuget/dt/Dragonfly.Umbraco10.SiteAuditor?color=FF5F49)](https://www.nuget.org/packages/Dragonfly.Umbraco10.SiteAuditor/) [![NuGet](https://img.shields.io/nuget/vpre/Dragonfly.Umbraco10.SiteAuditor)](https://www.nuget.org/packages/Dragonfly.Umbraco10.SiteAuditor) [![GitHub license](https://img.shields.io/github/license/hfloyd/Dragonfly.Umbraco10.SiteAuditor)](https://github.com/hfloyd/Dragonfly.Umbraco10.SiteAuditor/blob/main/LICENSE) [![GitHub Repo](https://img.shields.io/badge/GitHub-Code-yellow?logo=github)](https://github.com/hfloyd/Dragonfly.Umbraco10.SiteAuditor)

## Versions

Install the correct package/version for your Umbraco installation.

| Umbraco Version | Package / Version                                                                        |
| --------------- | ---------------------------------------------------------------------------------------- |
| v 17            | This package - v. 17.x                                                                   |
| v 16            | not supported                                                                            |
| v 15            | not supported                                                                            |
| v 14            | not supported                                                                            |
| v 13            | This package - v. 2.x                                                                    |
| v 12            | untested - try v. 2.x                                                                    |
| v 11            | untested - try v. 2.x                                                                    |
| v 10            | This package - v. 2.x                                                                    |
| v 9             | not supported                                                                            |
| v 8             | [Dragonfly.Umbraco8SiteAuditor](https://github.com/hfloyd/Dragonfly.Umbraco8SiteAuditor) |
| v 7             | [Dragonfly.SiteAuditor](https://github.com/hfloyd/Dragonfly.SiteAuditor)                 |

## Installation

[![Nuget Downloads](https://img.shields.io/nuget/vpre/Dragonfly.Umbraco10.SiteAuditor)](https://www.nuget.org/packages/Dragonfly.Umbraco10.SiteAuditor/)

```
PM>   Install-Package Dragonfly.Umbraco10.SiteAuditor


dotnet add package Dragonfly.Umbraco10.SiteAuditor
```

## Configuration

Configuration is optional. The default values will work for most use cases. You can, however, control certain aspects of the tool via an appSettings section:

```json
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

## Usage

The Tools are accessed via the Dashboard added to the back-office Settings section.

*NOTE: You need to be logged-in to the Umbraco back-office in order to access the tools.

You can edit some of the tools' output via the Views installed in /App_Plugins/Dragonfly.SiteAuditor

```

```
