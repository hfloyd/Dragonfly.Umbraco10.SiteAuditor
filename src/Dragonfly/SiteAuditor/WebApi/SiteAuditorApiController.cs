namespace Dragonfly.SiteAuditor.WebApi;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Net;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Extensions;

using Dragonfly.NetHelperServices;
using Dragonfly.NetModels;
using Dragonfly.SiteAuditor.Models;
using Dragonfly.SiteAuditor.Services;
using Microsoft.AspNetCore.Hosting;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

//  /umbraco/backoffice/Dragonfly/SiteAuditor/
[PluginController("Dragonfly")]
[IsBackOffice]
public class SiteAuditorController : UmbracoAuthorizedApiController
{
	#region ctor & DI

	private readonly ILogger<SiteAuditorController> _Logger;
	private readonly SiteAuditorService _SiteAuditorService;
	private readonly IViewRenderService _ViewRenderService;
	private readonly Dragonfly.NetHelperServices.FileHelperService _FileHelperService;
	private readonly ServiceContext _Services;
	private readonly ContentService _ContentService;
	//	private IWebHostEnvironment _HostingEnvironment;

	public SiteAuditorController(
		ILogger<SiteAuditorController> logger
		, SiteAuditorService siteAuditorService
		, IViewRenderService viewRenderService
		//	,IWebHostEnvironment hostingEnvironment
		, Dragonfly.NetHelperServices.FileHelperService fileHelperService
		, ServiceContext services
	)
	{
		_Logger = logger;
		_SiteAuditorService = siteAuditorService;
		_ViewRenderService = viewRenderService;
		_FileHelperService = fileHelperService;
		_Services = services;


		_ContentService = services.ContentService as ContentService;

		//	_HostingEnvironment = hostingEnvironment;
	}

	#endregion

	private string RazorFilesPath()
	{
		return _SiteAuditorService.PluginPath() + "RazorViews/";
	}


	private SiteAuditorService GetSiteAuditorService()
	{
		return _SiteAuditorService;
		//return new SiteAuditorService(Umbraco, UmbracoContext, Services, Logger<>);
	}

	internal StandardViewInfo GetStandardViewInfo()
	{
		var info = new StandardViewInfo();

		info.CurrentToolVersion = PackageInfo.Version;

		//TODO: Make configurable?
		info.ThumbnailWidth = 300;
		info.ThumbnailHeight = 300;

		info.SerilogDirectory = "/umbraco/Logs";

		return info;
	}


