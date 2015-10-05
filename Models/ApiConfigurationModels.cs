using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.PayUMea.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.PayUMea.Models
{
    public abstract class ApiConfigurationModel: ModelBase
	{
        public string[] ConfigGroups { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayUMea.UseSandbox")]
		public bool UseSandbox { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayUMea.TransactMode")]
		public int TransactMode { get; set; }
		public SelectList TransactModeValues { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayUMea.ApiAccountName")]
		public string ApiAccountName { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayUMea.ApiAccountPassword")]
		[DataType(DataType.Password)]
		public string ApiAccountPassword { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayUMea.Signature")]
		public string Signature { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayUMea.AdditionalFee")]
		public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayUMea.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }
	}



}