using FluentValidation;
using SmartStore.PayUMea.Models;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Validators;

namespace SmartStore.PayUMea.Validators
{
	public class PayUMeaExpressPaymentInfoValidator : AbstractValidator<PayUMeaExpressPaymentInfoModel>
	{
		public PayUMeaExpressPaymentInfoValidator(ILocalizationService localizationService) {

		}
	}
}