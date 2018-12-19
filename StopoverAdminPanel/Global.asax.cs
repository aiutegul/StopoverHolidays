using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace StopoverAdminPanel
{
	public class MvcApplication : HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			UnityConfig.RegisterComponents();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			BundleConfig.RegisterBundles(BundleTable.Bundles);
			DevExtremeBundleConfig.RegisterBundles(BundleTable.Bundles);

			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings
			.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
			GlobalConfiguration.Configuration.Formatters
				.Remove(GlobalConfiguration.Configuration.Formatters.XmlFormatter);

			// Uncomment to use pre-17.2 routing for .Mvc() and .WebApi() data sources
			// DevExtreme.AspNet.Mvc.Compatibility.DataSource.UseLegacyRouting = true;
			// Uncomment to use pre-17.2 behavior for the "required" validation check
			// DevExtreme.AspNet.Mvc.Compatibility.Validation.IgnoreRequiredForBoolean = false;
		}
	}
}