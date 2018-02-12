﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.ComponentModel;
using SmartStore.PayPal.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.PayPal.Models
{
	public abstract class ApiConfigurationModel : ModelBase
	{
        public string[] ConfigGroups { get; set; }
		public string PrimaryStoreCurrencyCode { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.UseSandbox")]
		public bool UseSandbox { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.IpnChangesPaymentStatus")]
		public bool IpnChangesPaymentStatus { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.TransactMode")]
		public TransactMode TransactMode { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.ApiAccountName")]
		public string ApiAccountName { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.ApiAccountPassword")]
		[DataType(DataType.Password)]
		public string ApiAccountPassword { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.Signature")]
		public string Signature { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.PayPal.ClientId")]
		public string ClientId { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.PayPal.Secret")]
		public string Secret { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.PayPal.ExperienceProfileId")]
		public string ExperienceProfileId { get; set; }

		[SmartResourceDisplayName("Plugins.SmartStore.PayPal.WebhookId")]
		public string WebhookId { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.AdditionalFee")]
		public decimal AdditionalFee { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPal.AdditionalFeePercentage")]
		public bool AdditionalFeePercentage { get; set; }
	}

    public class PayPalDirectConfigurationModel : ApiConfigurationModel
    {
        public void Copy(PayPalDirectPaymentSettings settings, bool fromSettings)
        {
            if (fromSettings)
				MiniMapper.Map(settings, this);
            else
				MiniMapper.Map(this, settings);
        }
    }

    public class PayPalExpressConfigurationModel : ApiConfigurationModel
    {
		[SmartResourceDisplayName("Plugins.Payments.PayPalExpress.Fields.ShowButtonInMiniShoppingCart")]
		public bool ShowButtonInMiniShoppingCart { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalExpress.Fields.ConfirmedShipment")]
        public bool ConfirmedShipment { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPalExpress.Fields.NoShipmentAddress")]
        public bool NoShipmentAddress { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPalExpress.Fields.CallbackTimeout")]
        public int CallbackTimeout { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPalExpress.Fields.DefaultShippingPrice")]
        public decimal DefaultShippingPrice { get; set; }

        public void Copy(PayPalExpressPaymentSettings settings, bool fromSettings)
        {
            if (fromSettings)
				MiniMapper.Map(settings, this);
            else
				MiniMapper.Map(this, settings);
        }
    }


	public class PayPalPlusConfigurationModel : ApiConfigurationModel
	{
		public PayPalPlusConfigurationModel()
		{
			TransactMode = TransactMode.AuthorizeAndCapture;
		}

		[SmartResourceDisplayName("Plugins.Payments.PayPalPlus.ThirdPartyPaymentMethods")]
		public List<string> ThirdPartyPaymentMethods { get; set; }
		public List<SelectListItem> AvailableThirdPartyPaymentMethods { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalPlus.DisplayPaymentMethodLogo")]
		public bool DisplayPaymentMethodLogo { get; set; }

		[SmartResourceDisplayName("Plugins.Payments.PayPalPlus.DisplayPaymentMethodDescription")]
		public bool DisplayPaymentMethodDescription { get; set; }


		public void Copy(PayPalPlusPaymentSettings settings, bool fromSettings)
		{
			if (fromSettings)
			{
				MiniMapper.Map(settings, this);
			}
			else
			{
				MiniMapper.Map(this, settings);
			}
		}
	}
}