namespace Dragonfly.SiteAuditor.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
//using System.Text.Json;
//using System.Text.Json.Nodes;

using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;

using Dragonfly.NetModels;
using Dragonfly.SiteAuditor.Models;
using Dragonfly.NetHelperServices;
using Dragonfly.UmbracoHelpers;
using Serilog.Formatting.Compact.Reader;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;

#pragma warning disable 0168
public class SiteAuditorService
{

	#region Private & Internal Variables

	private IEnumerable<AuditableContent> _AllAuditableContent = new List<AuditableContent>();

	private IEnumerable<AuditableContent> _AllPublishedAuditableContent = new List<AuditableContent>();

	private IEnumerable<AuditableMedia> _AllAuditableMedia = new List<AuditableMedia>();

	private IEnumerable<IContent> _AllContent = new List<IContent>();

	//private IEnumerable<IMedia> _AllMedia = new List<IMedia>();
	private List<IPublishedContent> _AllIPubMedia = new List<IPublishedContent>();

	/// <summary>
	/// Use .GetAllCompositions() to return this list
	/// </summary>
	private IEnumerable<IContentTypeComposition> _AllContentTypeComps = new List<IContentType>();

	private IEnumerable<AuditableDataType> _AllDataTypes = new List<AuditableDataType>();

	private IEnumerable<DataTypesWithElements> _AllDataTypesWithElements = new List<DataTypesWithElements>();

	internal static string DataPath()
	{
		//var config = Config.GetConfig();
		//return config.GetDataPath();

		return "/App_Data/DragonflySiteAuditor/";
	}

	internal static string PluginPath()
	{
		//var config = Config.GetConfig();
		//return config.GetDataPath();

		return "~/App_Plugins/Dragonfly.SiteAuditor/";
	}

	#endregion

	#region Public Properties

	/// <summary>
	/// Default string used for NodePathAsText
	/// ' » ' unless explicitly changed
	/// </summary>
	public string DefaultDelimiter
	{
		get { return _defaultDelimiter; }
		internal set { _defaultDelimiter = value; }
	}

	private string _defaultDelimiter = " » ";

	#endregion

	#region CTOR & DI

	private readonly UmbracoHelper _umbracoHelper;
	private readonly ILogger<SiteAuditorService> _logger;
	private readonly IUmbracoContextAccessor _umbracoContextAccessor;
	private readonly IUmbracoContext? _umbracoContext;
	private readonly ServiceContext _services;
	private readonly FileHelperService _FileHelperService;
	private readonly HttpContext _Context;
	private readonly IHostingEnvironment _HostingEnvironment;
	private readonly DependencyLoader _Dependencies;

	private readonly AuditorInfoService _auditorInfoService;

	private bool _HasUmbracoContext;



	public SiteAuditorService(
		DependencyLoader dependencies,
		ILogger<SiteAuditorService> logger,
		AuditorInfoService auditorInfoService)
	{
		//Services
		_Dependencies = dependencies;
		_HostingEnvironment = dependencies.HostingEnvironment;
		_umbracoHelper = dependencies.UmbHelper;
		_FileHelperService = dependencies.DragonflyFileHelperService;
		_Context = dependencies.Context;
		_logger = logger;
		_services = dependencies.Services;

		_umbracoContextAccessor = dependencies.UmbracoContextAccessor;
		_HasUmbracoContext = _umbracoContextAccessor.TryGetUmbracoContext(out _umbracoContext);

		_auditorInfoService = auditorInfoService;
	}

	//public SiteAuditorService(UmbracoHelper UmbHelper, UmbracoContext UmbContext, ServiceContext Services, ILogger Logger)
	//{
	//    //Services
	//    _umbracoHelper = UmbHelper;
	//    _services = Services;
	//    _logger = Logger;
	//    _umbracoContext = UmbContext;
	//    _HasUmbracoContext = true;
	//}

	#endregion

	#region All Published Content (IPublishedContent)

	/// <summary>
	/// Gets all Content nodes as IPublishedContent
	/// </summary>
	/// <returns></returns>
	public IList<IPublishedContent> GetAllPublishedContent()
	{
		var nodesList = new List<IPublishedContent>();

		var topLevelNodes = _umbracoHelper.ContentAtRoot().OrderBy(n => n.SortOrder);

		foreach (var thisNode in topLevelNodes)
		{
			nodesList.AddRange(LoopNodes(thisNode));
		}

		return nodesList;
	}

	private IList<IPublishedContent> LoopNodes(IPublishedContent ThisNode)
	{
		var nodesList = new List<IPublishedContent>();

		//Add current node, then loop for children
		try
		{
			nodesList.Add(ThisNode);

			if (ThisNode.Children != null && ThisNode.Children.Any())
			{
				foreach (var childNode in ThisNode.Children.OrderBy(n => n.SortOrder))
				{
					nodesList.AddRange(LoopNodes(childNode));
				}
			}
		}
		catch (Exception e)
		{
			//skip
		}

		return nodesList;
	}

	#endregion

	#region Content

	public List<IContent> GetAllContent()
	{
		var nodesList = new List<IContent>();

		var topLevelContentNodes = _services.ContentService!.GetRootContent().OrderBy(n => n.SortOrder);

		foreach (var thisNode in topLevelContentNodes)
		{
			nodesList.AddRange(LoopForContentNodes(thisNode));
		}

		_AllContent = nodesList;
		return nodesList;
	}

	internal List<IContent> LoopForContentNodes(IContent ThisNode)
	{
		var nodesList = new List<IContent>();

		//Add current node, then loop for children
		nodesList.Add(ThisNode);

		//figure out num of children
		long countChildren;
		var test = _services.ContentService!.GetPagedChildren(ThisNode.Id, 0, 1, out countChildren);
		if (countChildren > 0)
		{
			long countTest;
			var allChildren =
				_services.ContentService.GetPagedChildren(ThisNode.Id, 0, Convert.ToInt32(countChildren),
					out countTest);
			foreach (var childNode in allChildren.OrderBy(n => n.SortOrder))
			{
				nodesList.AddRange(LoopForContentNodes(childNode));
			}
		}

		return nodesList;
	}

	#endregion

	#region AuditableContent

	/// <summary>
	/// Gets all site nodes as AuditableContent models
	/// </summary>
	/// <param name="DocTypeAlias"></param>
	/// <returns></returns>
	public List<AuditableContent> GetContentNodes(bool PublishedOnly)
	{
		if (PublishedOnly)
		{
			if (_AllPublishedAuditableContent.Any())
			{
				return _AllPublishedAuditableContent.ToList();
			}
		}
		else
		{
			if (_AllAuditableContent.Any())
			{
				return _AllAuditableContent.ToList();
			}
		}

		var nodesList = new List<AuditableContent>();

		if (PublishedOnly)
		{
			var topLevelContentNodes = _umbracoHelper.ContentAtRoot().OrderBy(n => n.SortOrder);

			foreach (var thisNode in topLevelContentNodes)
			{
				nodesList.AddRange(LoopForAuditableContentNodes(thisNode));
			}

			_AllPublishedAuditableContent = nodesList;
		}
		else
		{
			var topLevelContentNodes = _services.ContentService!.GetRootContent().OrderBy(n => n.SortOrder);

			foreach (var thisNode in topLevelContentNodes)
			{
				nodesList.AddRange(LoopForAuditableContentNodes(thisNode));
			}

			_AllAuditableContent = nodesList;
		}

		return nodesList;
	}

	/// <summary>
	/// Gets all descendant nodes from provided Root Node Id as AuditableContent models
	/// </summary>
	/// <param name="RootNodeId">Integer Id of Root Node</param>
	/// <returns></returns>
	public List<AuditableContent> GetContentNodes(int RootNodeId, bool PublishedOnly)
	{
		var nodesList = new List<AuditableContent>();

		if (PublishedOnly)
		{
			var topLevelNodes = _umbracoHelper.Content(RootNodeId.AsEnumerableOfOne()).OrderBy(n => n.SortOrder);

			foreach (var thisNode in topLevelNodes)
			{
				nodesList.AddRange(LoopForAuditableContentNodes(thisNode));
			}
		}
		else
		{
			var topLevelNodes = _services.ContentService!.GetByIds(RootNodeId.AsEnumerableOfOne())
				.OrderBy(n => n.SortOrder);

			foreach (var thisNode in topLevelNodes)
			{
				nodesList.AddRange(LoopForAuditableContentNodes(thisNode));
			}
		}

		return nodesList;
	}

	/// <summary>
	/// Gets all descendant nodes from provided Root Node Id as AuditableContent models
	/// </summary>
	/// <param name="RootNodeUdi">Udi of Root Node</param>
	/// <returns></returns>
	public List<AuditableContent> GetContentNodes(Udi RootNodeUdi, bool PublishedOnly)
	{
		var nodesList = new List<AuditableContent>();

		if (PublishedOnly)
		{
			var topLevelNodes = _umbracoHelper.Content(RootNodeUdi.AsEnumerableOfOne()).OrderBy(n => n.SortOrder);

			foreach (var thisNode in topLevelNodes)
			{
				nodesList.AddRange(LoopForAuditableContentNodes(thisNode));
			}
		}
		else
		{
			var topLevelNodes = _services.ContentService!.GetByIds(RootNodeUdi.AsEnumerableOfOne())!.OrderBy(n => n.SortOrder);

			foreach (var thisNode in topLevelNodes)
			{
				nodesList.AddRange(LoopForAuditableContentNodes(thisNode));
			}
		}

		return nodesList;
	}

