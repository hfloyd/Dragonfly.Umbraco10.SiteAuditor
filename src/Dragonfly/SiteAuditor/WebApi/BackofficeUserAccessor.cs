namespace Dragonfly.SiteAuditor;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;


using UmbConstants = Umbraco.Cms.Core.Constants;


public interface IBackofficeUserAccessor
{
    ClaimsIdentity BackofficeUser { get; }
}

public class BackofficeUserAccessor(
    IOptionsSnapshot<CookieAuthenticationOptions> cookieOptionsSnapshot
    , IHttpContextAccessor httpContextAccessor
    ) : IBackofficeUserAccessor
{

    public ClaimsIdentity BackofficeUser
    {
        get
        {
            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext == null)
                return new ClaimsIdentity();

            var cookieOptions = cookieOptionsSnapshot.Get(UmbConstants.Security.BackOfficeAuthenticationType);
            var backOfficeCookie = httpContext.Request.Cookies[cookieOptions.Cookie.Name!];

            if (string.IsNullOrEmpty(backOfficeCookie))
                return new ClaimsIdentity();

            var unprotected = cookieOptions.TicketDataFormat.Unprotect(backOfficeCookie!);

            if (unprotected == null)
                return new ClaimsIdentity();

            var backOfficeIdentity = unprotected!.Principal.GetUmbracoIdentity();

            return backOfficeIdentity ?? new ClaimsIdentity();
        }
    }

}

public class BackofficeUserAccessorComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<IBackofficeUserAccessor, BackofficeUserAccessor>();
    }
}
