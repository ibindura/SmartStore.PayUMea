using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Bindsoft.PayuIntergration;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.PayUMea.Controllers;
using SmartStore.PayUMea.Services;
using SmartStore.PayUMea.Settings;
using SmartStore.Services;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.PayUMea.Providers
{
	/// <summary>
	/// PayUStandard provider
	/// </summary>
	[SystemName("Payments.PayUStandard")]
	[FriendlyName("PayU Standard")]
	[DisplayOrder(2)]
	public partial class PayUMeaStandardProvider : PaymentPluginBase, IConfigurable
	{
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;
		private readonly ICommonServices _services;
		private readonly ILogger _logger;
		private readonly HttpContextBase _httpContext;
		private RunPayUComms _payUApiComms;

		public PayUMeaStandardProvider(
			IOrderTotalCalculationService orderTotalCalculationService,
			ICommonServices services,
			ILogger logger,
			HttpContextBase httpContext)
		{
			_orderTotalCalculationService = orderTotalCalculationService;
			_services = services;
			_logger = logger;
			_httpContext = httpContext;
		}

		/// <summary>
		/// Process a payment
		/// </summary>
		/// <param name="processPaymentRequest">Payment info required for an order processing</param>
		/// <returns>Process payment result</returns>
		public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
		{
			var result = new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending };


			return result;
		}

		/// <summary>
		/// Captures payment
		/// </summary>
		/// <param name="capturePaymentRequest">Capture payment request</param>
		/// <returns>Capture payment result</returns>
		public override CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
		{
			var result = new CapturePaymentResult()
			{
				NewPaymentStatus = PaymentStatus.Pending,
				CaptureTransactionResult = "successful"
			};

			return result;
		}

		/// <summary>
		/// Post process payment (used by payment gateways that require redirecting to a third-party URL)
		/// </summary>
		/// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
		public override void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
		{
			if (postProcessPaymentRequest.Order.PaymentStatus == PaymentStatus.Paid)
				return;
			var settings = _services.Settings.LoadSetting<PayUMeaStandardPaymentSettings>(postProcessPaymentRequest.Order.StoreId);
			if (_payUApiComms == null)
			{

				_payUApiComms = new RunPayUComms(settings.Username, settings.Password, settings.ApiUrl, settings.SafeKey, "ONE_ZERO");
			}
			string cancelReturnUrl = _services.WebHelper.GetStoreLocation(false) + "Plugins/PaymentPayUPayUMeaStandard/CancelOrder";
			var orderid = postProcessPaymentRequest.Order.GetOrderNumber();
			string returnUrl = string.Format("{0}Plugins/PaymentPayUMeaStandard/PDTHandler?orderId={1}", _services.WebHelper.GetStoreLocation(false), orderid);
			var setTransactionResult = _payUApiComms.SetTransaction(new SetTransactionRequest
			{
				AmountInCents = Math.Truncate(postProcessPaymentRequest.Order.OrderTotal * 100).ToString(CultureInfo.InvariantCulture),
				CancelUrl = cancelReturnUrl,
				CustomerEmail = postProcessPaymentRequest.Order.BillingAddress.Email,
				CustomerLastname = postProcessPaymentRequest.Order.BillingAddress.LastName,
				CustomerMobile = postProcessPaymentRequest.Order.BillingAddress.PhoneNumber,
				CustomerName = postProcessPaymentRequest.Order.BillingAddress.FirstName,
				CustomerUsername = postProcessPaymentRequest.Order.Customer.Username,
				LineItems = postProcessPaymentRequest.Order.OrderItems.Select(x => new LineItem
				{
					Amount = Math.Truncate(x.PriceInclTax * 100).ToString(CultureInfo.InvariantCulture),
					Description = x.Product.Name,
					ProductCode = x.Product.Sku,
					Quantity = x.Quantity.ToString()
				}).ToList(),
				OrderNumber =  postProcessPaymentRequest.Order.OrderGuid.ToString(),
				PaymentTypes = null,
				ReturnUrl = returnUrl,
				Stage = false
			});
			if (!setTransactionResult.IsTransactionSet)
				throw new Exception(string.Format("Failed to Preprocess transaction with PayU ### {0} ###>> {1}:{2}:{3}", setTransactionResult.ErrorMessage, postProcessPaymentRequest.Order.BillingAddress.Email, postProcessPaymentRequest.Order.OrderNumber, postProcessPaymentRequest.Order.OrderGuid));
			_httpContext.Response.Redirect(string.Format("{0}?PayUReference={1}", settings.RedirectUrl, setTransactionResult.PayUReference));
		}

		/// <summary>
		/// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
		/// </summary>
		/// <param name="order">Order</param>
		/// <returns>Result</returns>
		public override bool CanRePostProcessPayment(Order order)
		{
			if (order == null)
				throw new ArgumentNullException("order");

			return true;
		}

		public override Type GetControllerType()
		{ 
			return typeof(PayUMeaStandardController);
		}

		public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
		{
			var result = decimal.Zero;
			try
			{
				var settings = _services.Settings.LoadSetting<PayUMeaStandardPaymentSettings>(_services.StoreContext.CurrentStore.Id);

				result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart, settings.AdditionalFee, settings.AdditionalFeePercentage);
			}
			catch (Exception)
			{
				// ignored
			}
			return result;
		}



		/// <summary>
		/// Splits the difference of two value into a portion value (for each item) and a rest value
		/// </summary>
		/// <param name="difference">The difference value</param>
		/// <param name="numberOfLines">Number of lines\items to split the difference</param>
		/// <param name="portion">Portion value</param>
		/// <param name="rest">Rest value</param>
		private void SplitDifference(decimal difference, int numberOfLines, out decimal portion, out decimal rest)
		{
			portion = rest = decimal.Zero;

			if (numberOfLines == 0)
				numberOfLines = 1;

			int intDifference = (int)(difference * 100);
			int intPortion = (int)Math.Truncate((double)intDifference / (double)numberOfLines);
			int intRest = intDifference % numberOfLines;

			portion = Math.Round(((decimal)intPortion) / 100, 2);
			rest = Math.Round(((decimal)intRest) / 100, 2);

			Debug.Assert(difference == ((numberOfLines * portion) + rest));
		}

		/// <summary>
		/// Get all PayU line items
		/// </summary>
		/// <param name="postProcessPaymentRequest">Post process paymenmt request object</param>
		/// <param name="checkoutAttributeValues">List with checkout attribute values</param>
		/// <param name="cartTotal">Receives the calculated cart total amount</param>
		/// <returns>All items for PayU Standard API</returns>
		public List<PayUMeaLineItem> GetLineItems(PostProcessPaymentRequest postProcessPaymentRequest, out decimal cartTotal)
		{
			cartTotal = decimal.Zero;

			var order = postProcessPaymentRequest.Order;
			var lst = new List<PayUMeaLineItem>();

			// order items
			foreach (var orderItem in order.OrderItems)
			{
				var item = new PayUMeaLineItem()
				{
					Type = PayUMeaItemType.CartItem,
					Name = orderItem.Product.GetLocalized(x => x.Name),
					Quantity = orderItem.Quantity,
					Amount = orderItem.UnitPriceExclTax
				};
				lst.Add(item);

				cartTotal += orderItem.PriceExclTax;
			}
			if (order.OrderShippingExclTax > decimal.Zero)
			{
				var item = new PayUMeaLineItem()
				{
					Type = PayUMeaItemType.Shipping,
					Name = T("Plugins.Payments.PayUStandard.ShippingFee").Text,
					Quantity = 1,
					Amount = order.OrderShippingExclTax
				};
				lst.Add(item);

				cartTotal += order.OrderShippingExclTax;
			}

			// payment fee
			if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
			{
				var item = new PayUMeaLineItem()
				{
					Type = PayUMeaItemType.PaymentFee,
					Name = T("Plugins.Payments.PayUStandard.PaymentMethodFee").Text,
					Quantity = 1,
					Amount = order.PaymentMethodAdditionalFeeExclTax
				};
				lst.Add(item);

				cartTotal += order.PaymentMethodAdditionalFeeExclTax;
			}

			// tax
			if (order.OrderTax > decimal.Zero)
			{
				var item = new PayUMeaLineItem()
				{
					Type = PayUMeaItemType.Tax,
					Name = T("Plugins.Payments.PayUStandard.SalesTax").Text,
					Quantity = 1,
					Amount = order.OrderTax
				};
				lst.Add(item);

				cartTotal += order.OrderTax;
			}

			return lst;
		}

		/// <summary>
		/// Manually adjusts the net prices for cart items to avoid rounding differences with the PayU API.
		/// </summary>
		/// <param name="paypalItems">PayU line items</param>
		/// <param name="postProcessPaymentRequest">Post process paymenmt request object</param>
		/// <remarks>
		/// In detail: We add what we have thrown away in the checkout when we rounded prices to two decimal places.
		/// It's a workaround. Better solution would be to store the thrown away decimal places for each OrderItem in the database.
		/// More details: http://magento.xonu.de/magento-extensions/empfehlungen/magento-paypal-rounding-error-fix/
		/// </remarks>
		public void AdjustLineItemAmounts(List<PayUMeaLineItem> paypalItems, PostProcessPaymentRequest postProcessPaymentRequest)
		{
			try
			{
				var cartItems = paypalItems.Where(x => x.Type == PayUMeaItemType.CartItem);

				if (cartItems.Count() <= 0)
					return;

				decimal totalSmartStore = Math.Round(postProcessPaymentRequest.Order.OrderSubtotalExclTax, 2);
				decimal totalPayU = decimal.Zero;
				decimal delta, portion, rest;

				// calculate what PayU calculates
				cartItems.Each(x => totalPayU += (x.AmountRounded * x.Quantity));
				totalPayU = Math.Round(totalPayU, 2, MidpointRounding.AwayFromZero);

				// calculate difference
				delta = Math.Round(totalSmartStore - totalPayU, 2);
				if (delta == decimal.Zero)
					return;

				// prepare lines... only lines with quantity = 1 are adjustable. if there is no one, create one.
				if (!cartItems.Any(x => x.Quantity == 1))
				{
					var item = cartItems.First(x => x.Quantity > 1);
					item.Quantity -= 1;
					var newItem = item.Clone();
					newItem.Quantity = 1;
					paypalItems.Insert(paypalItems.IndexOf(item) + 1, newItem);
				}

				var cartItemsOneQuantity = paypalItems.Where(x => x.Type == PayUMeaItemType.CartItem && x.Quantity == 1);
				Debug.Assert(cartItemsOneQuantity.Count() > 0);

				SplitDifference(delta, cartItemsOneQuantity.Count(), out portion, out rest);

				if (portion != decimal.Zero)
				{
					cartItems
						.Where(x => x.Quantity == 1)
						.Each(x => x.Amount = x.Amount + portion);
				}

				if (rest != decimal.Zero)
				{
					var restItem = cartItems.First(x => x.Quantity == 1);
					restItem.Amount = restItem.Amount + rest;
				}

				//"SM: {0}, PP: {1}, delta: {2} (portion: {3}, rest: {4})".FormatWith(totalSmartStore, totalPayU, delta, portion, rest).Dump();
			}
			catch (Exception exc)
			{
				_logger.Error(exc.Message, exc);
			}
		}




		/// <summary>
		/// Gets a route for provider configuration
		/// </summary>
		/// <param name="actionName">Action name</param>
		/// <param name="controllerName">Controller name</param>
		/// <param name="routeValues">Route values</param>
		public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			actionName = "Configure";
			controllerName = "PayUMeaStandard";
			routeValues = new RouteValueDictionary() { { "area", "SmartStore.PayUMea" } };
		}

		/// <summary>
		/// Gets a route for payment info
		/// </summary>
		/// <param name="actionName">Action name</param>
		/// <param name="controllerName">Controller name</param>
		/// <param name="routeValues">Route values</param>
		public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			actionName = "PaymentInfo";
			controllerName = "PayUMeaStandard";
			routeValues = new RouteValueDictionary() { { "area", "SmartStore.PayUMea" } };
		}

		#region Properties

		public override PaymentMethodType PaymentMethodType
		{
			get
			{
				return PaymentMethodType.Redirection;
			}
		}

		public override bool SupportCapture
		{
			get { return true; }
		}

		public override bool SupportVoid
		{
			get { return true; }
		}

		#endregion
	}
}
