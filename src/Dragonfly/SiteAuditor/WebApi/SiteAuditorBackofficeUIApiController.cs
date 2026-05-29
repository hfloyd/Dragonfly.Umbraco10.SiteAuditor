using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;

namespace Dragonfly.SiteAuditor;


[ApiController]
[ApiVersion(SiteAuditorApiConfig.ApiVersion)]
[MapToApi(SiteAuditorApiConfig.BackofficeUIApiName)]
[ApiExplorerSettings(GroupName = SiteAuditorApiConfig.ProjectDisplayName)]
[BackOfficeRoute("dragonflysiteauditorui/api/v{version:apiVersion}")]
[Authorize(Policy = AuthorizationPolicies.SectionAccessSettings)]
public class SiteAuditorBackofficeUIApiController : ControllerBase
{
	private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

	public SiteAuditorBackofficeUIApiController(IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
	{
		_backOfficeSecurityAccessor = backOfficeSecurityAccessor;
	}

	[HttpGet("ping")]
	[ProducesResponseType<string>(StatusCodes.Status200OK)]
	public string Ping() => "Pong";

	[HttpGet("whatsTheTimeMrWolf")]
	[ProducesResponseType(typeof(DateTime), 200)]
	public DateTime WhatsTheTimeMrWolf() => DateTime.Now;

	[HttpGet("whatsMyName")]
	[ProducesResponseType<string>(StatusCodes.Status200OK)]
	public string WhatsMyName()
	{
		// So we can see a long request in the dashboard with a spinning progress wheel
		Thread.Sleep(2000);

		var currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
		return currentUser?.Name ?? "I have no idea who you are";
	}

	[HttpGet("whoAmI")]
	[ProducesResponseType<IUser>(StatusCodes.Status200OK)]
	public IUser? WhoAmI() => _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
}

