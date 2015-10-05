using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartStore.Services.Payments;

namespace SmartStore.PayUMea.Services
{
    public class PayUMeaProcessPaymentRequest : ProcessPaymentRequest
    {
        /// <summary>
        /// Gets or sets an order Discount Amount
        /// </summary>
        public decimal Discount { get; set; }
    }
}
