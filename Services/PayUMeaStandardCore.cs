using System;
using System.Globalization;

namespace SmartStore.PayUMea.Services
{
	public class PayUMeaLineItem : ICloneable<PayUMeaLineItem>
	{
		public PayUMeaItemType Type { get; set; }
		public string Name { get; set; }
		public int Quantity { get; set; }
		public decimal Amount { get; set; }

		public decimal AmountRounded
		{
			get
			{
				return Math.Round(Amount, 2);
			}
		}

		public PayUMeaLineItem Clone()
		{
			var item = new PayUMeaLineItem()
			{
				Type = this.Type,
				Name = this.Name,
				Quantity = this.Quantity,
				Amount = this.Amount
			};
			return item;
		}

		object ICloneable.Clone()
		{
			return this.Clone();
		}
	}


	public enum PayUMeaItemType : int
	{
		CartItem = 0,
		CheckoutAttribute,
		Shipping,
		PaymentFee,
		Tax
	}
}
