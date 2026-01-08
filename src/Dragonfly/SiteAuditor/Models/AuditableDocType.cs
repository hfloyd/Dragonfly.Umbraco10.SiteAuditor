namespace Dragonfly.SiteAuditor.Models
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Umbraco.Cms.Core.Models;
	using Umbraco.Cms.Core.Models.PublishedContent;

	public class AuditableDocType
	{

		#region Public Props

		public IContentType? ContentType { get; set; }

		public string Name { get; set; } = "";
		public string Alias { get; set; } = "";
		public IEnumerable<string> FolderPaths { get; internal set; } = new List<string>();

		public Guid Guid { get; set; }
		public int Id { get; set; }
		public string DefaultTemplateName { get; set; } = "";
		public Dictionary<int, string> AllowedTemplates { get; set; } = new Dictionary<int, string>();
		public bool HasContentNodes { get; set; }
		public bool IsElement { get; set; }
		public bool IsComposition { get; set; }
		public Dictionary<int, string> CompositionsUsed { get; set; } = new Dictionary<int, string>();


		//TODO: Add Info about compositions/parents/folders: IsComposition, HasCompositions, etc.

		#endregion


		public AuditableDocType()
		{

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
		private string _defaultDelimiter = " » ";

		/// <summary>
		/// Full path in a single delimited string using object's default delimiter
		/// </summary>
		public string FolderPathAsText
		{
			get
			{
				if (this.FolderPaths is null || !this.FolderPaths.Any())
				{
					return "";
				}
				else
				{
					var path = string.Join(this.DefaultDelimiter, this.FolderPaths);
					return path;
				}
			}
		}

	}

	public class DocTypesAndElements
	{
		public IList<IContentType> AllNodeTypes { get; set; } = new List<IContentType>();
		public IList<IContentType> AllElementTypes { get; set; } = new List<IContentType>();
	}
}
