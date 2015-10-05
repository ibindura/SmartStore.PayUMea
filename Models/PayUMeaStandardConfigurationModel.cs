using System.ComponentModel;
using SmartStore.PayUMea.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.PayUMea.Models
{
    public class PayUMeaStandardConfigurationModel : ModelBase
	{
        [SmartResourceDisplayName("Plugins.Payments.PayUMea.UseSandbox")]
		public bool UseSandbox { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayUMeaStandard.Fields.BusinessEmail")]
		public string BusinessEmail { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayUMeaStandard.Fields.PDTToken")]
		public string PdtToken { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayUMeaStandard.Fields.PDTValidateOrderTotal")]
		public bool PdtValidateOrderTotal { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayUMeaStandard.Fields.AdditionalFee")]
		public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayUMeaStandard.Fields.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayUMeaStandard.Fields.PassProductNamesAndTotals")]
		public bool PassProductNamesAndTotals { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayUMeaStandard.Fields.EnableIpn")]
		public bool EnableIpn { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayUMeaStandard.Fields.IpnUrl")]
		public string IpnUrl { get; set; }

		public string SafeKey { get; set; }

		public string RedirectUrl { get; set; }

		public string Password { get; set; }

		public string ApiUrl { get; set; }

		public string Username { get; set; }
        public void Copy(PayUMeaStandardPaymentSettings settings, bool fromSettings)
        {
            if (fromSettings)
            {
                UseSandbox = settings.UseSandbox;
                AdditionalFee = settings.AdditionalFee;
                AdditionalFeePercentage = settings.AdditionalFeePercentage;
                EnableIpn = settings.EnableIpn;
                IpnUrl = settings.IpnUrl;
	            SafeKey = settings.SafeKey;
	            Username = settings.Username;
	            ApiUrl = settings.ApiUrl;
	            Password = settings.Password;
	            RedirectUrl = settings.RedirectUrl;
            }
            else
            {
                settings.UseSandbox = UseSandbox;
				settings.SafeKey = SafeKey;
                settings.Username = Username;
				settings.ApiUrl = ApiUrl;
                settings.AdditionalFee = AdditionalFee;
                settings.AdditionalFeePercentage = AdditionalFeePercentage;
				settings.Password = Password;
                settings.EnableIpn = EnableIpn;
				settings.RedirectUrl = RedirectUrl;
                settings.IpnUrl = IpnUrl;
            }

        }

	}
}