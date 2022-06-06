
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stripe;

namespace Code4GoodBackEnd.Services
{
    public interface IPaymentsGateway
    {
        
        Task<CustomerModel> GetCustomerByEmail(string email, params PaymentModelInclude[] include);
        

        Task<List<PaymentMethodModel>> GetPaymentMethods(string customerId, PaymentMethodType paymentMethodType);
        Task<List<PaymentMethodModel>> GetPaymentMethodsByCustomerEmail(string customerEmail, PaymentMethodType paymentMethodType);

        
        Task<PaymentIntent> Charge(string customerId, string paymentMethodId, string Currency, long unitAmount,
            string customerEmail, bool sendEmailAfterSuccess = false, string emailDescription = "");

        Task<PaymentIntent> ChargeWithCustomerEmail(string customerEmail, string paymentMethodId, string Currency, long unitAmount,
            bool sendEmailAfterSuccess = false, string emailDescription = "");

    }
}