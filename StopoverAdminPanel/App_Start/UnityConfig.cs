using System.Net.Http;
using System.Web.Mvc;
using Unity;
using Unity.Injection;
using Unity.Mvc5;

namespace StopoverAdminPanel
{
	public static class UnityConfig
	{
		public static void RegisterComponents()
		{
			var container = new UnityContainer();

			container.RegisterType<HttpClient>(
				new InjectionFactory(x =>
					new HttpClient()
				)
			);

			DependencyResolver.SetResolver(new UnityDependencyResolver(container));
		}
	}
}