	public List<AuditableContent> GetContentNodes(string DocTypeAlias, bool PublishedOnly)
	{
		var nodesList = new List<AuditableContent>();

		if (PublishedOnly)
		{
			var allPublishedContent = GetAllPublishedContent();

			var filteredContent = allPublishedContent.Where(n => n.ContentType.Alias == DocTypeAlias);

			foreach (var content in filteredContent)
			{
				nodesList.Add(ConvertIPubContentToAuditableContent(content));
			}
		}
		else
		{
			IEnumerable<IContent> allContent;
			if (_AllContent.Any())
			{
				allContent = _AllContent.ToList();
			}
			else
			{
				allContent = GetAllContent().ToList();
			}

			var filteredContent = allContent.Where(n => n.ContentType.Alias == DocTypeAlias);

			foreach (var content in filteredContent)
			{
				nodesList.Add(ConvertIContentToAuditableContent(content));
			}

		}

		return nodesList;
	}


	public List<KeyValuePair<AuditableContent, NodePropertyDataTypeInfo>> GetContentNodesUsingElement(string DocTypeAlias, bool PublishedOnly)
	{
		//TODO: This doesn't work with Published Only - needs fixing

		_logger.LogDebug($"~~DocTypeAlias: {DocTypeAlias}");
		//DataTypes using the Element
		var dataTypes = AllDataTypesUsingElement(DocTypeAlias).ToList();
		_logger.LogDebug($"~~DocTypeAlias: {DocTypeAlias} - DataTypes Using: {dataTypes.Count()}");

		//Properties which might use the element type
		var possibleProperties = new List<KeyValuePair<IPropertyType, string>>();
		foreach (AuditableDataType auditableDataType in dataTypes)
		{
			possibleProperties.AddRange(auditableDataType.UsedOnProperties);
		}

		var finalContentList = new List<KeyValuePair<AuditableContent, NodePropertyDataTypeInfo>>();

		foreach (var property in possibleProperties)
		{
			//Content Nodes with properties which might use the element type
			var allContentWithProp = GetContentWithProperty(property.Key.Alias, PublishedOnly);

			//Nodes where the property IS using the Element
			if (PublishedOnly)
			{
				//USE IPublishedContent
				foreach (var content in allContentWithProp)
				{
					if (content.UmbPublishedNode != null)
					{
						_logger.LogDebug($"1. Node #{content.UmbPublishedNode.Id} - {content.UmbPublishedNode.Name} for Element use: Property '{property.Key.Alias}'");

						var info = _auditorInfoService.GetPropertyDataTypeInfo(property.Key.Alias, content.UmbPublishedNode);
						_logger.LogDebug($"2. Node #{content.UmbPublishedNode.Id} - {content.UmbPublishedNode.Name} for Element use: Property '{property.Key.Alias}' - Info.DocType='{info.DocTypeAlias}'");

						if (info.PropertyData != null)
						{
							var valueString = info.PropertyData.ToString();
							_logger.LogDebug($"3A. Node #{content.UmbPublishedNode.Id} - {content.UmbPublishedNode.Name} for Element use: Property '{property.Key.Alias}' - Value='{(valueString == null ? "NULL" : valueString)}'");

							var elementContentType = _services.ContentTypeService!.Get(DocTypeAlias);
							_logger.LogDebug($"4. Node #{content.UmbPublishedNode.Id} - {content.UmbPublishedNode.Name} for Element use: Property '{property.Key.Alias}' - elementContentType='{(elementContentType == null ? "NULL" : elementContentType.Alias)}'");

							if (elementContentType != null)
							{
								var elementGuid = elementContentType.Key;
								_logger.LogInformation($"5. Node #{content.UmbPublishedNode.Id} - {content.UmbPublishedNode.Name} for Element use: Property '{property.Key.Alias}' - elementGuid='{elementGuid.ToString()}'");

								if (valueString != null && valueString.Contains(elementGuid.ToString()))
								{
									_logger.LogInformation($"6A. Node #{content.UmbPublishedNode.Id} - {content.UmbPublishedNode.Name} for Element use: Property '{property.Key.Alias}' - Value Includes GUID!");

									var match = new KeyValuePair<AuditableContent, NodePropertyDataTypeInfo>(content, info);
									finalContentList.Add(match);
								}
								else
								{
									var elementAlias = elementContentType.Alias;
									_logger.LogInformation($"6B. Node #{content.UmbPublishedNode.Id} - {content.UmbPublishedNode.Name} for Element use: Property '{property.Key.Alias}' - elementAlias='{(elementAlias == null ? "NULL" : elementAlias.ToString())}'");
									if (elementAlias != null && valueString != null)
									{
										if (valueString.Contains(elementAlias.ToString()))
										{
											_logger.LogInformation($"7A. Node #{content.UmbPublishedNode.Id} - {content.UmbPublishedNode.Name} for Element use: Property '{property.Key.Alias}' - Value Includes Alias!");

											var match = new KeyValuePair<AuditableContent, NodePropertyDataTypeInfo>(content, info);
											finalContentList.Add(match);
										}
										else
										{
											_logger.LogInformation($"7B. Node #{content.UmbPublishedNode.Id} - {content.UmbPublishedNode.Name} for Element use: Property '{property.Key.Alias}' - Value DOES NOT include Alias!");
										}
									}
								}
							}
						}
						else
						{
							_logger.LogDebug($"3B. Node #{content.UmbPublishedNode.Id} - {content.UmbPublishedNode.Name} for Element use: Property '{property.Key.Alias}' - PropertyData IS NULL'");
						}
					}
				}
			}
			else
			{
				//USE IContent
				foreach (var content in allContentWithProp)
				{
					if (content.UmbContentNode != null)
					{
						_logger.LogDebug(
							$"1. Node #{content.UmbContentNode.Id} - {content.UmbContentNode.Name} for Element use: Property '{property.Key.Alias}'");

						var info = _auditorInfoService.GetPropertyDataTypeInfo(property.Key.Alias, content.UmbContentNode);
						_logger.LogDebug($"2. Node #{content.UmbContentNode.Id} - {content.UmbContentNode.Name} for Element use: Property '{property.Key.Alias}' - Info.DocType='{info.DocTypeAlias}'");

						if (info.PropertyData != null)
						{
							var valueString = info.PropertyData.ToString();
							_logger.LogDebug($"3A. Node #{content.UmbContentNode.Id} - {content.UmbContentNode.Name} for Element use: Property '{property.Key.Alias}' - Value='{(valueString == null ? "NULL" : valueString)}'");

							var elementContentType = _services.ContentTypeService!.Get(DocTypeAlias);
							_logger.LogDebug($"4. Node #{content.UmbContentNode.Id} - {content.UmbContentNode.Name} for Element use: Property '{property.Key.Alias}' - elementContentType='{(elementContentType == null ? "NULL" : elementContentType.Alias)}'");

							if (elementContentType != null)
							{
								var elementGuid = elementContentType.Key;
								_logger.LogInformation($"5. Node #{content.UmbContentNode.Id} - {content.UmbContentNode.Name} for Element use: Property '{property.Key.Alias}' - elementGuid='{elementGuid.ToString()}'");

								if (valueString != null && valueString.Contains(elementGuid.ToString()))
								{
									_logger.LogInformation($"6A. Node #{content.UmbContentNode.Id} - {content.UmbContentNode.Name} for Element use: Property '{property.Key.Alias}' - Value Includes GUID!");

									var match = new KeyValuePair<AuditableContent, NodePropertyDataTypeInfo>(content, info);
									finalContentList.Add(match);
								}
								else
								{
									var elementAlias = elementContentType.Alias;
									_logger.LogInformation($"6B. Node #{content.UmbContentNode.Id} - {content.UmbContentNode.Name} for Element use: Property '{property.Key.Alias}' - elementAlias='{(elementAlias == null ? "NULL" : elementAlias.ToString())}'");
									if (elementAlias != null && valueString != null)
									{
										if (valueString.Contains(elementAlias.ToString()))
										{
											_logger.LogInformation($"7A. Node #{content.UmbContentNode.Id} - {content.UmbContentNode.Name} for Element use: Property '{property.Key.Alias}' - Value Includes Alias!");

											var match = new KeyValuePair<AuditableContent, NodePropertyDataTypeInfo>(content, info);
											finalContentList.Add(match);
										}
										else
										{
											_logger.LogInformation($"7B. Node #{content.UmbContentNode.Id} - {content.UmbContentNode.Name} for Element use: Property '{property.Key.Alias}' - Value DOES NOT include Alias!");
										}
									}
								}
							}
						}
						else
						{
							_logger.LogDebug($"3B. Node #{content.UmbContentNode.Id} - {content.UmbContentNode.Name} for Element use: Property '{property.Key.Alias}' - PropertyData IS NULL'");
						}
					}
				}
			}
		}

		return finalContentList;

	}

