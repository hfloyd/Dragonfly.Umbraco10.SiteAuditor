namespace Dragonfly.SiteAuditor.Services;

using Microsoft.AspNetCore.Http;
using Dragonfly.NetHelperServices;
using Dragonfly.NetModels;
using Dragonfly.SiteAuditor.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Services;

public class SiteAuditorApiContentService(
	ILogger<SiteAuditorController> Logger,
	SiteAuditorService SiteAuditorService,
	IViewRenderService ViewRenderService,
	FileHelperService FileHelperService,
	ServiceContext Services)
{

	#region CTOR & DI

	private readonly ILogger<SiteAuditorController> _Logger = Logger;
	private readonly ServiceContext _Services = Services;
	private readonly ContentService _ContentService = (Services.ContentService as ContentService)!;
	//	private IWebHostEnvironment _HostingEnvironment;

	private readonly string _RazorFilesPath = SiteAuditorApiConfig.RazorFilesPath(SiteAuditorService);
	private readonly StandardViewInfo _StandardViewInfo = SiteAuditorApiConfig.GetStandardViewInfo();

	#endregion

	#region Content Nodes
	internal string GetAllContentAsHtmlTable(HttpContext HttpContext, bool PublishedOnly = false, string Search = "")
	{
		//Setup
		var pvPath = _RazorFilesPath + "AllContentAsHtmlTable.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var contentNodes = saService.GetContentNodes(PublishedOnly);

		//VIEW DATA 
		var model = contentNodes;
		var viewData = new Dictionary<string, object>();
		var standardInfo = _StandardViewInfo;
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("PublishedOnly", PublishedOnly);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}

	internal string GetContentForDoctypeHtml(HttpContext HttpContext, string DocTypeAlias = "", bool PublishedOnly = false, string Search = "")
	{
		if (DocTypeAlias != "")
		{
			return ContentForDoctypeHtml(HttpContext, DocTypeAlias, PublishedOnly, Search);
		}
		else
		{
			return DoctypesForContentForDoctypeHtml(HttpContext);
		}
	}

	internal string GetContentForElementHtml(HttpContext HttpContext, string DocTypeAlias = "", bool PublishedOnly = false, string Search = "")
	{
		if (DocTypeAlias != "")
		{
			return ContentForElementHtml(HttpContext, DocTypeAlias, PublishedOnly, Search);
		}
		else
		{
			return DoctypesForContentForDoctypeHtml(HttpContext);
		}
	}

	private string DoctypesForContentForDoctypeHtml(HttpContext HttpContext)
	{
		//Setup
		var pvPath = _RazorFilesPath + "DoctypesForContentList.cshtml";
		var saService = SiteAuditorService;

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
		viewData.Add("StandardInfo", _StandardViewInfo);
		viewData.Add("Status", status);


		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}

	private string ContentForDoctypeHtml(HttpContext HttpContext, string DocTypeAlias, bool PublishedOnly, string Search = "")
	{
		//Setup
		var pvPath = _RazorFilesPath + "AllContentAsHtmlTable.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var contentNodes = saService.GetContentNodes(DocTypeAlias, PublishedOnly);

		//VIEW DATA 
		var model = contentNodes;
		var viewData = new Dictionary<string, object>();
		var standardInfo = _StandardViewInfo;
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("DocTypeAlias", DocTypeAlias);
		viewData.Add("PublishedOnly", PublishedOnly);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}

	private string ContentForElementHtml(HttpContext HttpContext, string DocTypeAlias, bool PublishedOnly, string Search = "")
	{
		//Setup
		var pvPath = _RazorFilesPath + "AllElementContentAsHtmlTable.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var contentNodes = saService.GetContentNodesUsingElement(DocTypeAlias, PublishedOnly);

		//VIEW DATA 
		var model = contentNodes;
		var viewData = new Dictionary<string, object>();
		var standardInfo = _StandardViewInfo;
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("DocTypeAlias", DocTypeAlias);
		viewData.Add("PublishedOnly", PublishedOnly);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}
	
	#endregion

	#region Media Nodes

	internal string GetAllMediaAsHtmlTable(HttpContext HttpContext, bool ShowImageThumbnails = false, string Search = "")
	{
		//Setup
		var pvPath = _RazorFilesPath + "AllMediaAsHtmlTable.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var nodes = saService.GetMediaNodes();

		//VIEW DATA 
		var model = nodes;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", _StandardViewInfo);
		viewData.Add("Status", status);
		viewData.Add("ShowImageThumbnails", ShowImageThumbnails);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}

	internal string GetMediaForTypeHtml(HttpContext HttpContext, string MediaTypeAlias = "", bool ShowImageThumbnails = false, string Search = "")
	{
		if (MediaTypeAlias != "")
		{
			return MediaForTypeHtml(HttpContext, MediaTypeAlias, ShowImageThumbnails, Search);
		}
		else
		{
			return TypesForMediaHtml(HttpContext);
		}
	}

	private string TypesForMediaHtml(HttpContext HttpContext)
	{
		//Setup
		var pvPath = _RazorFilesPath + "MediaTypesForMediaList.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var data = saService.GetAllMediaTypes().OrderBy(n => n.Alias).ToList();

		//VIEW DATA 
		var model = data;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", _StandardViewInfo);
		viewData.Add("Status", status);


		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}

	private string MediaForTypeHtml(HttpContext HttpContext, string MediaTypeAlias, bool ShowImageThumbnails = false, string Search = "")
	{
		//Setup
		var pvPath = _RazorFilesPath + "AllMediaAsHtmlTable.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var nodes = saService.GetMediaNodes(MediaTypeAlias);

		//VIEW DATA 
		var model = nodes;
		var viewData = new Dictionary<string, object>();
		var standardInfo = _StandardViewInfo;
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("ShowImageThumbnails", ShowImageThumbnails);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}


	#endregion

	#region GetContentWithValues

	internal string GetContentWithValues(HttpContext HttpContext, string PropertyAlias = "", bool PublishedOnly = false, string Search = "")
	{
		//GET DATA TO DISPLAY
		if (PropertyAlias == "")
		{
			return GetContentWithValuesList(HttpContext);
		}
		else
		{
			return GetContentWithValuesTable(HttpContext, PropertyAlias, PublishedOnly, Search);
		}
	}

	private string GetContentWithValuesTable(HttpContext HttpContext, string PropertyAlias, bool PublishedOnly, string Search = "")
	{
		//Setup
		var pvPath = _RazorFilesPath + "ContentWithValuesTable.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var displayHtml = "";

		//Get matching Property data
		var contentNodes = saService.GetContentWithProperty(PropertyAlias, PublishedOnly);

		//VIEW DATA 
		var model = contentNodes;
		var viewData = new Dictionary<string, object>();
		var standardInfo = _StandardViewInfo;
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("PropertyAlias", PropertyAlias);
		viewData.Add("PublishedOnly", PublishedOnly);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}

	private string GetContentWithValuesList(HttpContext HttpContext)
	{
		//Setup
		var pvPath = _RazorFilesPath + "ContentWithValuesList.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var displayHtml = "";


		//Get list of properties
		//displayHtml = HtmlListOfProperties();
		var allPropsAliases = ListOfProperties();

		//VIEW DATA 
		var model = allPropsAliases;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", _StandardViewInfo);
		viewData.Add("Status", status);
		//viewData.Add("PropertyAlias", PropertyAlias);
		// viewData.Add("IncludeUnpublished", IncludeUnpublished);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}


	/// <summary>
	/// Returns a list of all property aliases and their data types
	/// </summary>
	/// <returns>Key = Property Alias; Value=DataType object</returns>
	private IEnumerable<KeyValuePair<string, AuditableDataType>> ListOfProperties()
	{
		//Setup
		var saService = SiteAuditorService;

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


	#endregion

	#region GetMediaWithValues

	internal string GetMediaWithValues(HttpContext HttpContext, string PropertyAlias = "", string Search = "")
	{
		//GET DATA TO DISPLAY
		if (PropertyAlias == "")
		{
			return GetMediaWithValuesList(HttpContext);
		}
		else
		{
			return GetMediaWithValuesTable(HttpContext, PropertyAlias, Search);
		}
	}

	private string GetMediaWithValuesTable(HttpContext HttpContext, string PropertyAlias, string Search = "")
	{
		//Setup
		var pvPath = _RazorFilesPath + "MediaWithValuesTable.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var displayHtml = "";

		//Get matching Property data
		var nodes = saService.GetMediaWithProperty(PropertyAlias);

		//VIEW DATA 
		var model = nodes;
		var viewData = new Dictionary<string, object>();
		var standardInfo = _StandardViewInfo;
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("PropertyAlias", PropertyAlias);
		// viewData.Add("IncludeUnpublished", IncludeUnpublished);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}

	private string GetMediaWithValuesList(HttpContext HttpContext)
	{
		//Setup
		var pvPath = _RazorFilesPath + "MediaWithValuesList.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var displayHtml = "";


		//Get list of properties
		//displayHtml = HtmlListOfProperties();
		var allPropsAliases = ListOfMediaProperties();

		//VIEW DATA 
		var model = allPropsAliases;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", _StandardViewInfo);
		viewData.Add("Status", status);
		//viewData.Add("PropertyAlias", PropertyAlias);
		// viewData.Add("IncludeUnpublished", IncludeUnpublished);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}

	private IList<string> ListOfMediaProperties()
	{
		//Setup
		var saService = SiteAuditorService;

		//GET DATA
		var allTypes = saService.GetAllMediaTypes();
		var allProps = allTypes.SelectMany(n => n.PropertyTypes);
		var allPropsAliases = allProps.Select(n => n.Alias).Distinct();

		return allPropsAliases.ToList();
	}


	#endregion

	#region Properties Info

	internal string GetAllPropertiesAsHtmlTable(HttpContext HttpContext, string Search = "")
	{
		//Setup
		var pvPath = _RazorFilesPath + "AllPropertiesAsHtmlTable.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);

		var siteProps = saService.AllProperties();
		var propertiesList = siteProps.AllProperties;

		//VIEW DATA 
		var model = propertiesList;
		var viewData = new Dictionary<string, object>();
		var standardInfo = _StandardViewInfo;
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("DocTypeAlias", "");

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}

	internal string GetPropertiesForDoctypeHtml(HttpContext HttpContext, string DocTypeAlias = "", string Search = "")
	{
		if (DocTypeAlias != "")
		{
			return PropsForDoctypeHtml(HttpContext, DocTypeAlias, Search);
		}
		else
		{
			return DoctypesForPropertiesForDoctypeHtml(HttpContext);
		}
	}

	private string PropsForDoctypeHtml(HttpContext HttpContext, string DocTypeAlias, string Search = "")
	{
		//Setup
		var pvPath = _RazorFilesPath + "AllPropertiesAsHtmlTable.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);

		var siteProps = saService.AllPropertiesForDocType(DocTypeAlias);
		var propertiesList = siteProps.AllProperties;


		//VIEW DATA 
		var model = propertiesList;
		var viewData = new Dictionary<string, object>();
		var standardInfo = _StandardViewInfo;
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);
		viewData.Add("Title", $"Properties for Document Type '{DocTypeAlias}'");
		viewData.Add("DocTypeAlias", DocTypeAlias);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}

	private string DoctypesForPropertiesForDoctypeHtml(HttpContext HttpContext)
	{
		//Setup
		var pvPath = _RazorFilesPath + "DoctypesForPropertiesForDoctypeList.cshtml";

		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);

		var allSiteDocTypes = saService.GetAllDocTypes();
		var allAliases = allSiteDocTypes.Select(n => n.Alias).OrderBy(n => n).ToList();

		//VIEW DATA 
		var model = allAliases;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", _StandardViewInfo);
		viewData.Add("Status", status);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}

	
	#endregion

	#region DataType Info

	internal string GetAllDataTypesAsHtmlTable(HttpContext HttpContext, string Search = "")
	{
		//Setup
		var pvPath = _RazorFilesPath + "AllDataTypesAsHtmlTable.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);

		var dataTypes = saService.AllDataTypes();

		//VIEW DATA 
		var model = dataTypes;
		var viewData = new Dictionary<string, object>();
		var standardInfo = _StandardViewInfo;
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		return displayHtml;
	}


	#endregion

	#region DocTypes Info

	internal string GetAllDocTypesAsHtmlTable(HttpContext HttpContext, string Search = "")
	{
		//Setup
		var pvPath = _RazorFilesPath + "AllDocTypesAsHtmlTable.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var allDts = saService.GetAuditableDocTypes();

		//VIEW DATA 
		var model = allDts;
		var viewData = new Dictionary<string, object>();
		var standardInfo = _StandardViewInfo;
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		return displayHtml;
	}


	#endregion

	#region Element Types Info

	internal string GetContentForElementType(HttpContext HttpContext, string ElementTypeAlias = "", bool PublishedOnly = false, string Search = "")
	{
		//GET DATA TO DISPLAY
		if (ElementTypeAlias == "")
		{
			return GetContentForElementTypeList(HttpContext);
		}
		else
		{
			return GetContentForElementTypeTable(HttpContext, ElementTypeAlias, PublishedOnly, Search);
		}
	}

	private string GetContentForElementTypeList(HttpContext HttpContext)
	{
		//Setup
		var pvPath = _RazorFilesPath + "ContentForElementTypeList.cshtml";
		//var saService = _SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var displayHtml = "";


		//Get list of properties
		//displayHtml = HtmlListOfProperties();
		var allElementTypeAliases = ListOfElementTypes();

		//VIEW DATA 
		var model = allElementTypeAliases;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", _StandardViewInfo);
		viewData.Add("Status", status);
		//viewData.Add("PropertyAlias", PropertyAlias);
		// viewData.Add("IncludeUnpublished", IncludeUnpublished);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}

	private string GetContentForElementTypeTable(HttpContext HttpContext, string ElementTypeAlias, bool PublishedOnly, string Search)
	{
		//Setup
		var pvPath = _RazorFilesPath + "ContentForElementTypeTable.cshtml";
		var saService = SiteAuditorService;

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

								if (hasElementInContent)
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
			var standardInfo = _StandardViewInfo;
			standardInfo.DefaultSearchString = Search;
			viewData.Add("StandardInfo", standardInfo);
			viewData.Add("Status", status);
			viewData.Add("ElementTypeAlias", ElementTypeAlias);
			viewData.Add("ElementTypeGuid", elementGuid);
			viewData.Add("PublishedOnly", PublishedOnly);

			//RENDER
			var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
			displayHtml = htmlTask.Result;
		}

		//RETURN AS HTML
		var result = new ContentResult()
		{
			Content = displayHtml,
			StatusCode = (int)HttpStatusCode.OK,
			ContentType = "text/html; charset=utf-8"
		};

		return displayHtml;
	}

	private IList<AuditableDocType> ListOfElementTypes()
	{
		//Setup
		var saService = SiteAuditorService;
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

	internal string GetAllTemplatesAsHtmlTable(HttpContext HttpContext, string Search = "")
	{
		//Setup
		var pvPath = _RazorFilesPath + "AllTemplatesAsHtmlTable.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var allTemps = saService.GetAuditableTemplates();

		//VIEW DATA 
		var model = allTemps;
		var viewData = new Dictionary<string, object>();
		var standardInfo = _StandardViewInfo;
		standardInfo.DefaultSearchString = Search;
		viewData.Add("StandardInfo", standardInfo);
		viewData.Add("Status", status);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN AS HTML
		return displayHtml;
	}

	#endregion

	#region Special Queries
	internal string TemplateUsageReport(HttpContext HttpContext)
	{
		//Setup
		var pvPath = _RazorFilesPath + "TemplateUsageReport.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);

		//VIEW DATA 
		var model = status;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", _StandardViewInfo);
		viewData.Add("Status", status);
		viewData.Add("TemplatesUsedOnContent", saService.TemplatesUsedOnContent());
		viewData.Add("TemplatesNotUsedOnContent", saService.TemplatesNotUsedOnContent());

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}

	#endregion

	#region Logs

	internal string GetLogs(HttpContext HttpContext, DateTime? StartDate = null, DateTime? EndDate = null, int BatchBy = 0, string PromotedProperties = "")
	{
		//GET DATA TO DISPLAY
		if (StartDate is null)
		{
			return GetLogDateOptions(HttpContext, BatchBy);
		}
		else
		{
			var start = (DateTime)StartDate;
			return GetLogsAsHtmlTable(HttpContext, start, EndDate, PromotedProperties);
		}
	}


	private string GetLogDateOptions(HttpContext HttpContext, int BatchBy)
	{
		//Setup
		var pvPath = _RazorFilesPath + "LogOptions.cshtml";
		var saService = SiteAuditorService;

		//GET DATA TO DISPLAY
		var status = new StatusMessage(true);
		var displayHtml = "";


		//Get list of properties
		//displayHtml = HtmlListOfProperties();
		var dateOptions = DateOptions();

		//VIEW DATA 
		var model = dateOptions;
		var viewData = new Dictionary<string, object>();
		viewData.Add("StandardInfo", _StandardViewInfo);
		viewData.Add("Status", status);
		viewData.Add("BatchBy", BatchBy);
		// viewData.Add("IncludeUnpublished", IncludeUnpublished);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
	}


	internal string GetLogsAsHtmlTable(HttpContext HttpContext, DateTime StartDate, DateTime? EndDate = null, string PromotedProperties = "")
	{
		//Setup
		var pvPath = _RazorFilesPath + "LogsAsHtmlTable.cshtml";
		var saService = SiteAuditorService;

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
		viewData.Add("StandardInfo", _StandardViewInfo);
		viewData.Add("Status", status);
		viewData.Add("AllDateOptions", DateOptions());
		viewData.Add("ThisDateOption", matchingDateOption);
		viewData.Add("PromotedProperties", PromotedProperties);

		//RENDER
		var htmlTask = ViewRenderService.RenderToStringAsync(HttpContext, pvPath, model, viewData);
		var displayHtml = htmlTask.Result;

		//RETURN HTML
		return displayHtml;
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
		var canMapPath = FileHelperService.TryGetMappedPath(_StandardViewInfo.SerilogDirectory, out mappedPath, true);

		return mappedPath;
	}

	#endregion

}