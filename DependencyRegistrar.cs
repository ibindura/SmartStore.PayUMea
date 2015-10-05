using Autofac;
using Autofac.Integration.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Core.Plugins;
using SmartStore.Web.Controllers;
using SmartStore.PayUMea.Filters;

namespace SmartStore.PayUMea
{
	public class DependencyRegistrar : IDependencyRegistrar
	{
		public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
		{
			if (isActiveModule)
			{
				builder.RegisterType<PayUMeaExpressCheckoutFilter>().AsActionFilterFor<CheckoutController>(x => x.PaymentMethod()).InstancePerRequest();
			}
		}

		public int Order
		{
			get { return 1; }
		}
	}
}