	public List<AuditableContent> GetContentWithProperty(string PropertyAlias, bool PublishedOnly)
	{
		if (PublishedOnly)
		{
			var allContent = GetAllPublishedContent();
			var contentWithProp = allContent.Where(n => n.HasProperty(PropertyAlias)).ToList();
			return ConvertIPubContentToAuditableContent(contentWithProp);
		}
		else
		{

			var allContent = GetAllContent();
			var contentWithProp = allContent.Where(n => n.HasProperty(PropertyAlias)).ToList();
			return ConvertIContentToAuditableContent(contentWithProp);
		}
	}

	internal List<AuditableContent> LoopForAuditableContentNodes(IContent ThisNode)
	{
		var nodesList = new List<AuditableContent>();

		//Add current node, then loop for children
		AuditableContent auditContent = ConvertIContentToAuditableContent(ThisNode);
		nodesList.Add(auditContent);

		//figure out num of children
		long countChildren;
		var test = _services.ContentService!.GetPagedChildren(ThisNode.Id, 0, 1, out countChildren);
		if (countChildren > 0)
		{
			long countTest;
			var allChildren = _services.ContentService.GetPagedChildren(ThisNode.Id, 0,
				Convert.ToInt32(countChildren), out countTest);
			foreach (var childNode in allChildren.OrderBy(n => n.SortOrder))
			{
				nodesList.AddRange(LoopForAuditableContentNodes(childNode));
			}
		}

		return nodesList;
	}

	internal List<AuditableContent> LoopForAuditableContentNodes(IPublishedContent ThisNode)
	{
		var nodesList = new List<AuditableContent>();

		//Add current node, then loop for children
		AuditableContent auditContent = ConvertIPubContentToAuditableContent(ThisNode);
		nodesList.Add(auditContent);

		var hasChildren = ThisNode.Children() != null;
		if (hasChildren)
		{
			var allChildren = ThisNode.Children()!.ToList();
			foreach (var childNode in allChildren.OrderBy(n => n.SortOrder))
			{
				nodesList.AddRange(LoopForAuditableContentNodes(childNode));
			}
		}

		return nodesList;
	}


	private AuditableContent ConvertIContentToAuditableContent(IContent ThisIContent)
	{
		var ac = new AuditableContent();

		ac.UmbContentNode = ThisIContent;
		ac.IsPublished = ac.UmbContentNode.Published;
		ac.NodeId = ThisIContent.Id;
		ac.NodeUdi = ThisIContent.GetUdi().ToString();
		ac.NodeName = ThisIContent.Name ?? "";
		ac.NodeContentTypeAlias = ThisIContent.ContentType.Alias;
		ac.NodeParentId = ThisIContent.ParentId;
		ac.NodeLevel = ThisIContent.Level;
		ac.NodeSortOrder = ThisIContent.SortOrder;
		ac.NodeCreateDate = ThisIContent.CreateDate;
		ac.NodeUpdateDate = ThisIContent.UpdateDate;


		if (ThisIContent.TemplateId != null)
		{
			var template = _services.FileService!.GetTemplate((int)ThisIContent.TemplateId);
			ac.TemplateAlias = template != null ? template.Alias : "MISSING";
		}
		else
		{
			ac.TemplateAlias = "NONE";
		}

		if (ac.UmbContentNode.Published)
		{
			try
			{
				var iPub = _umbracoHelper.Content(ThisIContent.Id);
				ac.UmbPublishedNode = iPub;
				//ac.RelativeNiceUrl = iPub.Url(mode: UrlMode.Relative);
				//ac.FullNiceUrl = iPub.Url(mode: UrlMode.Absolute);
			}
			catch (Exception e)
			{
				//Get preview - unpublished
				var iPub = _umbracoContext!.Content!.GetById(true, ThisIContent.Id);
				ac.UmbPublishedNode = iPub;
			}

			if (ac.UmbPublishedNode != null && ac.UmbPublishedNode.ItemType == PublishedItemType.Element)
			{
				_logger.LogError($"SiteAuditorService.ConvertIContentToAuditableContent: Invalid Item Type (Element) on Node #{ac.UmbContentNode.Id} - Document Type = {ac.UmbContentNode.ContentType.Name}");
			}
			else
			{
				try
				{
					ac.RelativeNiceUrl = ac.UmbPublishedNode != null
						? ac.UmbPublishedNode.Url(mode: UrlMode.Relative)
						: "UNPUBLISHED";
					ac.FullNiceUrl = ac.UmbPublishedNode != null
						? ac.UmbPublishedNode.Url(mode: UrlMode.Absolute)
						: "UNPUBLISHED";
				}
				catch (Exception e)
				{
					_logger.LogWarning(e,
						$"SiteAuditorService.ConvertIContentToAuditableContent: Unable to set Urls on Node #{ac.UmbContentNode.Id}");
				}
			}
		}

		ac.NodePath = _auditorInfoService.NodePath(ThisIContent);
		ac.CreateUser = _auditorInfoService.GetUser(ThisIContent.CreatorId);
		ac.UpdateUser = _auditorInfoService.GetUser(ThisIContent.WriterId);

		return ac;
	}

	private List<AuditableContent> ConvertIContentToAuditableContent(List<IContent> ContentList)
	{
		var nodesList = new List<AuditableContent>();
		foreach (var content in ContentList)
		{
			nodesList.Add(ConvertIContentToAuditableContent(content));
		}

		return nodesList;
	}

	private AuditableContent ConvertIPubContentToAuditableContent(IPublishedContent PubContentNode)
	{
		var content = _umbracoHelper.Content(PubContentNode.Id);

		var ac = new AuditableContent();
		if (content != null)
		{
			ac.UmbContentNode = null;
			ac.NodePath = _auditorInfoService.PublishedContentNodePath(content);
			ac.TemplateAlias = GetTemplateAlias(content);
			ac.FullNiceUrl = content.Url(mode: UrlMode.Absolute);
			ac.RelativeNiceUrl = content.Url(mode: UrlMode.Relative);
			ac.IsPublished = true;

			ac.NodeId = content.Id;
			ac.NodeUdi = content.ToUdi().ToString();
			ac.NodeName = content.Name ?? "";
			ac.NodeContentTypeAlias = content.ContentType.Alias;
			ac.NodeParentId = content.Parent != null ? content.Parent.Id : 0;
			ac.NodeLevel = content.Level;
			ac.NodeSortOrder = content.SortOrder;
			ac.NodeCreateDate = content.CreateDate;
			ac.NodeUpdateDate = content.UpdateDate;
		}

		ac.UmbPublishedNode = PubContentNode;
		ac.CreateUser = _auditorInfoService.GetUser(PubContentNode.CreatorId);
		ac.UpdateUser = _auditorInfoService.GetUser(PubContentNode.WriterId);

		return ac;
	}

	private List<AuditableContent> ConvertIPubContentToAuditableContent(List<IPublishedContent> ContentList)
	{
		var nodesList = new List<AuditableContent>();
		foreach (var content in ContentList)
		{
			nodesList.Add(ConvertIPubContentToAuditableContent(content));
		}

		return nodesList;
	}

	#endregion

	#region Media

	public List<IPublishedContent> GetAllIPubMedia()
	{
		var nodesList = new List<IPublishedContent>();

		var topLevelNodes = _umbracoHelper.MediaAtRoot().OrderBy(n => n.SortOrder);

		foreach (var thisNode in topLevelNodes)
		{
			nodesList.AddRange(LoopForIPubMediaNodes(thisNode));
		}

		_AllIPubMedia = nodesList;
		return nodesList;
	}

	internal List<IPublishedContent> LoopForIPubMediaNodes(IPublishedContent ThisNode)
	{
		var nodesList = new List<IPublishedContent>();

		//Add current node, then loop for children
		nodesList.Add(ThisNode);


		var hasChildren = ThisNode.Children != null;
		if (hasChildren)
		{
			var allChildren = ThisNode.Children!.ToList();
			foreach (var childNode in allChildren.OrderBy(n => n.SortOrder))
			{
				nodesList.AddRange(LoopForIPubMediaNodes(childNode));
			}
		}

		return nodesList;
	}

	//public List<IMedia> GetAllMedia()
	//{
	//	var nodesList = new List<IMedia>();

	//	var topLevelNodes = _services.MediaService!.GetRootMedia().OrderBy(n => n.SortOrder);

	//	foreach (var thisNode in topLevelNodes)
	//	{
	//		nodesList.AddRange(LoopForMediaNodes(thisNode));
	//	}

	//	_AllMedia = nodesList;
	//	return nodesList;
	//}

	//internal List<IMedia> LoopForMediaNodes(IMedia ThisNode)
	//{
	//	var nodesList = new List<IMedia>();

	//	//Add current node, then loop for children
	//	nodesList.Add(ThisNode);

	//	//figure out num of children
	//	long countChildren;
	//	var test = _services.MediaService!.GetPagedChildren(ThisNode.Id, 0, 1, out countChildren);
	//	if (countChildren > 0)
	//	{
	//		long countTest;
	//		var allChildren =
	//			_services.MediaService.GetPagedChildren(ThisNode.Id, 0, Convert.ToInt32(countChildren), out countTest);
	//		foreach (var childNode in allChildren.OrderBy(n => n.SortOrder))
	//		{
	//			nodesList.AddRange(LoopForMediaNodes(childNode));
	//		}
	//	}

	//	return nodesList;
	//}

	#endregion

	#region AuditableMedia

