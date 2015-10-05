using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Bindsoft.PayuIntergration;
using SmartStore.Core;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Logging;
using SmartStore.PayUMea.Models;
using SmartStore.PayUMea.Providers;
using SmartStore.PayUMea.Settings;
using SmartStore.Services;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.PayUMea.Controllers
{
	public class PayUMeaStandardController : PaymentControllerBase
	{
		private readonly IPaymentService _paymentService;
		private readonly IOrderService _orderService;
		private readonly IOrderProcessingService _orderProcessingService;
		private readonly IStoreContext _storeContext;
		private readonly IWorkContext _workContext;
		private readonly IWebHelper _webHelper;
		private readonly PaymentSettings _paymentSettings;
		private readonly ILocalizationService _localizationService;
		private readonly ICommonServices _services;
		private readonly IStoreService _storeService;

		public PayUMeaStandardController(
			IPaymentService paymentService, IOrderService orderService,
			IOrderProcessingService orderProcessingService,
			IStoreContext storeContext,
			IWorkContext workContext,
			IWebHelper webHelper,
			PaymentSettings paymentSettings,
			ILocalizationService localizationService,
			ICommonServices services,
			IStoreService storeService)
		{
			_paymentService = paymentService;
			_orderService = orderService;
			_orderProcessingService = orderProcessingService;
			_storeContext = storeContext;
			_workContext = workContext;
			_webHelper = webHelper;
			_paymentSettings = paymentSettings;
			_localizationService = localizationService;
			_services = services;
			_storeService = storeService;
		}

		[AdminAuthorize, ChildActionOnly]
		public ActionResult Configure()
		{
			var model = new PayUMeaStandardConfigurationModel();
			int storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _services.WorkContext);
			var settings = _services.Settings.LoadSetting<PayUMeaStandardPaymentSettings>(storeScope);

			model.Copy(settings, true);

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			storeDependingSettingHelper.GetOverrideKeys(settings, model, storeScope, _services.Settings);

			return View(model);
		}

		[HttpPost, AdminAuthorize, ChildActionOnly]
		public ActionResult Configure(PayUMeaStandardConfigurationModel model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return Configure();

			ModelState.Clear();

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			int storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _services.WorkContext);
			var settings = _services.Settings.LoadSetting<PayUMeaStandardPaymentSettings>(storeScope);

			model.Copy(settings, false);

			storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, _services.Settings);

			// multistore context not possible, see IPN handling
			_services.Settings.SaveSetting(settings, x => x.UseSandbox, 0, false);

			_services.Settings.ClearCache();
			NotifySuccess(_services.Localization.GetResource("Plugins.Payments.PayUMea.ConfigSaveNote"));

			return Configure();
		}

		public ActionResult PaymentInfo()
		{
			return PartialView();
		}

		[NonAction]
		public override IList<string> ValidatePaymentForm(FormCollection form)
		{
			var warnings = new List<string>();
			return warnings;
		}

		[NonAction]
		public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
		{
			var paymentInfo = new ProcessPaymentRequest();
			return paymentInfo;
		}

		[ValidateInput(false)]
		public ActionResult PDTHandler()
		{

			var provider = _paymentService.LoadPaymentMethodBySystemName("Payments.PayUStandard", true);
			var processor = provider != null ? provider.Value as Providers.PayUMeaStandardProvider : null;
			if (processor == null)
				throw new SmartException(_localizationService.GetResource("Plugins.Payments.PayUMeaStandard.NoModuleLoading"));
			string orderNumber = Request.QueryString["orderId"];
		
			var order = _orderService.GetOrderById(int.Parse(orderNumber));
			if (order == null)
			{
				throw new SmartException(string.Format("Could not locate order with ID {0}",orderNumber));
			}
			//order note
			order.OrderNotes.Add(new OrderNote()
			{
				Note = Request.QueryString["PayUReference"],
				DisplayToCustomer = false,
				CreatedOnUtc = DateTime.UtcNow
			});
			order.AuthorizationTransactionCode = Request.QueryString["PayUReference"];
			_orderService.UpdateOrder(order);
			//mark order as paid
			if (!_orderProcessingService.CanMarkOrderAsPaid(order))
				return RedirectToAction("Completed", "Checkout", new { area = "" });
			order.AuthorizationTransactionId = Request.QueryString["PayUReference"];
			_orderService.UpdateOrder(order);
			_orderProcessingService.MarkOrderAsPaid(order);
			return RedirectToAction("Completed", "Checkout", new { area = "" });
		}

		[ValidateInput(false)]
		public ActionResult IPNHandler()
		{
			Debug.WriteLine("PayUMea Standard IPN: {0}".FormatWith(Request.ContentLength));

			var provider = _paymentService.LoadPaymentMethodBySystemName("Payments.PayUMeaStandard", true);
			var processor = provider != null ? provider.Value as PayUMeaStandardProvider : null;
			if (processor == null)
				throw new SmartException(_localizationService.GetResource("Plugins.Payments.PayUMeaStandard.NoModuleLoading"));


			var ueasyPaymentSettings = _services.Settings.LoadSetting<PayUMeaStandardPaymentSettings>();
			var runPayUcomms = new RunPayUComms(ueasyPaymentSettings.Username, ueasyPaymentSettings.Password, ueasyPaymentSettings.ApiUrl, ueasyPaymentSettings.SafeKey, "ONE_ZERO");
			var transactionRequest1 = new GetTransactionRequest
			{
				PayUReference = Request.QueryString["payUReference"]
			};
			var transaction = runPayUcomms.GetTransaction(transactionRequest1);
			var orderByNumber = _orderService.GetOrderByNumber(Request.QueryString["orderId"]);
			switch (transaction.TransactionState)
			{
				case "RESERVE":
					orderByNumber.AuthorizationTransactionId = Request.QueryString["payUReference"];
					_orderService.UpdateOrder(orderByNumber);
					_orderProcessingService.MarkAsAuthorized(orderByNumber);
					break;
				case "RESERVE_CANCEL":
					orderByNumber.AuthorizationTransactionId = Request.QueryString["payUReference"];
					_orderService.UpdateOrder(orderByNumber);
					_orderProcessingService.CancelOrder(orderByNumber, true);
					break;
				case "PAYMENT":
					orderByNumber.AuthorizationTransactionId = Request.QueryString["payUReference"];
					_orderService.UpdateOrder(orderByNumber);
					_orderProcessingService.MarkOrderAsPaid(orderByNumber);
					break;
				case "FINALIZE":
					orderByNumber.AuthorizationTransactionId = Request.QueryString["payUReference"];
					_orderService.UpdateOrder(orderByNumber);
					_orderProcessingService.MarkOrderAsPaid(orderByNumber);
					break;
			}
			return Content("");
		}


		/// <summary>
		/// Gets a payment status
		/// </summary>
		/// <param name="paymentStatus">PayUMea payment status</param>
		/// <param name="pendingReason">PayUMea pending reason</param>
		/// <returns>Payment status</returns>
		public PaymentStatus GetPaymentStatus(string paymentStatus, string pendingReason)
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

		public ActionResult CancelOrder(FormCollection form)
		{
			var order = _orderService.SearchOrders(_storeContext.CurrentStore.Id, _workContext.CurrentCustomer.Id, null, null, null, null, null, null, null, null, 0, 1)
				.FirstOrDefault();

			if (order != null)
			{
				return RedirectToAction("Details", "Order", new { id = order.Id, area = "" });
			}

			return RedirectToAction("Index", "Home", new { area = "" });
		}
	}
}