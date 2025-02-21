namespace Dragonfly.SiteAuditor.Models;

using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;

/// <summary>
/// Meta data about a Media Node for Auditing purposes.
/// </summary>
[DataContract]
public class AuditableMedia
{
	#region Private vars
	//private UmbracoHelper umbHelper = new UmbracoHelper(UmbracoContext.Current);
	// private IContentService umbContentService = ApplicationContext.Current.Services.ContentService;

	private string _defaultDelimiter = " » ";
	#endregion

	#region Public Props
	/// <summary>
	/// Gets or sets the node as an IMedia
	/// </summary>
	//public IMedia? UmbMediaNode { get; internal set; }

	/// <summary>
	/// Gets or sets the node and an IPublishedContent
	/// </summary>
	public IPublishedContent? UmbPublishedNode { get; internal set; }

	/// <summary>
	/// The node path.
	/// </summary>
	/// <returns>
	/// The <see cref="string"/>.
	/// </returns>
	public IEnumerable<string> NodePath { get; internal set; } = new List<string>();

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
	/// Size of the file in bytes.
	/// </summary>
	public long SizeInBytes { get; internal set; }

	/// <summary>
	/// Size of the file in B, KB, MB, etc.
	/// </summary>
	public string SizeReadable { get; internal set; } = "";

	/// <summary>
	/// Url with domain name.
	/// </summary>
	public string FullUrl { get; internal set; } = "";


	/// <summary>
	/// Path-only Url. 
	/// </summary>
	public string RelativeUrl { get; internal set; } = "";

	public IUser? CreateUser { get; set; }
	public IUser? UpdateUser { get; set; }
	public string? FileExtension { get; set; }
	public int WidthPixels { get; set; }
	public int HeightPixels { get; set; }
	public string? AltText { get; set; }

	#endregion

	public AuditableMedia()
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

