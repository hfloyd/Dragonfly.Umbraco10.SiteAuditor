﻿namespace Dragonfly.SiteAuditor.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dragonfly.SiteAuditor.Models;
using Dragonfly.NetHelperServices;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common;
using Umbraco.Extensions;
using Umbraco.Cms.Infrastructure.PublishedCache;

public class AuditorInfoService
{
	#region CTOR /DI

	private readonly UmbracoHelper _umbracoHelper;
	private readonly ILogger _logger;

	private readonly IUmbracoContextAccessor _umbracoContextAccessor;
	private readonly IUmbracoContext? _umbracoContext;
	private readonly ServiceContext _services;
	private readonly FileHelperService _FileHelperService;
	private readonly HttpContext _Context;
	private readonly IHostingEnvironment _HostingEnvironment;
	private readonly DependencyLoader _Dependencies;

	private bool _HasUmbracoContext;

	public AuditorInfoService(
		DependencyLoader dependencies,
		ILogger<SiteAuditorService> logger
		)
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

	}



	#endregion

	#region Public Props
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

	#region NodePath

	/// <summary>
	/// Takes an Umbraco content node and returns the full "path" to it using ancestor Node Names
	/// </summary>
	/// <param name="UmbContentNode">
	/// The Umbraco Content Node.
	/// </param>
	/// <returns>
	/// list of strings representing ancestor node names
	/// </returns>
	public IEnumerable<string> NodePath(IContent UmbContentNode)
	{
		var nodepathList = new List<string>();
		string pathIdsCsv = UmbContentNode.Path;
		string[] pathIdsArray = pathIdsCsv.Split(',');

		foreach (var sId in pathIdsArray)
		{
			if (sId != "-1")
			{
				IContent? getNode = _services.ContentService!.GetById(Convert.ToInt32(sId));
				if (getNode != null)
				{
					var nodeName = getNode.Name;
					if (nodeName != null)
					{
						nodepathList.Add(nodeName);
					}
				}
				else
				{
					nodepathList.Add($"MISSING [{sId}]");
				}
			}
		}

		return nodepathList;
	}

	/// <summary>
	/// Takes an Umbraco media node and returns the full "path" to it using ancestor Node Names
	/// </summary>
	/// <param name="UmbMediaNode">
	/// The Umbraco Media Node.
	/// </param>
	/// <returns>
	/// list of strings representing ancestor node names
	/// </returns>
	public IEnumerable<string> NodePath(IMedia UmbMediaNode)
	{
		var nodepathList = new List<string>();
		string pathIdsCsv = UmbMediaNode.Path;
		string[] pathIdsArray = pathIdsCsv.Split(',');

		foreach (var sId in pathIdsArray)
		{
			if (sId != "-1")
			{
				IMedia? getNode = _services.MediaService!.GetById(Convert.ToInt32(sId));
				if (getNode != null)
				{
					var nodeName = getNode.Name;
					if (nodeName != null)
					{
						nodepathList.Add(nodeName);
					}
				}
				else
				{
					nodepathList.Add($"MISSING [{sId}]");
				}
			}
		}

		return nodepathList;
	}

	public IEnumerable<string> MediaNodePath(IPublishedContent IPubMedia)
	{
		var nodepathList = new List<string>();
		string pathIdsCsv = IPubMedia.Path;
		string[] pathIdsArray = pathIdsCsv.Split(',');

		foreach (var sId in pathIdsArray)
		{
			if (sId != "-1")
			{
				IPublishedContent? getNode = _umbracoHelper.Media(Convert.ToInt32(sId));
				if (getNode != null)
				{
					var nodeName = getNode.Name;
					if (nodeName != null)
					{
						nodepathList.Add(nodeName);
					}
				}
				else
				{
					nodepathList.Add($"MISSING [{sId}]");
				}
			}
		}

		return nodepathList;
	}

	public IEnumerable<string> PublishedContentNodePath(IPublishedContent IPub)
	{
		var nodepathList = new List<string>();
		string pathIdsCsv = IPub.Path;
		string[] pathIdsArray = pathIdsCsv.Split(',');

		foreach (var sId in pathIdsArray)
		{
			if (sId != "-1")
			{
				IPublishedContent? getNode = _umbracoHelper.Content(Convert.ToInt32(sId));
				if (getNode != null)
				{
					var nodeName = getNode.Name;
					if (nodeName != null)
					{
						nodepathList.Add(nodeName);
					}
				}
				else
				{
					nodepathList.Add($"MISSING [{sId}]");
				}
			}
		}

		return nodepathList;
	}

	/// <summary>
	/// Takes an Umbraco content node and returns the full "path" to it using ancestor Node Names
	/// </summary>
	/// <param name="Content"></param>
	/// <returns>single concatenated string, using default delimiter</returns>
	public string NodePathAsCustomText(IContent Content)
	{
		var paths = NodePath(Content);
		var nodePath = string.Join(_defaultDelimiter, paths);
		return nodePath;
	}

	/// <summary>
	/// Takes an Umbraco content node and returns the full "path" to it using ancestor Node Names
	/// </summary>
	/// <param name="Content"></param>
	/// <param name="Separator"></param>
	/// <returns>single concatenated string, using provided 'Separator' string as delimiter</returns>
	public string NodePathAsCustomText(IContent Content, string Separator)
	{
		var paths = NodePath(Content);
		var nodePath = string.Join(Separator, paths);
		return nodePath;
	}

	#endregion

	#region Node Property Data

	/// <summary>
	/// Get a NodePropertyDataTypeInfo model for a specified Node and Property Alias
	/// (Includes information about the Property, Datatype, and the node's property Value)
	/// </summary>
	/// <param name="PropertyAlias"></param>
	/// <param name="Node">IPublishedContent Node</param>
	/// <returns></returns>
	public NodePropertyDataTypeInfo GetPropertyDataTypeInfo(string PropertyAlias, IPublishedContent? PubNode)
	{
		var umbContentService = _services.ContentService;
		var umbContentTypeService = _services.ContentTypeService;
		var umbDataTypeService = _services.DataTypeService;

		var dtInfo = new NodePropertyDataTypeInfo();

		if (PubNode != null)
		{
			dtInfo.NodeId = PubNode.Id;

			//Get Property
			var content = umbContentService!.GetById(PubNode.Id);
			if (content != null)
			{
				dtInfo.Property = content.Properties.First(n => n.Alias == PropertyAlias);
				dtInfo.PropertyData = PubNode.Value(PropertyAlias);
			}

			//Find datatype of property
			IDataType? dataType = null;

			var docType = umbContentTypeService!.Get(PubNode.ContentType.Id);
			if (docType != null)
			{
				var matchingProperties = docType.PropertyTypes.Where(n => n.Alias == PropertyAlias).ToList();

				if (matchingProperties.Any())
				{
					var propertyType = matchingProperties.First();
					dataType = umbDataTypeService!.GetDataType(propertyType.DataTypeId);

					dtInfo.DataType = dataType;
					if (dataType != null)
					{
						dtInfo.PropertyEditorAlias = dataType.EditorAlias;
						dtInfo.DatabaseType = dataType.DatabaseType.ToString();
					}

					dtInfo.DocTypeAlias = PubNode.ContentType.Alias;
				}
				else
				{
					//Look at Compositions for prop data
					var matchingCompProperties =
						docType.CompositionPropertyTypes.Where(n => n.Alias == PropertyAlias).ToList();
					if (matchingCompProperties.Any())
					{
						var propertyType = matchingCompProperties.First();
						dataType = umbDataTypeService!.GetDataType(propertyType.DataTypeId);

						dtInfo.DataType = dataType;
						if (dataType != null)
						{
							dtInfo.PropertyEditorAlias = dataType.EditorAlias;
							dtInfo.DatabaseType = dataType.DatabaseType.ToString();
						}

						if (docType.ContentTypeComposition.Any())
						{
							var compsList = docType.ContentTypeComposition
								.Where(n => n.PropertyTypeExists(PropertyAlias)).ToList();
							if (compsList.Any())
							{
								dtInfo.DocTypeAlias = PubNode.ContentType.Alias;
								dtInfo.DocTypeCompositionAlias = compsList.First().Alias;
							}
							else
							{
								dtInfo.DocTypeAlias = PubNode.ContentType.Alias;
								dtInfo.DocTypeCompositionAlias = "Unknown Composition";
							}
						}
					}
					else
					{
						dtInfo.ErrorMessage =
							$"No property found for alias '{PropertyAlias}' in DocType '{docType.Name}'";
					}
				}
			}
		}
		else
		{
			_logger.LogError($"AuditorInfoService.GetPropertyDataTypeInfo: PubNode is null");
		}

		return dtInfo;
	}

	/// <summary>
	/// Get a NodePropertyDataTypeInfo model for a specified Node and Property Alias
	/// (Includes information about the Property, Datatype, and the node's property Value)
	/// </summary>
	/// <param name="PropertyAlias"></param>
	/// <param name="Node">IContent Node</param>
	/// <returns></returns>
	public NodePropertyDataTypeInfo GetPropertyDataTypeInfo(string PropertyAlias, IContent? Node)
	{
		var umbContentService = _services.ContentService;
		var umbContentTypeService = _services.ContentTypeService;
		var umbDataTypeService = _services.DataTypeService;

		var dtInfo = new NodePropertyDataTypeInfo();

		if (Node != null)
		{
			dtInfo.NodeId = Node.Id;

			//Get Property
			//var content = umbContentService.GetById(Node.Id);
			var propMatches = Node.Properties.Where(n => n.Alias == PropertyAlias).ToList();
			if (propMatches.Any())
			{
				dtInfo.Property = propMatches.First();
				dtInfo.PropertyData = Node.GetValue(PropertyAlias);
			}
			else
			{
				dtInfo.ErrorMessage =
					$"No property found for alias '{PropertyAlias}' in ContentNode '{Node.Name}'";
			}

			//Find datatype of property
			IDataType? dataType = null;

			var docType = umbContentTypeService!.Get(Node.ContentType.Id);
			dtInfo.DocTypeAlias = Node.ContentType.Alias;

			IPropertyType? propertyType = null;

			if (docType != null)
			{
				var matchingProperties = docType.PropertyTypes.Where(n => n.Alias == PropertyAlias).ToList();
				if (matchingProperties.Any())
				{
					propertyType = matchingProperties.First();
				}
				else
				{
					//Look at Compositions for prop data
					var matchingCompProperties =
						docType.CompositionPropertyTypes.Where(n => n.Alias == PropertyAlias).ToList();
					if (matchingCompProperties.Any())
					{
						propertyType = matchingCompProperties.First();

						if (docType.ContentTypeComposition.Any())
						{
							var compsList = docType.ContentTypeComposition
								.Where(n => n.PropertyTypeExists(PropertyAlias)).ToList();
							if (compsList.Any())
							{
								dtInfo.DocTypeAlias = Node.ContentType.Alias;
								dtInfo.DocTypeCompositionAlias = compsList.First().Alias;
							}
							else
							{
								dtInfo.DocTypeAlias = Node.ContentType.Alias;
								dtInfo.DocTypeCompositionAlias = "Unknown Composition";
							}
						}
					}
					else
					{
						//Look at NoGroupPropertyTypes for prop data
						var matchingNoGroupProperties =
							docType.NoGroupPropertyTypes.Where(n => n.Alias == PropertyAlias).ToList();
						if (matchingNoGroupProperties.Any())
						{
							propertyType = matchingNoGroupProperties.First();
						}
						else
						{
							dtInfo.ErrorMessage =
								$"No property found for alias '{PropertyAlias}' in DocType '{docType.Name}'";
						}
					}
				}
			}

			if (propertyType != null)
			{
				dataType = umbDataTypeService!.GetDataType(propertyType.DataTypeId);

				dtInfo.DataType = dataType;
				if (dataType != null)
				{
					dtInfo.PropertyEditorAlias = dataType.EditorAlias;
					dtInfo.DatabaseType = dataType.DatabaseType.ToString();
				}
			}
		}

		return dtInfo;
	}

	/// <summary>
	/// Get a NodePropertyDataTypeInfo model for a specified Node and Property Alias
	/// (Includes information about the Property, Datatype, and the node's property Value)
	/// </summary>
	/// <param name="PropertyAlias"></param>
	/// <param name="Node">IMedia Node</param>
	/// <returns></returns>
	public NodePropertyDataTypeInfo GetPropertyDataTypeInfo(string PropertyAlias, IMedia? Node)
	{
		var umbContentTypeService = _services.MediaTypeService;
		var umbDataTypeService = _services.DataTypeService;

		var dtInfo = new NodePropertyDataTypeInfo();

		if (Node != null)
		{
			dtInfo.NodeId = Node.Id;

			//Get Property
			//var content = umbContentService.GetById(Node.Id);
			var propMatches = Node.Properties.Where(n => n.Alias == PropertyAlias).ToList();
			if (propMatches.Any())
			{
				dtInfo.Property = propMatches.First();
				dtInfo.PropertyData = Node.GetValue(PropertyAlias);
			}
			else
			{
				dtInfo.ErrorMessage =
					$"No property found for alias '{PropertyAlias}' in ContentNode '{Node.Name}'";
			}

			//Find datatype of property
			IDataType? dataType = null;

			var docType = umbContentTypeService!.Get(Node.ContentType.Id);
			dtInfo.DocTypeAlias = Node.ContentType.Alias;

			IPropertyType? propertyType = null;

			if (docType != null)
			{
				var matchingProperties = docType.PropertyTypes.Where(n => n.Alias == PropertyAlias).ToList();
				if (matchingProperties.Any())
				{
					propertyType = matchingProperties.First();
				}
				else
				{
					//Look at Compositions for prop data
					var matchingCompProperties =
						docType.CompositionPropertyTypes.Where(n => n.Alias == PropertyAlias).ToList();
					if (matchingCompProperties.Any())
					{
						propertyType = matchingCompProperties.First();

						if (docType.ContentTypeComposition.Any())
						{
							var compsList = docType.ContentTypeComposition
								.Where(n => n.PropertyTypeExists(PropertyAlias)).ToList();
							if (compsList.Any())
							{
								dtInfo.DocTypeAlias = Node.ContentType.Alias;
								dtInfo.DocTypeCompositionAlias = compsList.First().Alias;
							}
							else
							{
								dtInfo.DocTypeAlias = Node.ContentType.Alias;
								dtInfo.DocTypeCompositionAlias = "Unknown Composition";
							}
						}
					}
					else
					{
						//Look at NoGroupPropertyTypes for prop data
						var matchingNoGroupProperties =
							docType.NoGroupPropertyTypes.Where(n => n.Alias == PropertyAlias).ToList();
						if (matchingNoGroupProperties.Any())
						{
							propertyType = matchingNoGroupProperties.First();
						}
						else
						{
							dtInfo.ErrorMessage =
								$"No property found for alias '{PropertyAlias}' in DocType '{docType.Name}'";
						}
					}
				}
			}

			if (propertyType != null)
			{
				dataType = umbDataTypeService!.GetDataType(propertyType.DataTypeId);

				dtInfo.DataType = dataType;
				if (dataType != null)
				{
					dtInfo.PropertyEditorAlias = dataType.EditorAlias;
					dtInfo.DatabaseType = dataType.DatabaseType.ToString();
				}
			}
		}

		return dtInfo;
	}
	public IContentType? GetContentType(string Alias)
	{
		var umbContentTypeService = _services.ContentTypeService;

		var docType = umbContentTypeService!.Get(Alias);

		return docType;
	}

	public HtmlString HighlightText(string Text, List<string> Highlights)
	{
		var highlightedText = Text;

		foreach (var term in Highlights)
		{
			highlightedText = highlightedText.Replace(term, $"<mark>{term}</mark>");
		}

		return new HtmlString(highlightedText);
	}


	#endregion

	#region Users

	public IUser? GetUser(int UserId)
	{
		var umbUserService = _services.UserService;

		return umbUserService!.GetUserById(UserId);
	}

	#endregion


	
}

