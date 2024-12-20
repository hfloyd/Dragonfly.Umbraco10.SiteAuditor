#pragma warning disable 1591

namespace Dragonfly.SiteAuditor.Composers
{
	using Dragonfly.NetHelperServices;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.Extensions.DependencyInjection;
	using SixLabors.ImageSharp.Web.DependencyInjection;
	using Umbraco.Cms.Core.Composing;
	using Umbraco.Cms.Core.DependencyInjection;

	public class SetupComposer : IComposer
	{
		public void Compose(IUmbracoBuilder umbBuilder)
		{
			umbBuilder.Services.AddMvcCore().AddRazorViewEngine();
			umbBuilder.Services.AddControllersWithViews();
			umbBuilder.Services.AddRazorPages();
			//builder.Services.AddSingleton<IRazorViewEngine>();
			//  builder.Services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
			// builder.Services.AddScoped<IServiceProvider, ServiceProvider>();

			umbBuilder.Services.AddHttpContextAccessor();

			umbBuilder.Services.AddScoped<IViewRenderService, Dragonfly.NetHelperServices.ViewRenderService>();
			umbBuilder.Services.AddScoped<Dragonfly.NetHelperServices.FileHelperService>();

			umbBuilder.Services.AddScoped<Dragonfly.SiteAuditor.Services.DependencyLoader>();
			umbBuilder.Services.AddScoped<Dragonfly.SiteAuditor.Services.SiteAuditorService>();
			umbBuilder.Services.AddScoped<Dragonfly.SiteAuditor.Services.AuditorInfoService>();

			
			WebApplicationBuilder waBuilder = WebApplication.CreateBuilder();
			var app = waBuilder.Build();
			app.UseImageSharp();
			app.UseStaticFiles();

		}

	}

}