	#region Content Nodes

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllContentAsHtmlTable
	[HttpGet]
	public IActionResult GetAllContentAsHtmlTable(bool PublishedOnly=false, string Search = "")
	{
		//Setup
		var pvPath = RazorFilesPath() + "AllContentAsHtmlTable.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var contentNodes = saService.GetContentNodes(PublishedOnly);

		//VIEW DATA 
		var model = contentNodes;
		var viewData = new Dictionary<string, object>();
		var standardInfo = GetStandardViewInfo();
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("PublishedOnly", PublishedOnly);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);
	}

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetContentForDoctypeHtml?DocTypeAlias=X
	[HttpGet]
	public IActionResult GetContentForDoctypeHtml(string DocTypeAlias = "", bool PublishedOnly = false, string Search = "")
	{
		if (DocTypeAlias != "")
		{
			return ContentForDoctypeHtml(DocTypeAlias, PublishedOnly,Search);
		}
		else
		{
			return DoctypesForContentForDoctypeHtml();
		}
	}

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetContentForDoctypeHtml?DocTypeAlias=X
	[HttpGet]
	public IActionResult GetContentForElementHtml(string DocTypeAlias = "", bool PublishedOnly = false, string Search = "")
	{
		if (DocTypeAlias != "")
		{
			return ContentForElementHtml(DocTypeAlias, PublishedOnly, Search);
		}
		else
		{
			return DoctypesForContentForDoctypeHtml();
		}
	}

	private IActionResult DoctypesForContentForDoctypeHtml()
	{
		//Setup
		var pvPath = RazorFilesPath() + "DoctypesForContentList.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var allSiteDocTypes = saService.GetAllDocTypes().ToList();
		var allNodeTypes = allSiteDocTypes.Where(n => n.IsElement == false).OrderBy(n => n.Alias).ToList();
		var allElementTypes = allSiteDocTypes.Where(n => n.IsElement == true).OrderBy(n => n.Alias).ToList();

		var data = new DocTypesAndElements();
		data.AllNodeTypes = allNodeTypes;
		data.AllElementTypes = allElementTypes;

		//VIEW DATA 
		var model = data;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", GetStandardViewInfo());
		viewData.Add("Status", status);

		//BUILD HTML
		//var returnSB = new StringBuilder();
		//returnSB.AppendLine($"<h1>Get Content for a Selected Document Type</h1>");
		//returnSB.AppendLine($"<p id=\"Nav\"><a href=\"#ContentNodes\">Content Node Document Types</a> | <a href=\"#Elements\">Element Types</a></p>");


		//returnSB.AppendLine($"<h3 id=\"ContentNodes\">Available Content Node Document Types</h3>");
		//returnSB.AppendLine("<ul>");
		//foreach (var docType in allNodeTypes)
		//{
		//	var type = docType.IsElement ? "Nodes Using Element" : "Content Nodes of Type";
		//	var api = docType.IsElement ? "GetContentForElementHtml" : "GetContentForDoctypeHtml";
		//	var url = $"/umbraco/backoffice/Dragonfly/SiteAuditor/{api}?DocTypeAlias={docType.Alias}";

		//	returnSB.AppendLine($"<li>{docType.Alias} <a target=\"_blank\" href=\"{url}\">View</a></li>");
		//}
		//returnSB.AppendLine("</ul>");
		//returnSB.AppendLine($"<p><a href=\"#Nav\">Top Nav</a></p>");

		//returnSB.AppendLine($"<h3 id=\"Elements\">Available Element Document Types</h3>");
		//returnSB.AppendLine("<p>Note: These options will take longer to load because they have to recursively check the content property values.</p>");
		//returnSB.AppendLine("<ul>");
		//foreach (var docType in allElementTypes)
		//{
		//	var type = docType.IsElement ? "Nodes Using Element" : "Content Nodes of Type";
		//	var api = docType.IsElement ? "GetContentForElementHtml" : "GetContentForDoctypeHtml";
		//	var url = $"/umbraco/backoffice/Dragonfly/SiteAuditor/{api}?DocTypeAlias={docType.Alias}";

		//	returnSB.AppendLine($"<li>{docType.Alias} <a target=\"_blank\" href=\"{url}\">View</a></li>");
		//}
		//returnSB.AppendLine("</ul>");
		//returnSB.AppendLine($"<p><a href=\"#Nav\">Top Nav</a></p>");

		//var displayHtml = returnSB.ToString();

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		return new ContentResult()
		{
			Content = displayHtml,
			StatusCode = (int)HttpStatusCode.OK,
			ContentType = "text/html; charset=utf-8"
		};

	}

	private IActionResult ContentForDoctypeHtml(string DocTypeAlias, bool PublishedOnly, string Search = "")
	{
		//Setup
		var pvPath = RazorFilesPath() + "AllContentAsHtmlTable.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var contentNodes = saService.GetContentNodes(DocTypeAlias, PublishedOnly);

		//VIEW DATA 
		var model = contentNodes;
		var viewData = new Dictionary<string, object>();
		var standardInfo = GetStandardViewInfo();
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("DocTypeAlias", DocTypeAlias);
		viewData.Add("PublishedOnly", PublishedOnly);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		return new ContentResult()
		{
			Content = displayHtml,
			StatusCode = (int)HttpStatusCode.OK,
			ContentType = "text/html; charset=utf-8"
		};

	}

	private IActionResult ContentForElementHtml(string DocTypeAlias, bool PublishedOnly, string Search = "")
	{
		//Setup
		var pvPath = RazorFilesPath() + "AllElementContentAsHtmlTable.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var contentNodes = saService.GetContentNodesUsingElement(DocTypeAlias, PublishedOnly);

		//VIEW DATA 
		var model = contentNodes;
		var viewData = new Dictionary<string, object>();
		var standardInfo = GetStandardViewInfo();
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("DocTypeAlias", DocTypeAlias);
		viewData.Add("PublishedOnly", PublishedOnly);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		return new ContentResult()
		{
			Content = displayHtml,
			StatusCode = (int)HttpStatusCode.OK,
			ContentType = "text/html; charset=utf-8"
		};
	}

	#region Obsolete
	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllContentAsJson
	[HttpGet]
	public IActionResult GetAllContentAsJson()
	{
		var saService = GetSiteAuditorService();
		var allNodes = saService.GetContentNodes(false);
		var exportable = allNodes.ConvertToExportable();

		//Return JSON
		return new JsonResult(exportable);
	}

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllContentAsCsv
	[HttpGet]
	public IActionResult GetAllContentAsCsv()
	{
		var saService = GetSiteAuditorService();
		var returnSB = new StringBuilder();

		var allNodes = saService.GetContentNodes(false);

		var tableData = new StringBuilder();

		tableData.AppendLine(
			"\"Node Name\"" +
			",\"NodeID\"" +
			",\"Node Path\"" +
			",\"DocType\"" +
			",\"ParentID\"" +
			",\"Full URL\"" +
			",\"Level\"" +
			",\"Sort Order\"" +
			",\"Template Name\"" +
			",\"Udi\"" +
			",\"Create Date\"" +
			",\"Update Date\"");

		foreach (var auditNode in allNodes)
		{
			if (auditNode.UmbContentNode != null)
			{
				var nodeLine = $"\"{auditNode.UmbContentNode.Name}\"" +
							   $",{auditNode.UmbContentNode.Id}" +
							   $",\"{auditNode.NodePathAsCustomText(" > ")}\"" +
							   $",\"{auditNode.UmbContentNode.ContentType.Alias}\"" +
							   $",{auditNode.UmbContentNode.ParentId}" +
							   $",\"{auditNode.FullNiceUrl}\"" +
							   $",{auditNode.UmbContentNode.Level}" +
							   $",{auditNode.UmbContentNode.SortOrder}" +
							   $",\"{auditNode.TemplateAlias}\"" +
							   $",\"{auditNode.UmbContentNode.GetUdi()}\"" +
							   $",{auditNode.UmbContentNode.CreateDate}" +
							   $",{auditNode.UmbContentNode.UpdateDate}" +
							   $"{Environment.NewLine}";

				tableData.Append(nodeLine);
			}
		}

		returnSB.Append(tableData);


		//RETURN AS CSV FILE
		var fileName = "AllContentNodes.csv";
		var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
		var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
		return File(result, "text/csv", fileName);

		//var contentDispositionHeader = new System.Net.Mime.ContentDisposition()
		//{
		//	FileName = "AllContentNodes.csv",
		//	DispositionType = "attachment"
		//};
		//HttpContext.Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());
		//return Content(
		//	returnSB.ToString(),
		//	"text/csv",
		//	Encoding.UTF8
		//);

	}

	#endregion

	#endregion

	#region Media Nodes

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllMediaAsHtmlTable
	[HttpGet]
	public IActionResult GetAllMediaAsHtmlTable(bool ShowImageThumbnails = false, string Search = "")
	{
		//Setup
		var pvPath = RazorFilesPath() + "AllMediaAsHtmlTable.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var nodes = saService.GetMediaNodes();

		//VIEW DATA 
		var model = nodes;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", GetStandardViewInfo());
		viewData.Add("Status", status);
		viewData.Add("ShowImageThumbnails", ShowImageThumbnails);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);
	}

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetMediaForTypeHtml?MediaTypeAlias=X&ShowImageThumbnails=false
	[HttpGet]
	public IActionResult GetMediaForTypeHtml(string MediaTypeAlias = "", bool ShowImageThumbnails = false, string Search = "")
	{
		if (MediaTypeAlias != "")
		{
			return MediaForTypeHtml(MediaTypeAlias, ShowImageThumbnails, Search);
		}
		else
		{
			return TypesForMediaHtml();
		}
	}

	private IActionResult TypesForMediaHtml()
	{
		//Setup
		var pvPath = RazorFilesPath() + "MediaTypesForMediaList.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var data = saService.GetAllMediaTypes().OrderBy(n => n.Alias).ToList();

		//VIEW DATA 
		var model = data;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", GetStandardViewInfo());
		viewData.Add("Status", status);


		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		return new ContentResult()
		{
			Content = displayHtml,
			StatusCode = (int)HttpStatusCode.OK,
			ContentType = "text/html; charset=utf-8"
		};

	}

	private IActionResult MediaForTypeHtml(string MediaTypeAlias, bool ShowImageThumbnails = false, string Search = "")
	{
		//Setup
		var pvPath = RazorFilesPath() + "AllMediaAsHtmlTable.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var nodes = saService.GetMediaNodes(MediaTypeAlias);

		//VIEW DATA 
		var model = nodes;
		var viewData = new Dictionary<string, object>();
		var standardInfo = GetStandardViewInfo();
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("ShowImageThumbnails", ShowImageThumbnails);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		return new ContentResult()
		{
			Content = displayHtml,
			StatusCode = (int)HttpStatusCode.OK,
			ContentType = "text/html; charset=utf-8"
		};

	}


	#endregion

	#region GetContentWithValues

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetContentWithValues?PropertyAlias=xxx
	[HttpGet]
	public IActionResult GetContentWithValues(string PropertyAlias = "", bool PublishedOnly = false, string Search = "")
	{
		//GET DATA TO DISPLAY
		if (PropertyAlias == "")
		{
			return GetContentWithValuesList();
		}
		else
		{
			return GetContentWithValuesTable(PropertyAlias, PublishedOnly,Search);
		}
	}
	
	private IActionResult GetContentWithValuesTable(string PropertyAlias, bool PublishedOnly, string Search = "")
	{
		//Setup
		var pvPath = RazorFilesPath() + "ContentWithValuesTable.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var displayHtml = "";

		//Get matching Property data
		var contentNodes = saService.GetContentWithProperty(PropertyAlias, PublishedOnly);

		//VIEW DATA 
		var model = contentNodes;
		var viewData = new Dictionary<string, object>();
		var standardInfo = GetStandardViewInfo();
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("PropertyAlias", PropertyAlias);
		viewData.Add("PublishedOnly", PublishedOnly);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		displayHtml = htmlTask.Result;


		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);
	}

	private IActionResult GetContentWithValuesList()
	{
		//Setup
		var pvPath = RazorFilesPath() + "ContentWithValuesList.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var displayHtml = "";


		//Get list of properties
		//displayHtml = HtmlListOfProperties();
		var allPropsAliases = ListOfProperties();

		//VIEW DATA 
		var model = allPropsAliases;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", GetStandardViewInfo());
		viewData.Add("Status", status);
		//viewData.Add("PropertyAlias", PropertyAlias);
		// viewData.Add("IncludeUnpublished", IncludeUnpublished);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		displayHtml = htmlTask.Result;


		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);
	}


	/// <summary>
	/// Returns a list of all property aliases and their data types
	/// </summary>
	/// <returns>Key = Property Alias; Value=DataType object</returns>
	private IEnumerable<KeyValuePair<string, AuditableDataType>> ListOfProperties()
	{
		//Setup
		var saService = GetSiteAuditorService();

		//GET DATA
		var allSiteDocTypes = saService.GetAllDocTypes();
		var allProps = allSiteDocTypes.SelectMany(n => n.PropertyTypes).ToList();
		var allPropsAliases = allProps.Select(n => n.Alias).Distinct();
		var allPropsWithDataType = new List<KeyValuePair<string, AuditableDataType>>();

		foreach (var prop in allProps)
		{
			var dataType = saService.GetAuditableDataType(prop.DataTypeId);
			if (dataType != null)
			{
				allPropsWithDataType.Add(new KeyValuePair<string, AuditableDataType>(prop.Alias, dataType));
			}
		}

		var distinctList = allPropsWithDataType.Distinct().ToList();

		return distinctList;
	}

	//private string HtmlListOfProperties()
	//{
	//	//Setup
	//	//    var pvPath = RazorFilesPath() + "Start.cshtml";
	//	//var saService = GetSiteAuditorService();

	//	//GET DATA TO DISPLAY
	//	var status = new StatusMessage(true);

	//	//var allSiteDocTypes = saService.GetAllDocTypes();
	//	//var allProps = allSiteDocTypes.SelectMany(n => n.PropertyTypes);
	//	//var allPropsAliases = allProps.Select(n => n.Alias).Distinct();

	//	var allPropsAliases = ListOfProperties();

	//	//BUILD HTML
	//	var returnSB = new StringBuilder();
	//	returnSB.AppendLine($"<h1>Get Content with Values</h1>");
	//	returnSB.AppendLine($"<h3>Available Properties</h3>");
	//	//returnSB.AppendLine("<p>Note: Choosing the 'All' option will take significantly longer to load than the 'Published' option because we need to bypass the cache and query the database directly.</p>");

	//	returnSB.AppendLine("<ol>");
	//	foreach (var propAlias in allPropsAliases.OrderBy(n => n))
	//	{
	//		//var url1 =
	//		//    $"/umbraco/backoffice/Dragonfly/SiteAuditor/GetContentWithValues?PropertyAlias={propAlias}&IncludeUnpublished=false";
	//		var url2 = $"/umbraco/backoffice/Dragonfly/SiteAuditor/GetContentWithValues?PropertyAlias={propAlias}";

	//		returnSB.AppendLine($"<li>{propAlias} <a target=\"_blank\" href=\"{url2}\">View</a></li>");
	//	}
	//	returnSB.AppendLine("</ol>");

	//	return returnSB.ToString();
	//}

	#endregion

	#region GetMediaWithValues

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetMediaWithValues?PropertyAlias=xxx
	[HttpGet]
	public IActionResult GetMediaWithValues(string PropertyAlias = "", string Search = "")
	{
		//GET DATA TO DISPLAY
		if (PropertyAlias == "")
		{
			return GetMediaWithValuesList();
		}
		else
		{
			return GetMediaWithValuesTable(PropertyAlias,Search);
		}
	}

	private IActionResult GetMediaWithValuesTable(string PropertyAlias, string Search = "")
	{
		//Setup
		var pvPath = RazorFilesPath() + "MediaWithValuesTable.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var displayHtml = "";

		//Get matching Property data
		var nodes = saService.GetMediaWithProperty(PropertyAlias);

		//VIEW DATA 
		var model = nodes;
		var viewData = new Dictionary<string, object>();
		var standardInfo = GetStandardViewInfo();
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("PropertyAlias", PropertyAlias);
		// viewData.Add("IncludeUnpublished", IncludeUnpublished);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		displayHtml = htmlTask.Result;


		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);
	}

	private IActionResult GetMediaWithValuesList()
	{
		//Setup
		var pvPath = RazorFilesPath() + "MediaWithValuesList.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var displayHtml = "";


		//Get list of properties
		//displayHtml = HtmlListOfProperties();
		var allPropsAliases = ListOfMediaProperties();

		//VIEW DATA 
		var model = allPropsAliases;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", GetStandardViewInfo());
		viewData.Add("Status", status);
		//viewData.Add("PropertyAlias", PropertyAlias);
		// viewData.Add("IncludeUnpublished", IncludeUnpublished);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		displayHtml = htmlTask.Result;


		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);
	}

	private IList<string> ListOfMediaProperties()
	{
		//Setup
		var saService = GetSiteAuditorService();

		//GET DATA
		var allTypes = saService.GetAllMediaTypes();
		var allProps = allTypes.SelectMany(n => n.PropertyTypes);
		var allPropsAliases = allProps.Select(n => n.Alias).Distinct();

		return allPropsAliases.ToList();
	}


	#endregion

	#region Properties Info

	// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllPropertiesAsHtml
	[HttpGet]
	public IActionResult GetAllPropertiesAsHtmlTable( string Search = "")
	{
		//Setup
		var pvPath = RazorFilesPath() + "AllPropertiesAsHtmlTable.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);

		var siteProps = saService.AllProperties();
		var propertiesList = siteProps.AllProperties;

		//VIEW DATA 
		var model = propertiesList;
		var viewData = new Dictionary<string, object>();
		var standardInfo = GetStandardViewInfo();
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("DocTypeAlias", "");

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);
	}

	// /umbraco/backoffice/Dragonfly/SiteAuditor/GetPropertiesForDoctypeHtml?DocTypeAlias=xxx
	[HttpGet]
	public IActionResult GetPropertiesForDoctypeHtml(string DocTypeAlias = "", string Search = "")
	{
		if (DocTypeAlias != "")
		{
			return PropsForDoctypeHtml(DocTypeAlias,Search);
		}
		else
		{
			return DoctypesForPropertiesForDoctypeHtml();
		}
	}

	private IActionResult PropsForDoctypeHtml(string DocTypeAlias, string Search = "")
	{
		//Setup
		var pvPath = RazorFilesPath() + "AllPropertiesAsHtmlTable.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);

		var siteProps = saService.AllPropertiesForDocType(DocTypeAlias);
		var propertiesList = siteProps.AllProperties;


		//VIEW DATA 
		var model = propertiesList;
		var viewData = new Dictionary<string, object>();
		var standardInfo = GetStandardViewInfo();
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("Title", $"Properties for Document Type '{DocTypeAlias}'");
		viewData.Add("DocTypeAlias", DocTypeAlias);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);

	}

	private IActionResult DoctypesForPropertiesForDoctypeHtml()
	{
		//Setup
		var pvPath = RazorFilesPath() + "DoctypesForPropertiesForDoctypeList.cshtml";

		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);

		var allSiteDocTypes = saService.GetAllDocTypes();
		var allAliases = allSiteDocTypes.Select(n => n.Alias).OrderBy(n => n).ToList();

		//VIEW DATA 
		var model = allAliases;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", GetStandardViewInfo());
		viewData.Add("Status", status);


		//BUILD HTML
		//var returnSB = new StringBuilder();

		//returnSB.AppendLine($"<h1>Get Properties for a Selected Document Type</h1>");
		//returnSB.AppendLine($"<h3>Available Document Types</h3>");
		//returnSB.AppendLine("<p>Note: Choosing the 'All' option will take significantly longer to load than the 'Published' option because we need to bypass the cache and query the database directly.</p>");

		//returnSB.AppendLine("<ul>");
		//foreach (var alias in allAliases)
		//{
		//	var url = $"/umbraco/backoffice/Dragonfly/SiteAuditor/GetPropertiesForDoctypeHtml?DocTypeAlias={alias}";

		//	returnSB.AppendLine($"<li>{alias} <a target=\"_blank\" href=\"{url}\">View</a></li>");
		//}
		//returnSB.AppendLine("</ul>");
		//var displayHtml = returnSB.ToString();


		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);
	}

	#region Obsolete?

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllPropertiesAsJson
	[HttpGet]
	public IActionResult GetAllPropertiesAsJson()
	{
		var saService = GetSiteAuditorService();

		var siteProps = saService.AllProperties();
		var propertiesList = siteProps.AllProperties;

		//Return JSON
		return new JsonResult(propertiesList);
	}

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllPropertiesAsCsv
	[HttpGet]
	public IActionResult GetAllPropertiesAsCsv()
	{
		var saService = GetSiteAuditorService();
		var returnSB = new StringBuilder();

		var siteProps = saService.AllProperties();
		var propertiesList = siteProps.AllProperties;

		var tableData = new StringBuilder();

		tableData.AppendLine(
			"\"Property Name\",\"Property Alias\",\"DataType Name\",\"DataType Property Editor\",\"DataType Database Type\",\"DocumentTypes Used In\",\"Qty of DocumentTypes\"");

		foreach (var prop in propertiesList)
		{
			if (prop.UmbPropertyType != null && prop.DataType != null)
			{
				tableData.AppendFormat(
					"\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",{6}{7}",
					prop.UmbPropertyType.Name,
					prop.UmbPropertyType.Alias,
					prop.DataType.Name,
					prop.DataType.EditorAlias,
					prop.DataType.DatabaseType,
					string.Join(", ", prop.AllDocTypes.Select(n => n.DocTypeAlias)),
					prop.AllDocTypes.Count(),
					Environment.NewLine);
			}
		}

		returnSB.Append(tableData);


		//RETURN AS CSV FILE
		var fileName = "AllProperties.csv";
		var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
		var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
		return File(result, "text/csv", fileName);

		//var contentDispositionHeader = new System.Net.Mime.ContentDisposition()
		//{
		//	FileName = "AllProperties.csv",
		//	DispositionType = "attachment"
		//};
		//HttpContext.Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());
		//return new ContentResult()
		//{
		//	Content = returnSB.ToString(),
		//	StatusCode = (int)HttpStatusCode.OK,
		//	ContentType = "text/csv; charset=utf-8",
		//};

	}


	#endregion

	#endregion

	#region DataType Info
	
	// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDataTypesAsHtmlTable
	[HttpGet]
	public IActionResult GetAllDataTypesAsHtmlTable(string Search = "")
	{
		//Setup
		var pvPath = RazorFilesPath() + "AllDataTypesAsHtmlTable.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);

		var dataTypes = saService.AllDataTypes();

		//VIEW DATA 
		var model = dataTypes;
		var viewData = new Dictionary<string, object>();
		var standardInfo = GetStandardViewInfo();
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		return new ContentResult()
		{
			Content = displayHtml,
			StatusCode = (int)HttpStatusCode.OK,
			ContentType = "text/html; charset=utf-8"
		};
	}

	#region Obsolete?

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDataTypesAsJson
	[HttpGet]
	public IActionResult GetAllDataTypesAsJson()
	{
		var saService = GetSiteAuditorService();

		var dataTypes = saService.AllDataTypes();

		//Return JSON
		return new JsonResult(dataTypes);
	}

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDataTypesAsCsv
	[HttpGet]
	public IActionResult GetAllDataTypesAsCsv()
	{
		var saService = GetSiteAuditorService();
		var returnSB = new StringBuilder();

		var dataTypes = saService.AllDataTypes();

		var tableData = new StringBuilder();

		tableData.AppendLine(
			"\"DataType Name\",\"Property Editor Alias\",\"Id\",\"Guid Key\",\"Used On Properties\",\"Qty of Properties\"");

		foreach (var dt in dataTypes)
		{
			tableData.AppendFormat(
				"\"{0}\",\"{1}\",{2},\"{3}\",\"{4}\",{5}{6}",
				dt.Name,
				dt.EditorAlias,
				dt.Id,
				dt.Guid.ToString(),
				string.Join(" | ", dt.UsedOnProperties.Select(n => $"{n.Value} [{n.Key.Alias}]")),
				dt.UsedOnProperties.Count(),
				Environment.NewLine);
		}

		returnSB.Append(tableData);

		//RETURN AS CSV FILE
		var fileName = "AllDataTypes.csv";
		var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
		var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
		return File(result, "text/csv", fileName);

		//var contentDispositionHeader = new System.Net.Mime.ContentDisposition()
		//{
		//	FileName = "AllDataTypes.csv",
		//	DispositionType = "attachment"
		//};
		//HttpContext.Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());
		//return new ContentResult()
		//{
		//	Content = returnSB.ToString(),
		//	StatusCode = (int)HttpStatusCode.OK,
		//	ContentType = "text/csv; charset=utf-8",
		//};

	}

	#endregion

	#endregion

	#region DocTypes Info

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDocTypesAsHtmlTable
	[HttpGet]
	public IActionResult GetAllDocTypesAsHtmlTable(string Search = "")
	{
		//Setup
		var pvPath = RazorFilesPath() + "AllDocTypesAsHtmlTable.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var allDts = saService.GetAuditableDocTypes();

		//VIEW DATA 
		var model = allDts;
		var viewData = new Dictionary<string, object>();
		var standardInfo = GetStandardViewInfo();
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);
	}

	#region Obsolete?

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDocTypesAsJson
	[HttpGet]
	public IActionResult GetAllDocTypesAsJson()
	{
		var saService = GetSiteAuditorService();
		var allDts = saService.GetAuditableDocTypes();

		//Return JSON
		return new JsonResult(allDts);
	}

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllDocTypesAsCsv
	[HttpGet]
	public IActionResult GetAllDocTypesAsCsv()
	{
		var saService = GetSiteAuditorService();
		var returnSB = new StringBuilder();

		var allDts = saService.GetAuditableDocTypes();

		var tableData = new StringBuilder();

		tableData.AppendLine(
			"\"Doctype Name\",\"Alias\",\"Default Template\",\"GUID\",\"Create Date\",\"Update Date\"");

		foreach (var item in allDts)
		{
			tableData.AppendFormat(
				"\"{0}\",\"{1}\",\"{2}\",\"{3}\",{4},{5}{6}",
				item.Name,
				item.Alias,
				item.DefaultTemplateName,
				item.Guid,
				item.ContentType != null ? item.ContentType.CreateDate : DateTime.MinValue,
				item.ContentType != null ? item.ContentType.UpdateDate : DateTime.MinValue,
				Environment.NewLine);
		}
		returnSB.Append(tableData);

		//RETURN AS CSV FILE
		var fileName = "AllDoctypes.csv";
		var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
		var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
		return File(result, "text/csv", fileName);

		//var contentDispositionHeader = new System.Net.Mime.ContentDisposition()
		//{
		//	FileName = "AllDoctypes.csv",
		//	DispositionType = "attachment"
		//};
		//HttpContext.Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());
		//return new ContentResult()
		//{
		//	Content = returnSB.ToString(),
		//	StatusCode = (int)HttpStatusCode.OK,
		//	ContentType = "text/csv; charset=utf-8",
		//};

	}


	#endregion

	#endregion

	#region Element Types Info

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetContentForElementType?ElementTypeAlias=xxx
	[HttpGet]
	public IActionResult GetContentForElementType(string ElementTypeAlias = "", bool PublishedOnly = false, string Search="")
	{
		//GET DATA TO DISPLAY
		if (ElementTypeAlias == "")
		{
			return GetContentForElementTypeList();
		}
		else
		{
			return GetContentForElementTypeTable(ElementTypeAlias, PublishedOnly, Search);
		}
	}

	private IActionResult GetContentForElementTypeList()
	{
		//Setup
		var pvPath = RazorFilesPath() + "ContentForElementTypeList.cshtml";
		//var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var displayHtml = "";


		//Get list of properties
		//displayHtml = HtmlListOfProperties();
		var allElementTypeAliases = ListOfElementTypes();

		//VIEW DATA 
		var model = allElementTypeAliases;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", GetStandardViewInfo());
		viewData.Add("Status", status);
		//viewData.Add("PropertyAlias", PropertyAlias);
		// viewData.Add("IncludeUnpublished", IncludeUnpublished);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		displayHtml = htmlTask.Result;


		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);
	}

	private IActionResult GetContentForElementTypeTable(string ElementTypeAlias, bool PublishedOnly, string Search)
	{
		//Setup
		var pvPath = RazorFilesPath() + "ContentForElementTypeTable.cshtml";
		var saService = _SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var displayHtml = "";

		var elementType = saService.GetAuditableDocTypeByAlias(ElementTypeAlias);
		if (elementType is null)
		{
			displayHtml = $"{ElementTypeAlias} is not a valid Umbraco ContentType";
		}
		else
		{
			//Get matching data
			var elementGuid = elementType.Guid;

			var allDatatypes = saService.AllDataTypes();
			var elementDataTypes = allDatatypes.Where(n => n.UsesElementsAll.Contains(ElementTypeAlias));

			var propertiesUsingDataTypes = new List<AuditableProperty>();
			foreach (var dataType in elementDataTypes)
			{
				propertiesUsingDataTypes.AddRange(saService.AllPropertiesForDataType(dataType));
			}

			var contentNodesWithProps = new List<KeyValuePair<AuditableProperty, AuditableContent>>();
			foreach (AuditableProperty property in propertiesUsingDataTypes)
			{
				var propAlias = property.UmbPropertyType != null ? property.UmbPropertyType.Alias : "";
				var nodes = saService.GetContentWithProperty(propAlias, PublishedOnly);

				//Nodes with the Property present
				foreach (var node in nodes)
				{
					var iContent = node.UmbContentNode ?? _ContentService.GetById(node.NodeId);

					if (iContent != null)
					{
						var canGetValue = iContent.Properties.TryGetValue(propAlias, out var iProp);

						if (canGetValue)
						{
							foreach (var value in iProp.Values)
							{
								var strValue = value.PublishedValue != null ? value.PublishedValue.ToString() : "";
								var hasElementInContent = strValue.Contains(elementGuid.ToString());

								if(hasElementInContent)
								{
									contentNodesWithProps.Add(
										new KeyValuePair<AuditableProperty, AuditableContent>(property, node));
								}
							}
						}
					}
				}


			}

			//VIEW DATA 
			var model = contentNodesWithProps;
			var viewData = new Dictionary<string, object>();
			var standardInfo = GetStandardViewInfo();
			standardInfo.DefaultSearchString = Search;
			viewData.Add("StandardInfo", standardInfo);
			viewData.Add("Status", status);
			viewData.Add("ElementTypeAlias", ElementTypeAlias);
			viewData.Add("ElementTypeGuid", elementGuid);
			viewData.Add("PublishedOnly", PublishedOnly);

			//RENDER
			var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
			displayHtml = htmlTask.Result;
		}

		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);
	}

	private IList<AuditableDocType> ListOfElementTypes()
	{
		//Setup
		var saService = _SiteAuditorService;
		var allElementTypes = new List<AuditableDocType>();

		//GET DATA
		var allSiteDocTypes = saService.GetAllDocTypes();
		var allElementContentTypes = allSiteDocTypes.Where(n => n.IsElement).ToList();

		foreach (var ect in allElementContentTypes)
		{
			var elementType = saService.GetAuditableDocType(ect);
			if (elementType != null)
			{
				allElementTypes.Add(elementType);
			}
		}

		var distinctList = allElementTypes.Distinct().ToList();

		return distinctList;
	}



	#endregion

	#region Templates Info

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllTemplatesAsHtmlTable
	[HttpGet]
	public IActionResult GetAllTemplatesAsHtmlTable(string Search = "")
	{
		//Setup
		var pvPath = RazorFilesPath() + "AllTemplatesAsHtmlTable.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var allTemps = saService.GetAuditableTemplates();

		//VIEW DATA 
		var model = allTemps;
		var viewData = new Dictionary<string, object>();
		var standardInfo = GetStandardViewInfo();
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);
	}

	#region Obsolete?

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllTemplatesAsJson
	[HttpGet]
	public IActionResult GetAllTemplatesAsJson()
	{
		var saService = GetSiteAuditorService();
		var allTemps = saService.GetAuditableTemplates();

		//Return JSON
		return new JsonResult(allTemps);
	}



	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetAllTemplatesAsCsv
	[HttpGet]
	public IActionResult GetAllTemplatesAsCsv()
	{
		var saService = GetSiteAuditorService();
		var returnSB = new StringBuilder();

		var allTemplates = saService.GetAuditableTemplates();

		var tableData = new StringBuilder();

		tableData.AppendLine(
			"\"Template Name\",\"Alias\",\"Code Length\",\"GUID\",\"Create Date\",\"Update Date\"");

		foreach (var item in allTemplates)
		{
			tableData.AppendFormat(
				"\"{0}\",\"{1}\",\"{2}\",\"{3}\",{4},{5}{6}",
				item.Name,
				item.Alias,
				item.CodeLength,
				item.Guid,
				item.CreateDate,
				item.UpdateDate,
				Environment.NewLine);
		}
		returnSB.Append(tableData);

		//RETURN AS CSV FILE
		var fileName = "AllTemplates.csv";
		var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
		var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
		return File(result, "text/csv", fileName);

	}
	#endregion

	#endregion

	#region Special Queries
	/// /umbraco/backoffice/Dragonfly/SiteAuditor/TemplateUsageReport
	[HttpGet]
	public IActionResult TemplateUsageReport()
	{
		//Setup
		var pvPath = RazorFilesPath() + "TemplateUsageReport.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);

		//VIEW DATA 
		var model = status;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", GetStandardViewInfo());
		viewData.Add("Status", status);
		viewData.Add("TemplatesUsedOnContent", saService.TemplatesUsedOnContent());
		viewData.Add("TemplatesNotUsedOnContent", saService.TemplatesNotUsedOnContent());

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);
	}
	#endregion

	#region Logs

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetLogs
	[HttpGet]
	public IActionResult GetLogs(DateTime? StartDate = null, DateTime? EndDate = null, int BatchBy = 0, [FromQuery] string PromotedProperties = "")
	{
		//GET DATA TO DISPLAY
		if (StartDate is null)
		{
			return GetLogDateOptions(BatchBy);
		}
		else
		{
			var start = (DateTime)StartDate;
			return GetLogsAsHtmlTable(start, EndDate, PromotedProperties);
		}
	}


	private IActionResult GetLogDateOptions(int BatchBy)
	{
		//Setup
		var pvPath = RazorFilesPath() + "LogOptions.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var displayHtml = "";


		//Get list of properties
		//displayHtml = HtmlListOfProperties();
		var dateOptions = DateOptions();

		//VIEW DATA 
		var model = dateOptions;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", GetStandardViewInfo());
		viewData.Add("Status", status);
		viewData.Add("BatchBy", BatchBy);
		// viewData.Add("IncludeUnpublished", IncludeUnpublished);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		displayHtml = htmlTask.Result;


		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);
	}


	/// /umbraco/backoffice/Dragonfly/SiteAuditor/GetLogsAsHtmlTable?StartDate=xxx&EndDate=xxx
	[HttpGet]
	public IActionResult GetLogsAsHtmlTable(DateTime StartDate, DateTime? EndDate = null, string PromotedProperties = "")
	{
		//Setup
		var pvPath = RazorFilesPath() + "LogsAsHtmlTable.cshtml";
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);

		var end = EndDate != null ? (DateTime)EndDate : DateTime.Today.AddDays(1);
		var mappedPath = GetSerilogMappedDirectory();

		IList<SerilogItem> logs = new List<SerilogItem>();
		if (mappedPath != "")
		{
			logs = saService.GetLogsBetweenDates(mappedPath, StartDate, end, out status);
		}
		else
		{
			status.Success = false;
			status.Message = "Unable to access the Serilog directory path. Please check your configuration.";
		}

		var matchingDateOption = GetMatchingDatesOption(DateOptions(), StartDate, end);

		//VIEW DATA 
		var model = logs;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", GetStandardViewInfo());
		viewData.Add("Status", status);
		viewData.Add("AllDateOptions", DateOptions());
		viewData.Add("ThisDateOption", matchingDateOption);
		viewData.Add("PromotedProperties", PromotedProperties);

		//RENDER
		var htmlTask = _ViewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		var result = new HttpResponseMessage()
		{
			Content = new StringContent(
				displayHtml,
				Encoding.UTF8,
				"text/html"
			)
		};

		return new HttpResponseMessageResult(result);
	}

	DatesOption GetMatchingDatesOption(IList<DatesOption> AllDatesOptions, DateTime Start, DateTime End)
	{
		var match = AllDatesOptions.Where(n => n.StartDate == Start && n.EndDate == End);
		if (match.Any())
		{
			return match.First();
		}
		else
		{
			return new DatesOption(Start, End, "Custom Dates");
		}
	}

	private IList<DatesOption> DateOptions()
	{
		var dateOptions = new List<DatesOption>();
		dateOptions.Add(new DatesOption()
		{
			Description = "Today",
			StartDate = DateTime.Today,
			EndDate = DateTime.Today.AddDays(1)
		});
		dateOptions.Add(new DatesOption()
		{
			Description = "Yesterday",
			StartDate = DateTime.Today.AddDays(-1),
			EndDate = DateTime.Today
		});
		dateOptions.Add(new DatesOption()
		{
			Description = "Last 7 Days",
			StartDate = DateTime.Today.AddDays(-7),
			EndDate = DateTime.Today
		});
		dateOptions.Add(new DatesOption()
		{
			Description = "Last 30 Days",
			StartDate = DateTime.Today.AddDays(-30),
			EndDate = DateTime.Today
		});
		dateOptions.Add(new DatesOption()
		{
			Description = "Last 90 Days",
			StartDate = DateTime.Today.AddDays(-90),
			EndDate = DateTime.Today
		});

		dateOptions.Add(new DatesOption()
		{
			Description = "This Month",
			StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
			EndDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)).AddDays(1)
		});

		dateOptions.Add(new DatesOption()
		{
			Description = "Last Month",
			StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1),
			EndDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddDays(-1).AddDays(1)
		});



		return dateOptions;
	}

	private string GetSerilogMappedDirectory()
	{
		var mappedPath = "";
		var canMapPath = _FileHelperService.TryGetMappedPath(GetStandardViewInfo().SerilogDirectory, out mappedPath, true);

		return mappedPath;
	}

	#endregion


	#region Tests & Examples

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/TestJson?DocTypeAlias=xxx
	[HttpGet]
	public IActionResult TestJson(string DocTypeAlias)
	{
		//Setup
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		try
		{
			var contentNodes = saService.GetContentNodesUsingElement(DocTypeAlias, true);
			status.RelatedObject = contentNodes;
		}
		catch (Exception e)
		{
			status.Success = false;
			status.SetRelatedException(e);
		}


		//Return JSON
		return new JsonResult(status);
	}

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/TestData?DocTypeAlias=xxx
	[HttpGet]
	public List<KeyValuePair<AuditableContent, NodePropertyDataTypeInfo>> TestData(string DocTypeAlias)
	{
		_Logger.LogInformation($"SiteAuditor.TestData STARTING...");

		//Setup
		var saService = GetSiteAuditorService();

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);

		var contentNodes = saService.GetContentNodesUsingElement(DocTypeAlias, true);
		status.RelatedObject = contentNodes;

		_Logger.LogInformation($"SiteAuditor.TestData COMPLETED");
		return contentNodes;

	}



	/// /umbraco/backoffice/Dragonfly/AuthorizedApi/Test
	[HttpGet]
	public bool Test()
	{
		//LogHelper.Info<PublicApiController>("Test STARTED/ENDED");
		return true;
	}

	/// /umbraco/backoffice/Dragonfly/AuthorizedApi/ExampleReturnHtml
	[HttpGet]
	public IActionResult ExampleReturnHtml()
	{
		//Setup
		// var pvPath = RazorFilesPath() + "Start.cshtml";

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);

		//BUILD HTML
		var returnSB = new StringBuilder();
		returnSB.AppendLine("<h1>Hello! This is HTML</h1>");
		returnSB.AppendLine("<p>Use this type of return when you want to exclude &lt;XML&gt;&lt;/XML&gt; tags from your output and don\'t want it to be encoded automatically.</p>");
		var displayHtml = returnSB.ToString();

		////VIEW DATA 
		//var model = XXX;
		// var viewData = new Dictionary<string, object>();
		// viewData.Add("StandardInfo", GetStandardViewInfo());
		// viewData.Add("Status", status);

		// //RENDER
		// var htmlTask = _viewRenderService.RenderToStringAsync(this.HttpContext, pvPath, model, viewData);
		// var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		return new ContentResult()
		{
			Content = displayHtml,
			StatusCode = (int)HttpStatusCode.OK,
			ContentType = "text/html; charset=utf-8"
		};

	}

	/// /umbraco/backoffice/Dragonfly/AuthorizedApi/ExampleReturnJson
	[HttpGet]
	public IActionResult ExampleReturnJson()
	{
		//var testData1 = new TimeSpan(1, 1, 1, 1);
		var testData2 = new StatusMessage(true, "This is a test object so you can see JSON!");

		//Return JSON
		return new JsonResult(testData2);
	}

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/ExampleReturnCsv
	[HttpGet]
	public IActionResult ExampleReturnCsv()
	{
		var returnSB = new StringBuilder();
		var tableData = new StringBuilder();

		for (int i = 0; i < 10; i++)
		{
			tableData.AppendFormat(
				"\"{0}\",{1},\"{2}\",{3}{4}",
				"Name " + i,
				i,
				string.Format("Some text about item #{0} for demo.", i),
				DateTime.Now,
				Environment.NewLine);
		}
		returnSB.Append(tableData);

		//RETURN AS CSV FILE
		var fileName = "Example.csv";
		var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
		var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
		return File(result, "text/csv", fileName);

	}

	/// /umbraco/backoffice/Dragonfly/SiteAuditor/TestCSV
	[HttpGet]
	public IActionResult TestCsv()
	{
		var returnSB = new StringBuilder();

		var tableData = new StringBuilder();

		for (int i = 0; i < 10; i++)
		{
			tableData.AppendFormat(
				"\"{0}\",{1},\"{2}\",{3}{4}",
				"Name " + i,
				i,
				string.Format("Some text about item #{0} for demo.", i),
				DateTime.Now,
				Environment.NewLine);
		}
		returnSB.Append(tableData);


		//RETURN AS CSV FILE
		var fileName = "Test.csv";
		var fileContent = Encoding.UTF8.GetBytes(returnSB.ToString());
		var result = Encoding.UTF8.GetPreamble().Concat(fileContent).ToArray();
		return File(result, "text/csv", fileName);

	}

	#endregion
}