	/// <summary>
	/// Gets all site nodes as AuditableMedia models
	/// </summary>
	/// <returns></returns>
	public List<AuditableMedia> GetMediaNodes()
	{
		if (_AllAuditableMedia.Any())
		{
			return _AllAuditableMedia.ToList();
		}

		var nodesList = new List<AuditableMedia>();

		var topLevelNodes = _umbracoHelper.MediaAtRoot().OrderBy(n => n.SortOrder);

		foreach (var thisNode in topLevelNodes)
		{
			nodesList.AddRange(LoopForAuditableMediaNodes(thisNode));
		}

		_AllAuditableMedia = nodesList;
		return nodesList;
	}

	public List<AuditableMedia> GetMediaNodes(string MediaTypeAlias)
	{
		var nodesList = new List<AuditableMedia>();

		IEnumerable<IPublishedContent> allMedia;
		if (_AllIPubMedia.Any())
		{
			allMedia = _AllIPubMedia.ToList();
		}
		else
		{
			allMedia = GetAllIPubMedia().ToList();
		}

		var filtered = allMedia.Where(n => n.ContentType.Alias == MediaTypeAlias);

		foreach (var item in filtered)
		{
			nodesList.Add(ConvertIPubMediaToAuditableMedia(item));
		}

		return nodesList;
	}

	private AuditableMedia ConvertIPubMediaToAuditableMedia(IPublishedContent ThisIPubMedia)
	{
		var am = new AuditableMedia();

		//am.UmbMediaNode = _services.MediaService.GetById(ThisIPubMedia.Id);

		var iPub = ThisIPubMedia;
		am.UmbPublishedNode = ThisIPubMedia;

		if (iPub != null)
		{
			am.WidthPixels = ThisIPubMedia.Value<int>("umbracoWidth");
			am.HeightPixels = ThisIPubMedia.Value<int>("umbracoHeight");
			am.FileExtension = iPub.Value<string>("umbracoExtension");

			if (iPub.HasProperty("AltText"))
			{
				am.AltText = iPub.Value<string>("AltText");
			}
			else if (iPub.HasProperty("altText"))
			{
				am.AltText = iPub.Value<string>("altText");
			}

			am.SizeInBytes = ThisIPubMedia.Value<long>("umbracoBytes");
			if (am.SizeInBytes > 0)
			{
				am.SizeReadable = Dragonfly.NetHelpers.Strings.FileBytesToString(am.SizeInBytes);
			}

		}

		am.RelativeUrl = am.UmbPublishedNode != null
			? am.UmbPublishedNode.Url(mode: UrlMode.Relative)
			: "NONE";
		am.FullUrl = am.UmbPublishedNode != null
			? am.UmbPublishedNode.Url(mode: UrlMode.Absolute)
			: "NONE";

		am.NodePath = _auditorInfoService.MediaNodePath(ThisIPubMedia);
		am.CreateUser = _auditorInfoService.GetUser(ThisIPubMedia.CreatorId);
		am.UpdateUser = _auditorInfoService.GetUser(ThisIPubMedia.WriterId);

		return am;
	}

	private List<AuditableMedia> ConvertIPubMediaToAuditableMedia(List<IPublishedContent> MediaList)
	{
		var nodesList = new List<AuditableMedia>();
		foreach (var media in MediaList)
		{
			nodesList.Add(ConvertIPubMediaToAuditableMedia(media));
		}

		return nodesList;
	}

	internal List<AuditableMedia> LoopForAuditableMediaNodes(IPublishedContent ThisNode)
	{
		var nodesList = new List<AuditableMedia>();

		//Add current node, then loop for children
		AuditableMedia auditContent = ConvertIPubMediaToAuditableMedia(ThisNode);
		nodesList.Add(auditContent);

		var hasChildren = ThisNode.Children != null;
		if (hasChildren)
		{
			var allChildren = ThisNode.Children!.ToList();
			foreach (var childNode in allChildren.OrderBy(n => n.SortOrder))
			{
				nodesList.AddRange(LoopForAuditableMediaNodes(childNode));
			}
		}

		return nodesList;
	}

	public List<AuditableMedia> GetMediaWithProperty(string PropertyAlias)
	{
		var allMedia = GetAllIPubMedia();
		var withProp = allMedia.Where(n => n.HasProperty(PropertyAlias)).ToList();

		return ConvertIPubMediaToAuditableMedia(withProp);
	}

	//internal List<AuditableMedia> LoopForAuditableMediaNodes(IMedia ThisNode)
	//{
	//	var nodesList = new List<AuditableMedia>();

	//	//Add current node, then loop for children
	//	AuditableMedia auditContent = ConvertIMediaToAuditableMedia(ThisNode);
	//	nodesList.Add(auditContent);

	//	//figure out num of children
	//	long countChildren;
	//	var test = _services.MediaService!.GetPagedChildren(ThisNode.Id, 0, 1, out countChildren);
	//	if (countChildren > 0)
	//	{
	//		long countTest;
	//		var allChildren = _services.MediaService.GetPagedChildren(ThisNode.Id, 0,
	//			Convert.ToInt32(countChildren), out countTest);
	//		foreach (var childNode in allChildren.OrderBy(n => n.SortOrder))
	//		{
	//			nodesList.AddRange(LoopForAuditableMediaNodes(childNode));
	//		}
	//	}

	//	return nodesList;
	//}

	//private AuditableMedia ConvertIMediaToAuditableMedia(IMedia ThisIMedia)
	//{
	//	var am = new AuditableMedia();

	//	//am.UmbMediaNode = ThisIMedia;

	//	var iPub = _umbracoHelper.Media(ThisIMedia.Id);
	//	am.UmbPublishedNode = iPub;

	//	if (iPub != null)
	//	{
	//		am.WidthPixels = ThisIMedia.GetValue<int>("umbracoWidth");
	//		am.HeightPixels = ThisIMedia.GetValue<int>("umbracoHeight");
	//		am.FileExtension = iPub.Value<string>("umbracoExtension");

	//		if (iPub.HasProperty("AltText"))
	//		{
	//			am.AltText = iPub.Value<string>("AltText");
	//		}
	//		else if (iPub.HasProperty("altText"))
	//		{
	//			am.AltText = iPub.Value<string>("altText");
	//		}

	//		am.SizeInBytes = ThisIMedia.GetValue<long>("umbracoBytes");
	//		if (am.SizeInBytes > 0)
	//		{
	//			am.SizeReadable = Dragonfly.NetHelpers.Strings.FileBytesToString(am.SizeInBytes);
	//		}

	//	}

	//	am.RelativeUrl = am.UmbPublishedNode != null
	//		? am.UmbPublishedNode.Url(mode: UrlMode.Relative)
	//		: "NONE";
	//	am.FullUrl = am.UmbPublishedNode != null
	//		? am.UmbPublishedNode.Url(mode: UrlMode.Absolute)
	//		: "NONE";

	//	am.NodePath = _auditorInfoService.NodePath(ThisIMedia);
	//	am.CreateUser = _auditorInfoService.GetUser(ThisIMedia.CreatorId);
	//	am.UpdateUser = _auditorInfoService.GetUser(ThisIMedia.WriterId);

	//	return am;
	//}

	//private List<AuditableMedia> ConvertIMediaToAuditableMedia(List<IMedia> MediaList)
	//{
	//	var nodesList = new List<AuditableMedia>();
	//	foreach (var media in MediaList)
	//	{
	//		nodesList.Add(ConvertIMediaToAuditableMedia(media));
	//	}

	//	return nodesList;
	//}



	#endregion

	#region DocTypes

	/// <summary>
	/// Get a ContentType model for a Doctype by its alias
	/// </summary>
	/// <param name="DocTypeAlias"></param>
	/// <returns></returns>
	public IContentType? GetContentTypeByAlias(string DocTypeAlias)
	{
		return _services.ContentTypeService!.Get(DocTypeAlias);
	}

	/// <summary>
	/// Gets list of all DocTypes on site as IContentType models
	/// </summary>
	/// <returns></returns>
	public IEnumerable<IContentType> GetAllDocTypes()
	{
		var list = new List<IContentType>();

		var doctypes = _services.ContentTypeService!.GetAll();

		foreach (var type in doctypes)
		{
			list.Add(type);
		}

		return list;
	}

	/// <summary>
	/// Gets list of all DocTypes on site as IContentType models (uses saved list, if available)
	/// </summary>
	/// <returns></returns>
	public IEnumerable<IContentTypeComposition> GetAllCompositions()
	{
		if (_AllContentTypeComps.Any())
		{
			return _AllContentTypeComps;
		}
		else
		{
			var doctypes = _services.ContentTypeService!.GetAll();
			var comps = doctypes.SelectMany(n => n.ContentTypeComposition);

			_AllContentTypeComps = comps.DistinctBy(n => n.Id);

			return _AllContentTypeComps;
		}

	}

	/// <summary>
	/// Gets list of all DocTypes on site as AuditableDoctype models
	/// </summary>
	/// <returns></returns>
	public IEnumerable<AuditableDocType> GetAuditableDocTypes()
	{
		var list = new List<AuditableDocType>();

		var doctypes = _services.ContentTypeService!.GetAll();

		foreach (var type in doctypes)
		{
			list.Add(ConvertIContentTypeToAuditableDocType(type));
		}

		return list;
	}

