using Dragonfly.SiteAuditor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Cms.Web.Common.Routing;

namespace Dragonfly.SiteAuditor.WebApi
{
    [ApiController]
    [BackOfficeRoute("dragonflysiteauditor/api/v{version:apiVersion}")]
    [Authorize(Policy = AuthorizationPolicies.SectionAccessContent)]
    [MapToApi(Constants.ApiName)]
    public class DragonflySiteAuditorApiControllerBase : ControllerBase
    {
    }
}
