using System;
using SmartStore.Core.Configuration;

namespace SmartStore.PayUMea.Settings
{
	public class PayUMeaStandardPaymentSettings : PayUMeaSettingsBase, ISettings
	{
		public bool EnableIpn { get; set; }

		public string IpnUrl { get; set; }

		public string SafeKey { get; set; }

		public string Username { get; set; }

		public string ApiUrl { get; set; }

		public string Password { get; set; }

		public string RedirectUrl { get; set; }

		public PayUMeaStandardPaymentSettings()
		{
			this.EnableIpn = true;
		}
	}
}