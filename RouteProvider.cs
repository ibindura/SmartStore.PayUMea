using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Mvc.Routes;

namespace SmartStore.PayUMea
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.PayUMeaExpress",
                "Plugins/SmartStore.PayUMea/{controller}/{action}",
                new { controller = "PayUMeaExpress", action = "Index" },
                new[] { "SmartStore.PayUMea.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayUMea";

            routes.MapRoute("SmartStore.PayUMeaDirect",
                "Plugins/SmartStore.PayUMea/{controller}/{action}",
                new { controller = "PayUMeaDirect", action = "Index" },
                new[] { "SmartStore.PayUMea.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayUMea";

            routes.MapRoute("SmartStore.PayUMeaStandard",
                "Plugins/SmartStore.PayUMea/{controller}/{action}",
                new { controller = "PayUMeaStandard", action = "Index" },
                new[] { "SmartStore.PayUMea.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayUMea";

            //Legacay Routes
            routes.MapRoute("SmartStore.PayUMeaExpress.IPN",
                 "Plugins/PaymentPayUMeaExpress/IPNHandler",
                 new { controller = "PayUMeaExpress", action = "IPNHandler" },
                 new[] { "SmartStore.PayUMea.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayUMea";

            routes.MapRoute("SmartStore.PayUMeaDirect.IPN",
                 "Plugins/PaymentPayUMeaDirect/IPNHandler",
                 new { controller = "PayUMeaDirect", action = "IPNHandler" },
                 new[] { "SmartStore.PayUMea.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayUMea";

            routes.MapRoute("SmartStore.PayUMeaStandard.IPN",
                 "Plugins/PaymentPayUMeaStandard/IPNHandler",
                 new { controller = "PayUMeaStandard", action = "IPNHandler" },
                 new[] { "SmartStore.PayUMea.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayUMea";

            routes.MapRoute("SmartStore.PayUMeaStandard.PDT",
                 "Plugins/PaymentPayUMeaStandard/PDTHandler",
                 new { controller = "PayUMeaStandard", action = "PDTHandler" },
                 new[] { "SmartStore.PayUMea.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayUMea";

            routes.MapRoute("SmartStore.PayUMeaExpress.RedirectFromPaymentInfo",
                 "Plugins/PaymentPayUMeaExpress/RedirectFromPaymentInfo",
                 new { controller = "PayUMeaExpress", action = "RedirectFromPaymentInfo" },
                 new[] { "SmartStore.PayUMea.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayUMea";

            routes.MapRoute("SmartStore.PayUMeaStandard.CancelOrder",
                 "Plugins/PaymentPayUMeaStandard/CancelOrder",
                 new { controller = "PayUMeaStandard", action = "CancelOrder" },
                 new[] { "SmartStore.PayUMea.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.PayUMea";
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