	private AuditableDocType ConvertIContentTypeToAuditableDocType(IContentType ContentType)
	{
		var adt = new AuditableDocType();
		adt.ContentType = ContentType;
		adt.Name = ContentType.Name != null ? ContentType.Name : "UNKNOWN";
		adt.Alias = ContentType.Alias;
		adt.Guid = ContentType.Key;
		adt.Id = ContentType.Id;
		adt.IsElement = ContentType.IsElement;

		if (ContentType.DefaultTemplate != null)
		{
			adt.DefaultTemplateName =
				ContentType.DefaultTemplate.Name != null ? ContentType.DefaultTemplate.Name : "UNKNOWN";
		}
		else
		{
			adt.DefaultTemplateName = "NONE";
		}

		var templates = new Dictionary<int, string>();
		if (ContentType.AllowedTemplates != null)
		{
			foreach (var template in ContentType.AllowedTemplates)
			{
				templates.Add(template.Id, template.Alias);
			}
		}

		adt.AllowedTemplates = templates;

		var hasComps = new Dictionary<int, string>();
		foreach (var comp in ContentType.ContentTypeComposition)
		{
			hasComps.Add(comp.Id, comp.Alias);
		}

		adt.CompositionsUsed = hasComps;

		var allCompsIds = GetAllCompositions().Select(n => n.Id);

		adt.IsComposition = allCompsIds.Contains(ContentType.Id);

		adt.HasContentNodes = _services.ContentTypeService!.HasContentNodes(ContentType.Id);

		adt.FolderPath = GetFolderContainerPath(ContentType);

		return adt;
	}

	private List<string> GetFolderContainerPath(IContentType ContentType)
	{
		var folders = new List<string>();
		var ids = ContentType.Path.Split(',');

		try
		{
			//The final one is the DataType, so exclude it
			foreach (var sId in ids.Take(ids.Length - 1))
			{
				if (sId != "-1")
				{
					var container = _services.ContentTypeService!.GetContainer(Convert.ToInt32(sId));
					if (container != null && container.Name != null)
					{
						folders.Add(container.Name);
					}
				}
			}
		}
		catch (Exception e)
		{
			folders.Add("~ERROR~");
			var msg =
				$"Error in 'GetFolderContainerPath()' for ContentType {ContentType.Id} - '{ContentType.Name}'";
			_logger.LogError(e, msg);
		}

		return folders;
	}


	#endregion

	#region MediaTypes

	public IList<IMediaType> GetAllMediaTypes()
	{
		var list = new List<IMediaType>();

		var types = _services.MediaTypeService!.GetAll();

		foreach (var type in types)
		{
			list.Add(type);
		}

		return list;
	}

	#endregion

	#region AuditableProperties

	public SiteAuditableProperties AllProperties()
	{
		var allProps = new SiteAuditableProperties();
		allProps.PropsForDoctype = "[All]";
		List<AuditableProperty> propertiesList = new List<AuditableProperty>();

		var allDocTypes = _services.ContentTypeService!.GetAll();

		foreach (var docType in allDocTypes)
		{
			//var ct = _services.ContentTypeService.Get(docTypeAlias);

			foreach (var prop in docType.PropertyTypes)
			{
				//test for the same property already in list
				if (propertiesList.Exists(i =>
						i.UmbPropertyType != null
						&& i.UmbPropertyType?.Alias == prop.Alias
						& i.UmbPropertyType?.Name == prop.Name
						& i.UmbPropertyType?.DataTypeId == prop.DataTypeId))
				{
					//Add current DocType to existing property
					var info = new PropertyDoctypeInfo(docType);

					propertiesList.Find(i =>
							i.UmbPropertyType?.Alias == prop.Alias
						)
						?.AllDocTypes.Add(info);
				}
				else
				{
					//Add new property
					AuditableProperty auditProp = PropertyTypeToAuditableProperty(prop);

					var info = new PropertyDoctypeInfo(docType);

					auditProp.AllDocTypes.Add(info);
					propertiesList.Add(auditProp);
				}
			}
		}

		allProps.AllProperties = propertiesList;
		return allProps;
	}

	public SiteAuditableProperties AllPropertiesForDocType(string DocTypeAlias)
	{
		var allProps = new SiteAuditableProperties();
		allProps.PropsForDoctype = DocTypeAlias;
		List<AuditableProperty> propertiesList = new List<AuditableProperty>();

		var ct = _services.ContentTypeService!.Get(DocTypeAlias);
		var propsDone = new List<int>();

		//First, compositions
		if (ct != null)
		{
			foreach (var comp in ct.ContentTypeComposition)
			{
				foreach (var group in comp.CompositionPropertyGroups)
				{
					if (group.PropertyTypes != null)
						foreach (var prop in group.PropertyTypes)
						{
							AuditableProperty auditProp = PropertyTypeToAuditableProperty(prop);

							auditProp.InComposition = comp.Name ?? "";
							auditProp.GroupName = group.Name ?? "";

							propertiesList.Add(auditProp);
							propsDone.Add(prop.Id);
						}
				}
			}

			//Next, non-comp properties
			foreach (var group in ct.CompositionPropertyGroups)
			{
				if (group.PropertyTypes != null)
					foreach (var prop in group.PropertyTypes)
					{
						//check if already added...
						if (!propsDone.Contains(prop.Id))
						{
							AuditableProperty auditProp = PropertyTypeToAuditableProperty(prop);
							auditProp.GroupName = group.Name ?? "";
							auditProp.InComposition = "~NONE";
							propertiesList.Add(auditProp);
						}
					}
			}
		}

		allProps.AllProperties = propertiesList;
		return allProps;
	}



	/// <summary>
	/// Meta data about a Property for Auditing purposes.
	/// </summary>
	/// <param name="UmbPropertyType"></param>
	private AuditableProperty PropertyTypeToAuditableProperty(IPropertyType UmbPropertyType)
	{
		var ap = new AuditableProperty();
		ap.UmbPropertyType = UmbPropertyType;

		ap.DataType = _services.DataTypeService!.GetDataType(UmbPropertyType.DataTypeId);
		ap.DataTypeElementTypes = ap.DataType != null ? GetAllDataTypeElementsList(ap.DataType) : new List<string>();
		ap.DataTypeConfigType = ap.DataType != null && ap.DataType.Configuration != null
			? ap.DataType.Configuration.GetType()
			: null;
		try
		{
			var configDict = (Dictionary<string, string>)ap.DataType.Configuration;
			ap.DataTypeConfigDictionary = configDict;
		}
		catch (Exception e)
		{
			//ignore
			ap.DataTypeConfigDictionary = new Dictionary<string, string>();
		}

		//var  docTypes = AuditHelper.GetDocTypesForProperty(UmbPropertyType.Alias);
		// this.DocTypes = new List<string>();

		//if (ap.DataType.EditorAlias.Contains("NestedContent"))
		//{
		//	ap.IsNestedContent = true;
		//	var config = (NestedContentConfiguration)ap.DataType.Configuration;
		//	//var contentJson = ["contentTypes"];

		//	//var types = JsonConvert
		//	//    .DeserializeObject<IEnumerable<NestedContentContentTypesConfigItem>>(contentJson);
		//	ap.NestedContentDocTypesConfig = config.ContentTypes;
		//}

		return ap;
	}

	/// <summary>
	/// Get a list of all DocTypes which contain a property of a specified Alias
	/// </summary>
	/// <param name="PropertyAlias"></param>
	/// <returns></returns>
	public List<PropertyDoctypeInfo> GetDocTypesForProperty(string PropertyAlias)
	{
		var docTypesList = new List<PropertyDoctypeInfo>();

		var allDocTypes = _services.ContentTypeService!.GetAll();

		foreach (var docType in allDocTypes)
		{
			var matchingProps = docType.CompositionPropertyTypes.Where(n => n.Alias == PropertyAlias).ToList();
			if (matchingProps.Any())
			{
				foreach (var prop in matchingProps)
				{
					var x = new PropertyDoctypeInfo(docType);

					var matchingGroups = docType.PropertyGroups.Where(n => n.PropertyTypes.Contains(prop.Alias))
						.ToList();
					if (matchingGroups != null && matchingGroups.Any())
					{
						x.GroupName = matchingGroups.First().Name!;
					}

					docTypesList.Add(x);
				}
			}
		}

		return docTypesList;
	}

	private Dictionary<IPropertyType, string> PropsWithDocTypes()
	{
		var properties = new Dictionary<IPropertyType, string>();
		var docTypes = _services.ContentTypeService!.GetAll();
		foreach (var doc in docTypes)
		{
			foreach (var prop in doc.PropertyTypes)
			{
				properties.Add(prop, doc.Alias);
			}
		}

		return properties;
	}

	#endregion

	#region AuditableDataTypes

