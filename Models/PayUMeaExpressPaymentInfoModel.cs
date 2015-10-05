using SmartStore.Web.Framework.Mvc;

namespace SmartStore.PayUMea.Models
{
    public class PayUMeaExpressPaymentInfoModel : ModelBase
    {
        public PayUMeaExpressPaymentInfoModel()
        {
            
        }

        public bool CurrentPageIsBasket { get; set; }

        public string SubmitButtonImageUrl { get; set; }

    }
}