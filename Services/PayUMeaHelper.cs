using System;
using System.Net;
using System.Text;
using System.Web.Routing;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Payments;
using SmartStore.PayUMea.Settings;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.PayUMea.Services
{
    /// <summary>
    /// Represents paypal helper
    /// </summary>
    public static class PayUMeaHelper
    {
        /// <summary>
        /// Gets a payment status
        /// </summary>
        /// <param name="paymentStatus">PayUMea payment status</param>
        /// <param name="pendingReason">PayUMea pending reason</param>
        /// <returns>Payment status</returns>
        public static PaymentStatus GetPaymentStatus(string paymentStatus, string pendingReason)
        {
            var result = PaymentStatus.Pending;

            if (paymentStatus == null)
                paymentStatus = string.Empty;

            if (pendingReason == null)
                pendingReason = string.Empty;

            switch (paymentStatus.ToLowerInvariant())
            {
                case "pending":
                    switch (pendingReason.ToLowerInvariant())
                    {
                        case "authorization":
                            result = PaymentStatus.Authorized;
                            break;
                        default:
                            result = PaymentStatus.Pending;
                            break;
                    }
                    break;
                case "processed":
                case "completed":
                case "canceled_reversal":
                    result = PaymentStatus.Paid;
                    break;
                case "denied":
                case "expired":
                case "failed":
                case "voided":
                    result = PaymentStatus.Voided;
                    break;
                case "refunded":
                case "reversed":
                    result = PaymentStatus.Refunded;
                    break;
                default:
                    break;
            }
            return result;
        }

    

        public static string CheckIfButtonExists(string buttonUrl) 
        { 
        
            HttpWebResponse response = null;
            var request = (HttpWebRequest)WebRequest.Create(buttonUrl);
            request.Method = "HEAD";

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                return buttonUrl;
            }
            catch (WebException)
            {
                /* A WebException will be thrown if the status of the response is not `200 OK` */
                return "https://www.paypalobjects.com/en_US/i/btn/btn_xpressCheckout.gif";
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
            

        }

        public static bool CurrentPageIsBasket(RouteData routeData) 
        {
            return routeData.GetRequiredString("controller").IsCaseInsensitiveEqual("ShoppingCart")
                && routeData.GetRequiredString("action").IsCaseInsensitiveEqual("Cart");
        }

        //TODO: join the following two methods, with help of payment method type

        /// <summary>
        /// Gets Paypal URL
        /// </summary>
        /// <returns></returns>
        public static string GetPaypalUrl(PayUMeaSettingsBase settings)
        {
            return settings.UseSandbox ?
                "https://www.sandbox.paypal.com/cgi-bin/webscr" :
                "https://www.paypal.com/cgi-bin/webscr";
        }

        /// <summary>
        /// Gets Paypal URL
        /// </summary>
        /// <returns></returns>
        public static string GetPaypalServiceUrl(PayUMeaSettingsBase settings)
        {
            return settings.UseSandbox ?
                "https://api-3t.sandbox.paypal.com/2.0/" :
                "https://api-3t.paypal.com/2.0/";
        }

        public static string GetApiVersion()
        {
            return "1";
        }


    }
}

