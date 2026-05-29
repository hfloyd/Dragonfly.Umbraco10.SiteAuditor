namespace Dragonfly.SiteAuditor;

using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Umbraco.Cms.Api.Common.Attributes;


[ApiController]
[ApiVersion(SiteAuditorApiConfig.ApiVersion)]
[MapToApi(SiteAuditorApiConfig.ProjectAlias)]
[ApiExplorerSettings(GroupName = SiteAuditorApiConfig.ProjectDisplayName)]
public abstract class SiteAuditorApiControllerBase(IBackofficeUserAccessor backofficeUserAccessor) : ControllerBase
{
    protected IBackofficeUserAccessor _backofficeUserAccessor { get; } = backofficeUserAccessor;

    internal bool IsBackOfficeAuthorized()
    {
        if (!_backofficeUserAccessor.BackofficeUser.IsAuthenticated)
        {
            return false;
        }

        return true;
    }

    internal string? CurrentBackofficeUserName()
    {
        if (IsBackOfficeAuthorized())
        {
            return _backofficeUserAccessor.BackofficeUser.Name;
        }
        else
        {
            return null;
        }
    }

    internal BackOfficeUserInfo CurrentBackofficeUser()
    {
        if (IsBackOfficeAuthorized())
        {
            return new BackOfficeUserInfo(_backofficeUserAccessor.BackofficeUser);
        }
        else
        {
            return null;
        }
    }
}

internal class BackOfficeUserInfo
{
    public string UserName { get; }
    public string UserGuid { get; }
    public string DisplayName { get; }
    public string CultureCode { get; }
    public string EmailAddress { get; }

    public IEnumerable<string> BackOfficeRoles { get; }

    public IEnumerable<string> AllowedApps { get; }

    public int StartMediaNodeId { get; }
    public int StartContentNodeId { get; }
    public int UserNodeId { get; }


    [JsonIgnore]
    public List<Claim> AllClaims { get; }

    internal BackOfficeUserInfo(ClaimsIdentity ClaimsId)
    {
        //Strings
        UserName = ClaimsId.Name ?? "";
        AllClaims = ClaimsId.Claims.ToList();
        DisplayName = ClaimsId.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value ?? "";
        EmailAddress = ClaimsId.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value ?? "";
        CultureCode = ClaimsId.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/locality")?.Value ?? "";
        UserGuid = ClaimsId.Claims.FirstOrDefault(x => x.Type == "sub")?.Value ?? "";


        //Ints
        var userIdStr = ClaimsId.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        UserNodeId = String.IsNullOrEmpty(userIdStr) ? 0 : Convert.ToInt32(userIdStr);

        var contentNodeStr = ClaimsId.Claims.FirstOrDefault(x => x.Type == "http://umbraco.org/2015/02/identity/claims/backoffice/startcontentnode")?.Value;
        StartContentNodeId = String.IsNullOrEmpty(contentNodeStr) ? 0 : Convert.ToInt32(contentNodeStr);

        var mediaNodeStr = ClaimsId.Claims.FirstOrDefault(x => x.Type == "http://umbraco.org/2015/02/identity/claims/backoffice/startmedianode")?.Value;
        StartMediaNodeId = String.IsNullOrEmpty(mediaNodeStr) ? 0 : Convert.ToInt32(mediaNodeStr);

        //Other
        AllowedApps = ClaimsId.Claims.Where(x => x.Type == "http://umbraco.org/2015/02/identity/claims/backoffice/allowedapp").Select(c => c.Value);
        BackOfficeRoles = ClaimsId.Claims.Where(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Select(c => c.Value);
      
        //xx = ClaimsId.Claims.FirstOrDefault(x => x.Type == "http")?.Value ?? "";
        //xx = ClaimsId.Claims.FirstOrDefault(x => x.Type == "http")?.Value ?? "";
        //xx = ClaimsId.Claims.FirstOrDefault(x => x.Type == "http")?.Value ?? "";
        
    }

 
}