	public IEnumerable<AuditableDataType> AllDataTypes()
	{

		if (_AllDataTypes.Any())
		{
			return _AllDataTypes;
		}

		var list = new List<AuditableDataType>();
		var datatypes = _services.DataTypeService!.GetAll();

		var properties = PropsWithDocTypes();

		foreach (var dt in datatypes)
		{
			var adt = new AuditableDataType();
			adt.Name = dt.Name ?? "";
			adt.EditorAlias = dt.EditorAlias;
			adt.Guid = dt.Key;
			adt.Id = dt.Id;
			adt.FolderPath = GetFolderContainerPath(dt);
			adt.UsesElementsDirectly = GetDirectDataTypeElements(dt);
			adt.UsesElementsAll = GetAllDataTypeElementsList(dt);

			//adt.ConfigurationJson = dt.Configuration!=null? JsonSerializer.Serialize(dt.Configuration): "";
			adt.ConfigurationJson = dt.Configuration != null
				? Newtonsoft.Json.JsonConvert.SerializeObject(dt.Configuration)
				: "";

			var matchingProps = properties.Where(p => p.Key.DataTypeId == dt.Id);
			adt.UsedOnProperties = matchingProps;

			list.Add(adt);
		}

		//cache
		_AllDataTypes = list;

		return list;
	}

	private IEnumerable<DataTypesWithElements> GetAllDataTypesWithElements()
	{
		if (_AllDataTypesWithElements.Any())
		{
			return _AllDataTypesWithElements;
		}

		var list = new List<DataTypesWithElements>();
		var datatypes = _services.DataTypeService.GetAll();

		foreach (var dt in datatypes)
		{
			var directTypes = GetDirectElementsForDataType(dt);
			foreach (var type in directTypes)
			{
				var x = new DataTypesWithElements();
				x.DataTypeName = dt.Name;
				x.DataTypeId = dt.Id;
				x.ElementTypeAlias = type;

				list.Add(x);
			}
		}

		//cache
		_AllDataTypesWithElements = list;

		return list;

	}

	public IEnumerable<AuditableDataType> AllDataTypesUsingElement(string ElementTypeAlias)
	{
		var allDatatypes = AllDataTypes();
		return allDatatypes.Where(n => n.UsesElementsAll.Contains(ElementTypeAlias));
	}

	public IEnumerable<AuditableDataType> AllDataTypesUsingElementDirectly(string ElementTypeAlias)
	{
		var allDatatypes = AllDataTypes();
		return allDatatypes.Where(n => n.UsesElementsDirectly.Contains(ElementTypeAlias));
	}


	private IEnumerable<string> GetAllDataTypeElementsList(IDataType dt)
	{
		var allElementsList = new List<string>();

		var allTypes = GetAllDataTypesWithElements().ToList();

		var directTypes = allTypes.Where(n => n.DataTypeId == dt.Id).Select(n => n.ElementTypeAlias).ToList();
		allElementsList.AddRange(directTypes);

		//Recursive testing - build up a list of all possible related elements
		foreach (var elementTypeAlias in directTypes)
		{
			allElementsList.AddRange(LoopElements(elementTypeAlias, allTypes, new HashSet<string>()));
		}

		return allElementsList.Distinct();
	}

	private IEnumerable<string> GetDirectDataTypeElements(IDataType dt)
	{
		var allElementsList = new List<string>();

		var allTypes = GetAllDataTypesWithElements().ToList();

		var directTypes = allTypes.Where(n => n.DataTypeId == dt.Id).Select(n => n.ElementTypeAlias).ToList();
		allElementsList.AddRange(directTypes);

		return allElementsList.Distinct();
	}

	private IEnumerable<string> LoopElements(string ElementAlias, List<DataTypesWithElements> AllTypes,
		HashSet<string> VisitedElements)
	{
		var elementsList = new List<string>();

		// Check if the element has been visited
		if (VisitedElements.Contains(ElementAlias))
		{
			return elementsList;
		}

		// Mark the element as visited
		VisitedElements.Add(ElementAlias);

		// Process the element
		var docType = _services.ContentTypeService.Get(ElementAlias);
		var properties = docType.PropertyTypes;

		foreach (var property in properties)
		{
			//var dt = _services.DataTypeService.GetDataType();
			var dtElements = AllTypes.Where(n => n.DataTypeId == property.DataTypeId).Select(n => n.ElementTypeAlias)
				.ToList();
			elementsList.AddRange(dtElements);

			foreach (var element in dtElements)
			{
				elementsList.AddRange(LoopElements(element, AllTypes, VisitedElements));
			}
		}

		return elementsList;
	}


	private List<string> UmbracoStandardPropEditors()
	{
		return new List<string>()
		{
			{ "Umbraco.BlockList" },
			{ "Umbraco.CheckBoxList" },
			{ "Umbraco.ColorPicker" },
			{ "Umbraco.Decimal" },
			{ "Umbraco.ColorPicker.EyeDropper" },
			{ "Umbraco.ContentPicker" },
			{ "Umbraco.DateTime" },
			{ "Umbraco.DropDown.Flexible" },
			{ "Umbraco.Grid" },
			{ "Umbraco.ImageCropper" },
			{ "Umbraco.Integer" },
			{ "Umbraco.Label" },
			{ "Umbraco.ListView" },
			{ "Umbraco.MarkdownEditor" },
			{ "Umbraco.MediaPicker" },
			{ "Umbraco.MediaPicker3" },
			{ "Umbraco.MemberPicker" },
			{ "Umbraco.MultiNodeTreePicker" },
			{ "Umbraco.MultipleTextstring" },
			{ "Umbraco.MultiUrlPicker" },
			{ "Umbraco.NestedContent" },
			{ "Umbraco.RadioButtonList" },
			{ "Umbraco.Tags" },
			{ "Umbraco.TextArea" },
			{ "Umbraco.TextBox" },
			{ "Umbraco.TinyMCE" },
			{ "Umbraco.TrueFalse" },
			{ "Umbraco.UploadField" },
			{ "UmbracoForms.FormPicker" },
			{ "UmbracoForms.ThemePicker" }
		};

	}

	private List<string> PackagePropEditorsWithoutDocTypeConfig()
	{
		return new List<string>()
		{
			{ "Umbraco.Community.Contentment.CodeEditor" },
			{ "Umbraco.Community.Contentment.DataList" },
			{ "Umbraco.Community.Contentment.TemplatedLabel" },
			{ "Umbraco.Community.Contentment.RenderMacro" },
			{ "Umbraco.Community.Contentment.EditorNotes" },
			{ "Dragonfly.Theming.ThemePicker" },
			{ "Dragonfly.Theming.CssOverridePicker" },
			{ "our.iconic" },
			{ "Dawoe.OEmbedPickerPropertyEditor" },
			{ "Vokseverk.KeyValueEditor" }

		};
	}

	private List<string> ConfigKeysRepresentingDocType()
	{
		return new List<string>()
		{
			{ "doctype" },
			{ "doctypes" },
			{ "contenttype" },
			{ "contenttypes" },
			{ "element" },
			{ "elements" },
			{ "elementtype" },
			{ "elementtypes" }
		};
	}


