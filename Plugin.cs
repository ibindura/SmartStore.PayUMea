using SmartStore.Core.Plugins;
using SmartStore.PayUMea.Settings;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;

namespace SmartStore.PayUMea
{
	public class Plugin : BasePlugin
	{
		private readonly ISettingService _settingService;
		private readonly ILocalizationService _localizationService;

		public Plugin(
			ISettingService settingService,
			ILocalizationService localizationService)
		{
			_settingService = settingService;
			_localizationService = localizationService;
		}

		public override void Install()
		{
			_settingService.SaveSetting<PayUMeaStandardPaymentSettings>(new PayUMeaStandardPaymentSettings());

			_localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

			base.Install();
		}

		public override void Uninstall()
		{
            _settingService.DeleteSetting<PayUMeaStandardPaymentSettings>();

			_localizationService.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

			base.Uninstall();
		}
	}
}
