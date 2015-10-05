namespace SmartStore.PayUMea.Settings
{
	public abstract class PayUMeaApiSettingsBase : PayUMeaSettingsBase
	{
		public TransactMode TransactMode { get; set; }
		public string ApiAccountName { get; set; }
		public string ApiAccountPassword { get; set; }
		public string SafeKey { get; set; }
	}
}