	private IEnumerable<string> GetDirectElementsForDataType(IDataType dt)
	{
		var elementTypesList = new List<string>();

		if (dt.Configuration is null)
		{
			return elementTypesList;
		}

		try
		{
			string configJson = dt.Configuration != null ? JsonConvert.SerializeObject(dt.Configuration) : "";
			List<string> keyStrGuids = new List<string>();
			List<Guid> keyGuids = new List<Guid>();

			//Known PropertyEditors with Config Models representing DocTypes
			switch (dt.EditorAlias)
			{
				//Legacy built-in - [Obsolete("The grid is obsolete, will be removed in V13")]
				case "Umbraco.Grid":
					//Would need to read the global grideditors.config and recognize doctypealiases
					//- "IGridConfig config" passed-in via DI (ex: https://github.com/umbraco/Umbraco-CMS/blob/93415a9957f5a0f4dd057de56f0cd9520ac9313e/src/Umbraco.Infrastructure/PropertyEditors/ValueConverters/GridValueConverter.cs#L21)

					//	{"Items":{"styles":[], "config":[], "columns":12, "templates":[{"name":"Full", "sections":[{"grid":12}]}], "layouts":[{"name":"2 Columns", "areas":[{"grid":6, "allowAll":false, "allowed":["LinkList", "macro"]}, {"grid":6, "allowAll":false, "allowed":["LinkList", "macro"]}]}, {"name":"3 Columns", "areas":[{"grid":4, "allowAll":false, "allowed":["LinkList", "macro"]}, {"grid":4, "allowAll":false, "allowed":["LinkList", "macro"]}, {"grid":4, "allowAll":false, "allowed":["LinkList", "macro"]}]}, {"name":"4 Columns", "areas":[{"grid":3, "allowAll":false, "allowed":["LinkList", "macro"]}, {"grid":3, "allowAll":false, "allowed":["LinkList", "macro"]}, {"grid":3, "allowAll":false, "allowed":["LinkList", "macro"]}, {"grid":3, "allowAll":false, "allowed":["LinkList", "macro"]}]}, {"name":"6 Columns", "areas":[{"grid":2, "allowAll":false, "allowed":["LinkList", "macro"]}, {"grid":2, "allowAll":false, "allowed":["LinkList", "macro"]}, {"grid":2, "allowAll":false, "allowed":["LinkList", "macro"]}, {"grid":2, "allowAll":false, "allowed":["LinkList", "macro"]}, {"grid":2, "allowAll":false, "allowed":["LinkList", "macro"]}, {"grid":2, "allowAll":false, "allowed":["LinkList", "macro"]}]}]}, "Rte":{"toolbar":[], "stylesheets":["umbraco-rte"], "dimensions":{"height":500}, "maxImageSize":500}, "MediaParentId":null, "IgnoreUserStartNodes":false}
					//var gridConfig =(Umbraco.Cms.Core.PropertyEditors.NestedContentConfiguration)dt.Configuration; JsonSerializer.Deserialize<UmbracoGridConfig>(configJson);
					//if (gridConfig != null)
					//{
					//	var areas = gridConfig.Items.Layouts.SelectMany(l => l.Areas);
					//	var x = areas.Select(a=> a.Allowed.)
					//}
					break;

				//Legacy built-in [Obsolete("Nested content is obsolete, will be removed in V13")]
				case "Umbraco.NestedContent":
					var nestedContentConfig =
						(Umbraco.Cms.Core.PropertyEditors.NestedContentConfiguration)dt
							.Configuration; //JsonSerializer.Deserialize<NestedContentConfig>(configJson);
					if (nestedContentConfig != null && nestedContentConfig.ContentTypes != null)
					{
						var types = nestedContentConfig.ContentTypes.ToList();
						elementTypesList.AddRange(types.Select(c => c.Alias));
					}

					break;

				//Current built-in
				case "Umbraco.BlockList":
					var blockListConfig =
						(Umbraco.Cms.Core.PropertyEditors.BlockListConfiguration)dt
							.Configuration; //JsonSerializer.Deserialize<UmbracoBlockListConfig>(configJson);
					if (blockListConfig != null)
					{
						keyGuids.AddRange(blockListConfig.Blocks.Select(b => b.ContentElementTypeKey));
						var settingsKeys = blockListConfig.Blocks.Where(b => b.SettingsElementTypeKey != null)
							.Select(b => b.SettingsElementTypeKey).ToList();

						if (settingsKeys.Any())
						{
							foreach (var guid in settingsKeys)
							{
								keyGuids.Add(guid.Value);
							}
						}
					}

					break;

				//Current built-in
				case "Umbraco.BlockGrid":
					//	var blockGridConfig = (Umbraco.Cms.Core.PropertyEditors.BlockEditorPropertyEditor)dt.Configuration;//JsonSerializer.Deserialize<UmbracoBlockListConfig>(configJson);
					//if (blockGridConfig != null)
					//{
					//}
					_logger.LogWarning(
						$"SiteAuditorService.GetDataTypeElementsList: Unknown Editor '{dt.EditorAlias}' with Config Model {dt.Configuration.ToString()} - needs processing?");

					break;

				//Packages
				case "SimpleTreeMenu":
					JsonNode? stmConfig = JsonConvert.DeserializeObject<JsonNode>(configJson);
					if (stmConfig != null)
					{
						if (stmConfig["doctype"] != null)
						{
							var docType = stmConfig["doctype"].GetValue<string>();
							elementTypesList.Add(docType);
						}
					}
					/*public partial class SimpleTreeMenuConfig
						{
							[JsonPropertyName("doctype")]
							public string Doctype { get; set; }

							[JsonPropertyName("nameTemplate")]
							public string NameTemplate { get; set; }

							[JsonPropertyName("levels")]
							public long Levels { get; set; }
						}*/
					//{"doctype":"ElementNavItem", "nameTemplate":"{{$index + 1}}. {{DisplayTitle ? DisplayTitle : Link[0][\"name\"] ? Link[0][\"name\"]: \"NONE\"}}", "levels":3}


					break;

				default: //Unknown PropertyEditors, look for clues

					if (dt.Configuration is Dictionary<string, object>)
					{
						//If there is a dictionary of config keys, look for doc type-related keys

						var configDict = (Dictionary<string, object>)dt.Configuration;

						//Check for likely key names
						var keyFound = false;
						foreach (var keyName in ConfigKeysRepresentingDocType())
						{
							if (configDict.ContainsKey(keyName))
							{
								keyFound = true;
								var configVal = configDict[keyName];

								var guidVals = new List<Guid>();

								if (configVal is IEnumerable<string>)
								{
									elementTypesList.AddRange((IEnumerable<string>)configVal);
								}
								else if (configVal is string)
								{
									elementTypesList.Add((string)configVal);
								}
								else if (configVal is IEnumerable<Guid>)
								{
									guidVals.AddRange((IEnumerable<Guid>)configVal);
								}
								else if (configVal is Guid)
								{
									guidVals.Add((Guid)configVal);
								}
								else
								{
									_logger.LogWarning(
										$"SiteAuditorService.GetDataTypeElementsList: Unknown Editor '{dt.EditorAlias}' includes config key '{keyName}' of unprocessed type {configVal.GetType().ToString()}");
								}

								//Lookup any GUIDs
								if (guidVals.Any())
								{
									foreach (var guid in guidVals)
									{
										var contentType = _services.ContentTypeService.Get(guid);
										if (contentType != null)
										{
											elementTypesList.Add(contentType.Alias);
										}
									}
								}
							}
						}

						if (!keyFound)
						{
							//If not a standard prop editor... or known prop editor
							if (!UmbracoStandardPropEditors().Contains(dt.EditorAlias) &&
								!PackagePropEditorsWithoutDocTypeConfig().Contains(dt.EditorAlias))
							{
								_logger.LogWarning(
									$"SiteAuditorService.GetDataTypeElementsList: Unknown Editor '{dt.EditorAlias}' includes unprocessed config keys {string.Join(", ", configDict.Keys)}");
							}
						}
					}
					else if (!UmbracoStandardPropEditors().Contains(dt.EditorAlias) &&
							 !PackagePropEditorsWithoutDocTypeConfig().Contains(dt.EditorAlias))
					{
						//If not a standard prop editor... or known editor

						_logger.LogWarning(
							$"SiteAuditorService.GetDataTypeElementsList: Unknown Editor '{dt.EditorAlias}' with Config Model {dt.Configuration.ToString()} - needs processing?");
					}

					break;
			}

			//Process any located GUIDs into content types
			if (keyStrGuids.Any())
			{
				foreach (var sGuid in keyStrGuids)
				{
					var guid = Guid.Parse(sGuid);
					keyGuids.Add(guid);
				}
			}

			if (keyGuids.Any())
			{
				foreach (var guid in keyGuids)
				{
					var contentType = _services.ContentTypeService.Get(guid);
					if (contentType != null)
					{
						elementTypesList.Add(contentType.Alias);
					}
				}
			}

		}
		catch (Exception e)
		{
			_logger.LogError(e,
				$"SiteAuditorService.GetDataTypeElementsList: Error on '{dt.EditorAlias}' with config: {dt.Configuration}");
		}

		return elementTypesList.Distinct();
	}

	private List<string> GetFolderContainerPath(IDataType DataType)
	{
		var folders = new List<string>();
		var ids = DataType.Path.Split(',');

		try
		{
			//The final one is the DataType, so exclude it
			foreach (var sId in ids.Take(ids.Length - 1))
			{
				if (sId != "-1")
				{
					var container = _services.DataTypeService.GetContainer(Convert.ToInt32(sId));
					folders.Add(container.Name);
				}
			}
		}
		catch (Exception e)
		{
			folders.Add("~ERROR~");
			var msg = $"Error in 'GetFolderContainerPath()' for DataType {DataType.Id} - '{DataType.Name}'";
			_logger.LogError(e, msg);
		}

		return folders;
	}



	#endregion

	#region Special Queries

	public GroupingCollection<AuditableContent> TemplatesUsedOnContent()
	{
		var allContent = GetContentNodes(false);
		var allContentTemplates =
			new GroupingCollection<AuditableContent>(allContent); //allContent.Select(n => n.TemplateAlias);
		allContentTemplates.GroupItems(n => n.TemplateAlias);
		return allContentTemplates; //.GroupBy(n => n);
	}

	public List<ITemplate> TemplatesNotUsedOnContent()
	{
		var allTemplates = _services.FileService.GetTemplates();
		//var allTemplateAliases = _services.FileService.GetTemplates().Select(n => n.Alias).ToList();

		var allContent = GetContentNodes(false);

		var contentTemplatesInUse = allContent.Select(n => n.TemplateAlias).Distinct().ToList();

		var templatesWithoutContent = allTemplates.Where(t => !contentTemplatesInUse.Contains(t.Alias)).ToList();

		return templatesWithoutContent.OrderBy(n => n.Alias).ToList();

	}

	#endregion

	#region AuditableTemplates

	public IEnumerable<AuditableTemplate> GetAuditableTemplates()
	{
		var list = new List<AuditableTemplate>();
		var templates = _services.FileService.GetTemplates();

		var content = GetContentNodes(false).ToList();
		var docTypes = GetAllDocTypes().ToList();

		foreach (var temp in templates)
		{
			var at = new AuditableTemplate();
			at.Name = temp.Name;
			at.Alias = temp.Alias;
			at.Guid = temp.Key;
			at.Id = temp.Id;
			at.Udi = temp.GetUdi();
			at.FolderPath = GetFolderContainerPath(temp);
			at.IsMaster = temp.IsMasterTemplate;
			at.HasMaster = temp.MasterTemplateAlias ?? "";
			at.CodeLength = temp.Content != null ? temp.Content.Length : 0;
			at.CreateDate = temp.CreateDate;
			at.UpdateDate = temp.UpdateDate;
			at.OriginalPath = temp.OriginalPath;
			//at.XXX = temp.;
			//at.XXX = temp.UpdateDate;
			//at.ConfigurationJson = JsonConvert.SerializeObject(temp.Configuration);

			var matchingContent = content.Where(p => p.TemplateAlias == temp.Alias);
			at.UsedOnContent = matchingContent.Count();

			var doctypesAllowed = docTypes.Where(n => n.IsAllowedTemplate(temp.Id));
			if (doctypesAllowed.Any())
			{
				at.IsAllowedOn = doctypesAllowed;
			}
			else
			{
				at.IsAllowedOn = new List<IContentType>();
			}

			var doctypeDefault = docTypes.Where(n => n.DefaultTemplate != null && n.DefaultTemplate.Id == temp.Id);
			if (doctypeDefault.Any())
			{
				at.DefaultTemplateFor = doctypeDefault;
			}
			else
			{
				at.DefaultTemplateFor = new List<IContentType>();
			}

			list.Add(at);
		}

		return list;
	}

