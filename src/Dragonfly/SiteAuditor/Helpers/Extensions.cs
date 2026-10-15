using Dragonfly.NetHelpers;

namespace Dragonfly.SiteAuditor.Helpers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Dragonfly.SiteAuditor.Models;
	using Umbraco.Cms.Core.Models;

	public static class Extensions
	{
		#region IContent
		public static bool HasPropertyValue(this IContent Content, string PropertyAlias)
		{
			var hasProp = Content.HasProperty(PropertyAlias);

			if (!hasProp)
			{
				return false;
			}

			var valObject = Content.GetValue(PropertyAlias);
			if (valObject == null)
			{
				return false;
			}

			var valString = Content.GetValue<string>(PropertyAlias);
			if (valString == "")
			{
				return false;
			}

			return true;
		}

		#endregion

		#region IMedia
		public static bool HasPropertyValue(this IMedia Media, string PropertyAlias)
		{
			var hasProp = Media.HasProperty(PropertyAlias);

			if (!hasProp)
			{
				return false;
			}

			var valObject = Media.GetValue(PropertyAlias);
			if (valObject == null)
			{
				return false;
			}

			var valString = Media.GetValue<string>(PropertyAlias);
			if (valString == "")
			{
				return false;
			}

			return true;
		}

		#endregion
	}

	public static class Helper
	{
		public enum EntityType
		{
			DataType,
			DocType,
			MediaType,
			MemberType,
			Template,
			ContentNode,
			MediaNode,
		}

		public enum FileEntityType
		{
			PartialView,
			Stylesheet,
			Script

		}


		public static string GetBackofficeEntityUrl(EntityType Type, Guid EntityGuid)
		{
			var url = "/umbraco";

			switch (Type)
			{
				case EntityType.DataType:
					//url = $"/umbraco/#/settings/dataTypes/edit/{EntityGuid}"; v9-13
					url=$"/umbraco/section/settings/workspace/data-type/edit/{EntityGuid.ToString()}";
					break;

				case EntityType.DocType:
					//url = $"/umbraco/#/settings/documentTypes/edit/{EntityGuid}"; v9-13
					url = $"/umbraco/section/settings/workspace/document-type/edit/{EntityGuid.ToString()}";
					break;

				case EntityType.MediaType:
					url = $"/umbraco/section/settings/workspace/media-type/edit/{EntityGuid.ToString()}";
					break;

				case EntityType.MemberType:
					url = $"/umbraco/section/settings/workspace/member-type/edit/{EntityGuid.ToString()}";
					break;

				case EntityType.Template:
					//url = $"/umbraco/#/settings/templates/edit/{EntityGuid}"; v9-13
					url = $"/umbraco/section/settings/workspace/template/edit/{EntityGuid.ToString()}";
					break;

				case EntityType.ContentNode:
					url = $"/umbraco/section/content/workspace/document/edit/{EntityGuid.ToString()}";
					break;

				case EntityType.MediaNode:
					url = $"/umbraco/section/media/workspace/media/edit/{EntityGuid.ToString()}";
					break;
				

			}

			return url;
		}

		public static string GetBackofficeFileEditUrl(FileEntityType Type, string FileName)
		{
			var url = "/umbraco";

			switch (Type)
			{
				case FileEntityType.PartialView:
					url = $"/umbraco/section/settings/workspace/partial-view/edit/{FileName.UrlEncode()}";
					break;
				case FileEntityType.Stylesheet:
					url = $"/umbraco/section/settings/workspace/stylesheet/edit/{FileName.UrlEncode()}";
					break;
				case FileEntityType.Script:
					url = $"/umbraco/section/settings/workspace/script/edit/{FileName.UrlEncode()}";
					break;
			}

			return url;
		}
	}
}
