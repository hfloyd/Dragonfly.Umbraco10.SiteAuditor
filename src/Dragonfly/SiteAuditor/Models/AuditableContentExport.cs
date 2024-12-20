namespace Dragonfly.SiteAuditor.Models;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Extensions;


/// <summary>
/// Meta data about a Content Node for Auditing purposes.
/// </summary>
[DataContract]
public class AuditableContentExport
{

	#region Private vars / Methods
	private string _defaultDelimiter = " » ";

	/// <summary>
	/// Default string used for NodePathAsText
	/// ' » ' unless explicitly changed
	/// </summary>
	public void SetDefaultDelimiter(string Delimiter)
	{
		_defaultDelimiter = Delimiter;
	}

	#endregion

	#region Public Props
	public int OverallSort { get; set; }
	public string NodeName { get; set; } = "";

	public IEnumerable<string> NodePath { get; set; } = new List<string>();

	/// <summary>
	/// Full path to node in a single delimited string using object's default delimiter
	/// </summary>
	public string NodePathAsText
	{
		get
		{
			var nodePath = string.Join(_defaultDelimiter, this.NodePath);
			return nodePath;
		}
	}

	public string DocTypeAlias { get; set; } = "";

	public int ParentId { get; set; }

	/// <summary>
	/// Url with domain name. Returns "UNPUBLISHED" if there is no public url.
	/// </summary>
	public string FullUrl { get; set; } = "";

	/// <summary>
	/// Path-only Url. Returns "UNPUBLISHED" if there is no public url.
	/// </summary>
	public string RelativeUrl { get; set; } = "";

	public int Level { get; set; }

	public int SortOrder { get; set; }

	/// <summary>
	/// Alias of the Template assigned to this Content Node. Returns "NONE" if there is no template.
	/// </summary>
	public string TemplateAlias { get; set; } = "";

	public DateTime CreateDate { get; set; }

	public string CreateUser { get; set; } = "";
	public DateTime UpdateDate { get; set; }

	public string UpdateUser { get; set; } = "";

	public bool IsPublished { get; set; }

	public int NodeId { get; set; }

	public Guid NodeGuid { get; set; }

	public string Udi { get; set; } = "";


	#endregion

	public AuditableContentExport() { }

	public AuditableContentExport(AuditableContent Ac, int OverallSortNum)
	{
		this.OverallSort = OverallSortNum;

		this.NodeName = Ac.UmbContentNode != null && Ac.UmbContentNode.Name != null ? Ac.UmbContentNode.Name : "UNKNOWN";
		this.NodePath = Ac.NodePath;
		this.DocTypeAlias = Ac.UmbContentNode != null ? Ac.UmbContentNode.ContentType.Alias : "NONE";
		this.ParentId = Ac.UmbContentNode != null ? Ac.UmbContentNode.ParentId : 0;
		this.FullUrl = Ac.FullNiceUrl;
		this.RelativeUrl = Ac.RelativeNiceUrl;
		this.Level = Ac.UmbContentNode != null ?  Ac.UmbContentNode.Level:0;
		this.SortOrder = Ac.UmbContentNode != null ?  Ac.UmbContentNode.SortOrder:0;
		this.TemplateAlias = Ac.TemplateAlias;
		this.CreateDate = Ac.UmbContentNode != null ?  Ac.UmbContentNode.CreateDate:DateTime.MinValue;
		this.CreateUser = Ac.CreateUser != null ? Ac.CreateUser.Username : "UNKNOWN";
		this.UpdateDate = Ac.UmbContentNode != null ?  Ac.UmbContentNode.UpdateDate:DateTime.MinValue;
		this.UpdateUser = Ac.UpdateUser != null ? Ac.UpdateUser.Username : "UNKNOWN";
		this.IsPublished = Ac.IsPublished;
		this.NodeId = Ac.UmbContentNode != null ?  Ac.UmbContentNode.Id:0;
		this.NodeGuid = Ac.UmbContentNode != null ?  Ac.UmbContentNode.Key:new Guid();
		this.Udi = Ac.UmbContentNode != null ?  Ac.UmbContentNode.GetUdi().ToString():"";
	}


}

public static class AuditableContentExportExtensions
{
	public static IEnumerable<AuditableContentExport> ConvertToExportable(this IEnumerable<AuditableContent> Items)
	{
		var list = new List<AuditableContentExport>();
		var counter = 1;

		foreach (var ac in Items)
		{
			var eac = new AuditableContentExport(ac, counter);
			list.Add(eac);
			counter++;
		}

		return list;
	}
}



