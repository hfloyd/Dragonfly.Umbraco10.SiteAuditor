
namespace Dragonfly.SiteAuditor;

using System;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Services;

using Dragonfly.NetHelperServices;
using Dragonfly.NetModels;
using Dragonfly.SiteAuditor.Models;
using Dragonfly.SiteAuditor.Services;
using Microsoft.AspNetCore.Http;


[SiteAuditorApiRouteAttribute("SiteAuditor")]
public class SiteAuditorController(
	 ILogger<SiteAuditorController> logger
	 , IBackofficeUserAccessor backofficeUserAccessor
	 , SiteAuditorApiContentService siteAuditorApiContentService
	 , SiteAuditorService siteAuditorService
	 ) : DragonflyApiControllerBase(backofficeUserAccessor)
{
	#region CTOR/DI/Variables

	private readonly ILogger<SiteAuditorController> _Logger = logger;
	
	private readonly SiteAuditorService _SiteAuditorService = siteAuditorService;
	private readonly SiteAuditorApiContentService _SiteAuditorApiContentService = siteAuditorApiContentService;

	private readonly string _UnauthorizedMessage =
		"<p class=\"alert alert-danger\">You are not authorized to view this page. Please log in to the Umbraco back-office and try again.</p>";

	#endregion

	#region Content Nodes

	[HttpGet("GetAllContentAsHtmlTable")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult GetAllContentAsHtmlTable(bool PublishedOnly = false, string Search = "")
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.GetAllContentAsHtmlTable(this.HttpContext, PublishedOnly, Search);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");
	}


	[HttpGet("GetContentForDoctypeHtml")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult GetContentForDoctypeHtml(string DocTypeAlias = "", bool PublishedOnly = false, string Search = "")
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.GetContentForDoctypeHtml(this.HttpContext, DocTypeAlias, PublishedOnly, Search);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");
	}

	[HttpGet("GetContentForElementHtml")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult GetContentForElementHtml(string DocTypeAlias = "", bool PublishedOnly = false, string Search = "")
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.GetContentForElementHtml(this.HttpContext, DocTypeAlias, PublishedOnly, Search);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");
	}

	#endregion

	#region Media Nodes

	[HttpGet("GetAllMediaAsHtmlTable")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult GetAllMediaAsHtmlTable(bool ShowImageThumbnails = false, string Search = "")
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.GetAllMediaAsHtmlTable(this.HttpContext, ShowImageThumbnails, Search);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");
	}

	[HttpGet("GetMediaForTypeHtml")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult GetMediaForTypeHtml(string MediaTypeAlias = "", bool ShowImageThumbnails = false, string Search = "")
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.GetMediaForTypeHtml(this.HttpContext, MediaTypeAlias, ShowImageThumbnails, Search);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");

	}

	#endregion

	#region GetContentWithValues

	[HttpGet("GetContentWithValues")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult GetContentWithValues(string PropertyAlias = "", bool PublishedOnly = false, string Search = "")
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.GetContentWithValues(this.HttpContext, PropertyAlias, PublishedOnly, Search);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");

	}

	#endregion

	#region GetMediaWithValues

	[HttpGet("GetMediaWithValues")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult GetMediaWithValues(string PropertyAlias = "", string Search = "")
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.GetMediaWithValues(this.HttpContext, PropertyAlias, Search);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");

	}


	#endregion

	#region Properties Info

	[HttpGet("GetAllPropertiesAsHtmlTable")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult GetAllPropertiesAsHtmlTable(string Search = "")
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.GetAllPropertiesAsHtmlTable(this.HttpContext, Search);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");

	}

	[HttpGet("GetPropertiesForDoctypeHtml")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult GetPropertiesForDoctypeHtml(string DocTypeAlias = "", string Search = "")
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.GetPropertiesForDoctypeHtml(this.HttpContext, DocTypeAlias, Search);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");

	}


	#endregion

	#region DataType Info

	[HttpGet("GetAllDataTypesAsHtmlTable")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult GetAllDataTypesAsHtmlTable(string Search = "")
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.GetAllDataTypesAsHtmlTable(this.HttpContext, Search);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");

	}

	#endregion

	#region DocTypes Info

	[HttpGet("GetAllDocTypesAsHtmlTable")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult GetAllDocTypesAsHtmlTable(string Search = "")
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.GetAllDocTypesAsHtmlTable(this.HttpContext, Search);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");

	}

	#endregion

	#region Element Types Info

	[HttpGet("GetContentForElementType")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult GetContentForElementType(string ElementTypeAlias = "", bool PublishedOnly = false, string Search = "")
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.GetContentForElementType(this.HttpContext, ElementTypeAlias, PublishedOnly, Search);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");

	}

	#endregion

	#region Templates Info

	[HttpGet("GetAllTemplatesAsHtmlTable")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult GetAllTemplatesAsHtmlTable(string Search = "")
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.GetAllTemplatesAsHtmlTable(this.HttpContext, Search);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");

	}


	#endregion

	#region Special Queries

	[HttpGet("TemplateUsageReport")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult TemplateUsageReport()
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.TemplateUsageReport(this.HttpContext);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");

	}
	#endregion

	#region Logs

	[HttpGet("GetLogs")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult GetLogs(DateTime? StartDate = null, DateTime? EndDate = null, int BatchBy = 0, [FromQuery] string PromotedProperties = "")
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.GetLogs(this.HttpContext, StartDate, EndDate, BatchBy, PromotedProperties);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");

	}


	[HttpGet("GetLogsAsHtmlTable")]
	[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	public IActionResult GetLogsAsHtmlTable(DateTime StartDate, DateTime? EndDate = null, string PromotedProperties = "")
	{
		var htmlContent = _UnauthorizedMessage;
		if (IsBackOfficeAuthorized())
		{
			htmlContent = _SiteAuditorApiContentService.GetLogsAsHtmlTable(this.HttpContext, StartDate, EndDate, PromotedProperties);
		}

		//RETURN AS HTML
		return Content(htmlContent, "text/html");

	}

	#endregion


	#region Tests & Examples

	//[HttpGet("Test")]
	//public bool Test()
	//{
	//	//LogHelper.Info<PublicApiController>("Test STARTED/ENDED");
	//	return true;
	//}

	//[HttpGet("SayHello")]
	//[ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
	//public IActionResult SayHello()
	//{
	//	if (!IsBackOfficeAuthorized())
	//	{
	//		return Unauthorized("You must be logged-in to the back-office to use this");
	//	}

	//	var userInfo = CurrentBackofficeUserName();

	//	return Ok($"Hello, {userInfo} ");

	//}

	//[HttpGet("ExampleReturnHtml")]
	//[ProducesResponseType(typeof(ContentResult), StatusCodes.Status200OK)]
	//public IActionResult ExampleReturnHtml()
	//{
	//	var returnSB = new StringBuilder();

	//	if (!IsBackOfficeAuthorized())
	//	{
	//		returnSB.AppendLine("<h1>Hello! This is HTML</h1>");
	//	}
	//	else
	//	{
	//		returnSB.AppendLine($"<h1>Hello, {CurrentBackofficeUser().DisplayName}!</h1>");
	//	}

	//	returnSB.AppendLine($"<p>This is rendering on {DateTime.Today.ToShortDateString()}</p>");

	//	if (!IsBackOfficeAuthorized())
	//	{
	//		returnSB.AppendLine($"<p>You are not logged-in</p>");
	//	}
	//	else
	//	{
	//		returnSB.AppendLine($"<p>You are logged-in as {CurrentBackofficeUserName()}</p>");

	//		var user = CurrentBackofficeUser();

	//		returnSB.AppendLine($"<h2>User Object</h2>");
	//		returnSB.AppendLine($"<textarea id=\"user\" name=\"user\" rows=\"20\" cols=\"500\">{JsonConvert.SerializeObject(user, Formatting.Indented)}</textarea>");


	//		returnSB.AppendLine($"<h2>Your Claims</h2>");
	//		var claims = user.AllClaims;
	//		returnSB.AppendLine($"<ol>");
	//		foreach (var claim in claims)
	//		{
	//			returnSB.AppendLine($"<li>{claim.Type} = {claim.Value}</li>");
	//		}
	//		returnSB.AppendLine($"</ol>");


	//	}

	//	return Content(returnSB.ToString(), "text/html");
	//}

	//[HttpGet("ExampleReturnJson")]
	//[ProducesResponseType(typeof(JsonResult), StatusCodes.Status200OK)]
	//public IActionResult ExampleReturnJson()
	//{
	//	var testData1 = new TimeSpan(1, 1, 1, 1);
	//	//  var testData2= new StatusMessage(true, "This is a test object so you can see JSON!");

	//	return new JsonResult(testData1);
	//}

	#endregion
}

