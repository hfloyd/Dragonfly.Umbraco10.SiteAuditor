using Asp.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;

#pragma warning disable 1591

namespace Dragonfly.SiteAuditor;

using Dragonfly.NetHelperServices;
using Dragonfly.SiteAuditor.Services;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

public class SetupComposer : IComposer
{
	public void Compose(IUmbracoBuilder umbBuilder)
	{
		umbBuilder.Services.AddMvcCore().AddRazorViewEngine();
		umbBuilder.Services.AddControllersWithViews();
		umbBuilder.Services.AddRazorPages();

		umbBuilder.Services.AddSingleton<IViewRenderService, Dragonfly.NetHelperServices.ViewRenderService>();
		umbBuilder.Services.AddSingleton<Dragonfly.NetHelperServices.FileHelperService>();

		umbBuilder.Services.AddScoped<Dragonfly.SiteAuditor.Services.SiteAuditorService>();
		umbBuilder.Services.AddScoped<Dragonfly.SiteAuditor.Services.AuditorInfoService>();
		umbBuilder.Services.AddScoped<SiteAuditorApiContentService>();

		umbBuilder.Services.AddSingleton<IOperationIdHandler, CustomOperationHandler>();

		umbBuilder.Services.Configure<SwaggerGenOptions>(opt =>
		{
			// Related documentation:
			// https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api
			// https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/adding-a-custom-swagger-document
			// https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/versioning-your-api
			// https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/access-policies

			// Configure the Swagger generation options
			// Add in a new Swagger API document solely for our own package that can be browsed via Swagger UI
			// Along with having a generated swagger JSON file that we can use to auto generate a TypeScript client
			opt.SwaggerDoc(SiteAuditorApiConfig.BackofficeUIApiName, new OpenApiInfo
			{
				Title = "Dragonfly SiteAuditor Backoffice API",
				Version = "1.0",
				// Contact = new OpenApiContact
				// {
				//     Name = "Some Developer",
				//     Email = "you@company.com",
				//     Url = new Uri("https://company.com")
				// }
			});

			// Enable Umbraco authentication for the "Example" Swagger document
			// PR: https://github.com/umbraco/Umbraco-CMS/pull/15699
			opt.OperationFilter<DragonflySiteAuditorOperationSecurityFilter>();
		});

		umbBuilder.Services.Configure<RazorViewEngineOptions>(options =>
		{
			options.ViewLocationFormats.Add("~/App_Plugins/Dragonfly.SiteAuditor/RazorViews/{0}.cshtml");
			options.ViewLocationFormats.Add("/App_Plugins/Dragonfly.SiteAuditor/RazorViews/{0}.cshtml");
			options.ViewLocationFormats.Add("App_Plugins/Dragonfly.SiteAuditor/RazorViews/{0}.cshtml");
		});

	}

}

public class DragonflySiteAuditorOperationSecurityFilter : BackOfficeSecurityRequirementsOperationFilterBase
{
	protected override string ApiName => SiteAuditorApiConfig.BackofficeUIApiName;
}

// This is used to generate nice operation IDs in our swagger json file
// So that the gnerated TypeScript client has nice method names and not too verbose
// https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/umbraco-schema-and-operation-ids#operation-ids
public class CustomOperationHandler : OperationIdHandler
{
	public CustomOperationHandler(IOptions<ApiVersioningOptions> apiVersioningOptions) : base(apiVersioningOptions)
	{
	}

	protected override bool CanHandle(ApiDescription apiDescription, ControllerActionDescriptor controllerActionDescriptor)
	{
		return controllerActionDescriptor.ControllerTypeInfo.Namespace?.StartsWith("Dragonfly.SiteAuditor.Controllers", comparisonType: StringComparison.InvariantCultureIgnoreCase) is true;
	}

	public override string Handle(ApiDescription apiDescription) => $"{apiDescription.ActionDescriptor.RouteValues["action"]}";
}

