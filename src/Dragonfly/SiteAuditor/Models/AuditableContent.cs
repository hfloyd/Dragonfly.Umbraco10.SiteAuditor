namespace Dragonfly.SiteAuditor.Models;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants.Conventions;

/// <summary>
/// Meta data about a Content Node for Auditing purposes.
/// </summary>
[DataContract]
public class AuditableContent
{
	#region Private vars
	//private UmbracoHelper umbHelper = new UmbracoHelper(UmbracoContext.Current);
	// private IContentService umbContentService = ApplicationContext.Current.Services.ContentService;

	private string _defaultDelimiter = " » ";
	private IContent _umbContentNode;
	private IPublishedContent? _umbPublishedNode;
	private IEnumerable<string> _nodePath = new List<string>();
	private string _templateAlias = "";
	private string _fullNiceUrl = "";
	private bool _isPublished;
	private string _relativeNiceUrl = "";
	private IUser? _createUser;
	private IUser? _updateUser;
	private int _nodeId;
	private string _nodeName = "";
	private string _nodeContentTypeAlias;
	private int _nodeParentId;
	private int _nodeLevel;
	private int _nodeSortOrder;
	private DateTime _nodeCreateDate;
	private DateTime _nodeUpdateDate;
	private string _nodeUdi;

	#endregion

	#region Public Props

	/// <summary>
	/// Gets or sets the content node as an IContent
	/// </summary>
	/// <remarks>May be NULL if "Published Content Only" was used to create</remarks>
	public IContent? UmbContentNode
	{
		get => _umbContentNode;
		internal set => _umbContentNode = value;
	}

	/// <summary>
	/// Gets or sets the content node and an IPublishedContent
	/// </summary>
	public IPublishedContent? UmbPublishedNode
	{
		get => _umbPublishedNode;
		internal set => _umbPublishedNode = value;
	}

	/// <summary>
	/// The node path.
	/// </summary>
	/// <returns>
	/// The <see cref="string"/>.
	/// </returns>
	public IEnumerable<string> NodePath
	{
		get => _nodePath;
		internal set => _nodePath = value;
	}

	/// <summary>
	/// Default string used for NodePathAsText
	/// ' » ' unless explicitly changed
	/// </summary>
	public string DefaultDelimiter
	{
		get { return _defaultDelimiter; }
		internal set { _defaultDelimiter = value; }
	}

	/// <summary>
	/// Full path to node in a single delimited string using object's default delimiter
	/// </summary>
	public string NodePathAsText
	{
		get
		{
			var nodePath = string.Join(this.DefaultDelimiter, this.NodePath);
			return nodePath;
		}
	}

	/// <summary>
	/// Alias of the Template assigned to this Content Node. Returns "NONE" if there is no template.
	/// </summary>
	public string TemplateAlias
	{
		get => _templateAlias;
		internal set => _templateAlias = value;
	}

	/// <summary>
	/// Url with domain name. Returns "UNPUBLISHED" if there is no public url.
	/// </summary>
	public string FullNiceUrl
	{
		get => _fullNiceUrl;
		internal set => _fullNiceUrl = value;
	}

	public bool IsPublished
	{
		get => _isPublished;
		internal set => _isPublished = value;
	}

	/// <summary>
	/// Path-only Url. Returns "UNPUBLISHED" if there is no public url.
	/// </summary>
	public string RelativeNiceUrl
	{
		get => _relativeNiceUrl;
		internal set => _relativeNiceUrl = value;
	}

	public IUser? CreateUser
	{
		get => _createUser;
		set => _createUser = value;
	}

	public IUser? UpdateUser
	{
		get => _updateUser;
		set => _updateUser = value;
	}

	public int NodeId
	{
		get => _nodeId;
		set => _nodeId = value;
	}

	public string NodeName
	{
		get => _nodeName;
		set => _nodeName = value;
	}

	public string NodeContentTypeAlias
	{
		get => _nodeContentTypeAlias;
		set => _nodeContentTypeAlias = value;
	}

	public int NodeParentId
	{
		get => _nodeParentId;
		set => _nodeParentId = value;
	}

	public int NodeLevel
	{
		get => _nodeLevel;
		set => _nodeLevel = value;
	}

	public int NodeSortOrder
	{
		get => _nodeSortOrder;
		set => _nodeSortOrder = value;
	}

	public DateTime NodeCreateDate
	{
		get => _nodeCreateDate;
		set => _nodeCreateDate = value;
	}

	public DateTime NodeUpdateDate
	{
		get => _nodeUpdateDate;
		set => _nodeUpdateDate = value;
	}

	public string NodeUdi
	{
		get => _nodeUdi;
		set => _nodeUdi = value;
	}

	#endregion

	/// <summary>
	/// An empty constructor is provided because either an IContent or IPublishedContent can be used to populate this object.
	/// </summary>
	public AuditableContent()
	{
		
	}

	#region Methods

	public string NodePathAsCustomText(string Separator = " » ")
	{
		var nodePath = string.Join(Separator, this.NodePath);
		return nodePath;
	}

	#endregion

	
}

