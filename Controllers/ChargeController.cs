using Microsoft.AspNetCore.Mvc;
using Code4GoodBackEnd.Services;
using Stripe;

namespace Code4GoodBackEnd.Controllers;

[ApiController]
[Route("[controller]")]
public class ChargeController : ControllerBase
{


        private readonly IPaymentsGateway _paymentsGateway;

        public ChargeController(IPaymentsGateway paymentsGateway)
        {
            _paymentsGateway = paymentsGateway;
        }


    [HttpPost(Name = "PostCreateCharge")]
        public async Task<Stripe.PaymentIntent> Create(string customerEmail, string paymentMethodId, string Currency, decimal amount)
        {
          var intent  = await _paymentsGateway.ChargeWithCustomerEmail(customerEmail, paymentMethodId, Currency, (long)amount * 100);
            
            return intent;
            
            //return StatusCode(StatusCodes.Status200OK, "The customer was successfully charged");
        }


}