	private List<string> GetFolderContainerPath(ITemplate Template)
	{
		var folders = new List<string>();
		var ids = Template.Path.Split(',');

		try
		{
			//The final one is the current item, so exclude it
			foreach (var sId in ids.Take(ids.Length - 1))
			{
				if (sId != "-1")
				{
					var container = _services.FileService.GetTemplate(Convert.ToInt32(sId));
					folders.Add(container.Name);
				}
			}
		}
		catch (Exception e)
		{
			folders.Add("~ERROR~");
			var msg = $"Error in 'GetFolderContainerPath()' for Template {Template.Id} - '{Template.Name}'";
			_logger.LogError(e, msg);
		}

		return folders;
	}

	private string GetTemplateAlias(IContent Content)
	{
		string templateAlias = "NONE";
		if (Content.TemplateId != null)
		{
			var template = _services.FileService.GetTemplate((int)Content.TemplateId);
			templateAlias = template.Alias;
		}

		return templateAlias;
	}

	private string GetTemplateAlias(IPublishedContent Content)
	{
		string templateAlias = "NONE";
		if (Content.TemplateId != null)
		{
			var template = _services.FileService.GetTemplate((int)Content.TemplateId);
			templateAlias = template.Alias;
		}

		return templateAlias;
	}

	#endregion

	#region Serilog Items

	internal List<SerilogItem> GetLogsBetweenDates(string directoryPath, DateTime startDate, DateTime endDate)
	{
		var logEntries = new List<SerilogItem>();

		foreach (var filePath in Directory.GetFiles(directoryPath, "*.json"))
		{
			var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();
			var fileDate = ExtractDateFromSerilogFilename(fileName);

			//Test file date against start and end dates before parsing
			if (fileDate >= startDate && fileDate <= endDate)
			{
				using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (TextReader textReader = new StreamReader(stream))
				{
					using var reader = new LogEventReader(textReader);
					while (reader.TryRead(out var logEvent))
					{
						//var logEntry = SerilogItem.FromJson(line);
						var logEntry = new SerilogItem(logEvent);
						if (logEntry.Timestamp >= startDate && logEntry.Timestamp <= endDate)
						{
							logEntry.FileName = fileName;
							logEntry.FileDate = fileDate;
							logEntries.Add(logEntry);
						}
					}
				}
			}
		}

		return logEntries;
	}

	private static DateTime? ExtractDateFromSerilogFilename(string Filename)
	{
		var match = Regex.Match(Filename, @"\.(\d+)\.json$");
		if (match.Success)
		{
			var dateDigits = match.Groups[1].Value;

			var date = DateTime.ParseExact(dateDigits, "yyyyMMdd", CultureInfo.InvariantCulture);

			return date;
		}
		else
		{
			return null;
		}
	}

	#endregion

	#region Helpers

	public List<IPublishedContent> GetMediaListFromPropValue(string Editor, IReadOnlyCollection<IPropertyValue> PropertyValues, out string ErrorMessage)
	{
		var multiMedia = new List<IPublishedContent>();
		ErrorMessage = "";

		if (PropertyValues!=null && !PropertyValues.Any())
		{
			return multiMedia;
		}

		foreach (var propertyValue in PropertyValues)
		{
			var propValue = propertyValue.PublishedValue;
			if (propValue == null || propValue.ToString() == "[]")
			{
				return multiMedia;
			}

			//Values, figure out what type
			var propValueType = propValue.GetType();
			if (propValueType == typeof(IPublishedContent))
			{
				try
				{
					var media = (IPublishedContent)propValue;
					if (media != null)
					{
						multiMedia.Add(media);
					}
					else
					{
						//No matching media
						ErrorMessage = $"Unable to convert IPublishedContent '{propValue.ToString()}' to Media";
					}
				}
				catch (Exception e)
				{
					ErrorMessage = $"{@e.Message} on '{propValue.ToString()}' data format (1)";
				}
			}
			else if (propValueType == typeof(string))
			{
				var stringData = propValue.ToString();
				stringData = stringData ?? "";

				if (stringData.StartsWith("[{") && Editor == "Umbraco.MediaPicker3")
				{
					try
					{
						var mediaWCrops = JsonConvert.DeserializeObject<IEnumerable<RawMediaWithCrops>>(stringData).ToList();
						foreach (var item in mediaWCrops)
						{
							var media = _umbracoHelper.Media(item.GetMediaKey());
							if (media != null)
							{
								multiMedia.Add(media);
							}
							else
							{
								//No matching media
								ErrorMessage = $"Unable to find a Media with Key '{item.MediaKey}'";
							}
						}
					}
					catch (Exception e)
					{
						//Error
						ErrorMessage = $"{e.Message} - Unable to convert '{stringData}' data to MediaWithCrops for Umbraco.MediaPicker3";
					}
				}
				else if (stringData.StartsWith("["))
				{
					var listOfStrings = JsonConvert.DeserializeObject<IEnumerable<string>>(stringData);
					foreach (var s in listOfStrings)
					{
						if (s.StartsWith("umb://"))
						{
							//UDI value
							var media = _umbracoHelper.Media(s);
							if (media != null)
							{
								multiMedia.Add(media);
							}
							else
							{
								//No matching media
								ErrorMessage = $"Unable to find a Media with UDI '{s}'";
							}
						}
						else
						{
							//Not sure what this is...
							ErrorMessage = $"Need to process '{s}' data format (2)";
						}
					}
				}
				else if (stringData.StartsWith("umb://"))
				{
					try //UDI
					{
						var media = _umbracoHelper.Media(stringData);
						if (media != null)
						{
							multiMedia.Add(media);
						}
						else
						{
							//No matching media
							ErrorMessage = $"Unable to find a Media with UDI '{stringData}'";
						}
					}
					catch (Exception e)
					{
						//Error
						ErrorMessage = $"{e.Message} - Unable to convert '{stringData}' data to Media item";
					}
				}
				else
				{
					//Not sure what data format this is
					ErrorMessage = $"Need to process '{stringData}' data format (3)";
				}
			}
			else
			{
				//Other Type
				ErrorMessage = $"Need to process values of type {propValueType} like '{propValue.ToString()}'";
			}
		}

		return multiMedia;
	}

	

	public IList<string> GetImageSrcFromPropValue(string Editor, IReadOnlyCollection<IPropertyValue> PropertyValues, out string ErrorMessage)
	{
		var imgSrcs = new List<string>();
		ErrorMessage = "";

		if (PropertyValues!=null && !PropertyValues.Any())
		{
			return imgSrcs;
		}

		foreach (var propertyValue in PropertyValues)
		{
			var propValue = propertyValue.PublishedValue;
			if (propValue == null || propValue.ToString() == "[]" || propValue.ToString() == "{}")
			{
				return imgSrcs;
			}

			//Values, figure out what type
			var propValueType = propValue.GetType();
			if (propValueType == typeof(string))
			{
				var stringData = propValue.ToString();
				stringData = stringData ?? "";

				if (stringData.StartsWith("{") && Editor == "Umbraco.ImageCropper")
				{
					try
					{
						var imgCropper = JsonConvert.DeserializeObject<ImageCropperValue>(stringData);
						if (imgCropper != null)
						{
							if (imgCropper.Src != null)
							{
								imgSrcs.Add(imgCropper.Src);
							}
							else
							{
								//No matching media
								ErrorMessage = $"No Image Source provided";
							}
						}
					}
					catch (Exception e)
					{
						//Error
						ErrorMessage =
							$"{e.Message} - Unable to convert '{stringData}' data to ImageCropperValue for Umbraco.ImageCropper";
					}
				}
			}
		}

		return imgSrcs;
	}

	public static string GetAspectRatio(int width, int height)
	{
		// Handle edge cases for width and height
		if (width == 0 && height == 0)
		{
			return "NONE";
		}

		int gcd = GCD(width, height);
		if (gcd == 0)
		{
			// Handle case where gcd is 0, which means width or height is 0
			// Return a default value to avoid division by zero
			return "UNKNOWN";
		}

		int aspectWidth = width / gcd;
		int aspectHeight = height / gcd;
		return $"{aspectWidth}:{aspectHeight}";
	}

	private static int GCD(int a, int b)
	{
		while (b != 0)
		{
			int temp = b;
			b = a % b;
			a = temp;
		}
		return a;
	}

	#endregion


}

public class DataTypesWithElements
{
	public string? DataTypeName { get; set; }
	public int DataTypeId { get; set; }

	public string ElementTypeAlias { get; set; }
}
