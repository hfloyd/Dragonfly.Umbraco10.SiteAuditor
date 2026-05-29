
namespace Dragonfly.SiteAuditor;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Swashbuckle.AspNetCore.SwaggerGen;

using Umbraco.Cms.Api.Management.OpenApi;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

using Microsoft.OpenApi;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Asp.Versioning;

using Dragonfly.SiteAuditor.Models;
using Dragonfly.SiteAuditor.Services;


internal class SiteAuditorApiConfig
{
	internal const string ProjectNamespace = "Dragonfly.SiteAuditor";
	internal const string ProjectDisplayName = "Dragonfly SiteAuditor";
	internal const string ProjectAlias = "DragonflySiteAuditor";
	internal const string ApiVersion = "1";
	internal const string BackofficeUIApiName = "dragonflysiteauditorui";

	internal static string RazorFilesPath(SiteAuditorService SiteAuditorService)
	{
		return SiteAuditorService.PluginPath() + "RazorViews/";
	}

	internal static StandardViewInfo GetStandardViewInfo()
	{
		var info = new StandardViewInfo();

		info.CurrentToolVersion = PackageInfo.Version;

		//TODO: Make configurable?
		info.ThumbnailWidth = 300;
		info.ThumbnailHeight = 300;

		info.SerilogDirectory = "/umbraco/Logs";
		return info;
	}
}


internal class SiteAuditorApiRouteAttribute : RouteAttribute
{
	public SiteAuditorApiRouteAttribute(string RoutePath)
		: base($"{SiteAuditorApiConfig.ProjectAlias}/api/v{SiteAuditorApiConfig.ApiVersion}/{RoutePath.TrimStart('/')}")
	{ }
}

internal class ConfigureSiteAuditorApiSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
{
	public void Configure(SwaggerGenOptions options)
	{
		// Related documentation:
		// https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api
		// https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/adding-a-custom-swagger-document
		// https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/versioning-your-api
		// https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/access-policies

		// Configure the Swagger generation options
		// Add in a new Swagger API document solely for our own package that can be browsed via Swagger UI
		// Along with having a generated swagger JSON file that we can use to auto generate a TypeScript client
		options.SwaggerDoc(
			SiteAuditorApiConfig.ProjectAlias,
			new OpenApiInfo
			{
				Title = $"{SiteAuditorApiConfig.ProjectDisplayName} API",
				Version = $"{SiteAuditorApiConfig.ApiVersion}",
				// Contact = new OpenApiContact
				// {
				//     Name = "Some Developer",
				//     Email = "you@company.com",
				//     Url = new Uri("https://company.com")
				// }
			}
		);

		// Enable Umbraco authentication for Swagger document
		// PR: https://github.com/umbraco/Umbraco-CMS/pull/15699
		//options.OperationFilter<SiteSpecificApiOperationSecurityFilter>();

	}
}

public class SiteAuditorApiOperationHandler : OperationIdHandler
{
	// This is used to generate nice operation IDs in our swagger json file
	// So that the generated TypeScript client has nice method names and not too verbose
	// https://docs.umbraco.com/umbraco-cms/tutorials/creating-a-backoffice-api/umbraco-schema-and-operation-ids#operation-ids

	public SiteAuditorApiOperationHandler(IOptions<ApiVersioningOptions> apiVersioningOptions) : base(apiVersioningOptions)
	{
	}

	protected override bool CanHandle(ApiDescription apiDescription, ControllerActionDescriptor controllerActionDescriptor)
	{
		return controllerActionDescriptor.ControllerTypeInfo.Namespace?.StartsWith(SiteAuditorApiConfig.ProjectNamespace, comparisonType: StringComparison.InvariantCultureIgnoreCase) is true;
	}

	public override string Handle(ApiDescription apiDescription) => $"{apiDescription.ActionDescriptor.RouteValues["action"]}";
}

internal class SiteSpecificApiOperationSecurityFilter : BackOfficeSecurityRequirementsOperationFilterBase
{
	protected override string ApiName => SiteAuditorApiConfig.ProjectAlias;
}

public class SiteAuditorApiComposer : IComposer
{
	public void Compose(IUmbracoBuilder builder)
	{
		// Register operation ID handler for Swagger
		//	builder.Services.AddSingleton<IOperationIdHandler, SiteSpecificApiOperationHandler>();


		builder.Services.ConfigureOptions<ConfigureSiteAuditorApiSwaggerGenOptions>();
	}
}
