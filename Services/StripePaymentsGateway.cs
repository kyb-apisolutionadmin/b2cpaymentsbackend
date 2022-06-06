
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Stripe;


namespace Code4GoodBackEnd.Services
{
    public class StripePaymentsGateway : IPaymentsGateway
    {
        private readonly ILogger<StripePaymentsGateway> _logger;

        public StripePaymentsGateway(ILogger<StripePaymentsGateway> logger, string apiKey)
        {
            _logger = logger;

            
            StripeConfiguration.ApiKey = apiKey;
        }

        public async Task<CustomerModel> GetCustomerByEmail(string email, params PaymentModelInclude[] includes)
        {
            var service = new CustomerService();
            var stripeCustomers = await service.ListAsync(new CustomerListOptions()
            {
                Email = email
            });

            if (!stripeCustomers.Any())
                return null;

            var stripeCustomer = stripeCustomers.Single();

            var customerModel = new CustomerModel(stripeCustomer.Id)
            {
                Email = email,
                Name = stripeCustomer.Name
            };
            if (includes.Any() && includes.Contains(PaymentModelInclude.PaymentMethods))
            {
                var paymentMethods = await this.GetPaymentMethods(stripeCustomer.Id, PaymentMethodType.Card);
                customerModel.PaymentMethods = paymentMethods;
            }

            return customerModel;
        }

       


        public async Task<List<PaymentMethodModel>> GetPaymentMethods(string customerId, PaymentMethodType paymentMethodType)
        {
            var options = new PaymentMethodListOptions
            {
                Customer = customerId,
                Type = paymentMethodType.ToString().ToLower()
            };

            var service = new PaymentMethodService();
            var paymentMethods = await service.ListAsync(options);


            List<PaymentMethodModel> result = new List<PaymentMethodModel>();
            foreach (var stripePaymentMethod in paymentMethods)
            {
                if (!Enum.TryParse(stripePaymentMethod.Type, true, out PaymentMethodType currPaymentMethodType))
                {
                    this._logger.LogError($"Cannot find PAYMENT_METHOD_TYPE:{stripePaymentMethod.Type}");
                    continue;
                }

                PaymentMethodModel currentPaymentMethod = new PaymentMethodModel(stripePaymentMethod.Id)
                {
                    Type = currPaymentMethodType
                };

                if (currPaymentMethodType == PaymentMethodType.Card)
                {
                    currentPaymentMethod.Card = new PaymentMethodCardModel()
                    {
                        Brand = stripePaymentMethod.Card.Brand,
                        Country = stripePaymentMethod.Card.Country,
                        ExpMonth = stripePaymentMethod.Card.ExpMonth,
                        ExpYear = stripePaymentMethod.Card.ExpYear,
                        Issuer = stripePaymentMethod.Card.Issuer,
                        Last4 = stripePaymentMethod.Card.Last4,
                        Description = stripePaymentMethod.Card.Description,
                        Fingerprint = stripePaymentMethod.Card.Fingerprint,
                        Funding = stripePaymentMethod.Card.Funding,
                        Iin = stripePaymentMethod.Card.Iin
                    };
                }

                result.Add(currentPaymentMethod);
            }
            return result;
        }

        public async Task<List<PaymentMethodModel>> GetPaymentMethodsByCustomerEmail(string customerEmail, PaymentMethodType paymentMethodType)
        {
            CustomerModel customer = await this.GetCustomerByEmail(customerEmail);

            return await this.GetPaymentMethods(customer.Id, paymentMethodType);
        }


        public async Task<PaymentIntent> ChargeWithCustomerEmail(string customerEmail, string paymentMethodId, string Currency, long unitAmount,
            bool sendEmailAfterSuccess = false, string emailDescription = "")
        {
            var customer = await GetCustomerByEmail(customerEmail);
          var intent =  await Charge(customer.Id, paymentMethodId, Currency, unitAmount, customerEmail, sendEmailAfterSuccess, emailDescription);
            return intent;
        
        }

        
        public async Task<PaymentIntent> Charge(string customerId, string paymentMethodId,
            string Currency, long unitAmount, string customerEmail, bool sendEmailAfterSuccess = false, string emailDescription = "")
        {

            

            try
            {
                var service = new PaymentIntentService();
                var options = new PaymentIntentCreateOptions
                {
                    Amount = unitAmount,
                    Currency = Currency.ToString().ToLower(),
                    Customer = customerId,
                    PaymentMethod = paymentMethodId,
                    Confirm = true,
                    OffSession = true,
                    ReceiptEmail = sendEmailAfterSuccess ? customerEmail : null,
                    Description = emailDescription,
                };
                var intent = await service.CreateAsync(options);
                 var charges = intent.Charges.Data;
                

            Console.WriteLine("Returning intent after charging as " + intent);


               return intent;
                
                
            }
            catch (StripeException e)
            {
                switch (e.StripeError.Type)
                {
                    case "card_error":
                        // Error code will be authentication_required if authentication is needed
                        Console.WriteLine("Error code: " + e.StripeError.Code);
                        var paymentIntentIderror = e.StripeError.PaymentIntent.Id;
                        var serviceerror = new PaymentIntentService();
                        var paymentIntent2 = serviceerror.Get(paymentIntentIderror);

                        Console.WriteLine(paymentIntent2.Id);

                        
                        break;
                        
                    default:
                        break;

                         
                }

                   var paymentIntentId = e.StripeError.PaymentIntent.Id;
                        var service = new PaymentIntentService();
                        var paymentIntent = service.Get(paymentIntentId);

                return paymentIntent;

            }
                

           

        }


       
    }
}