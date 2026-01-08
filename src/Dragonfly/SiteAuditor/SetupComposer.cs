#pragma warning disable 1591

namespace Dragonfly.SiteAuditor
{
	using Microsoft.Extensions.DependencyInjection;
	using Umbraco.Cms.Core.Composing;
	using Umbraco.Cms.Core.DependencyInjection;

	using Dragonfly.NetHelperServices;

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

			//This causes a 503 error in Umbraco 10.0.0
			//WebApplicationBuilder waBuilder = WebApplication.CreateBuilder();
			//var app = waBuilder.Build();
			//app.UseImageSharp();
			//app.UseStaticFiles();

		}

	}

}