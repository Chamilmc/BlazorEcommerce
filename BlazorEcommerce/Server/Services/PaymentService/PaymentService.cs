﻿using Stripe;
using Stripe.Checkout;

namespace BlazorEcommerce.Server.Services.PaymentService
{
    public class PaymentService : IPaymentService
    {
        private readonly ICartService _cartService;
        private readonly IAuthService _authService;
        private readonly IOrderService _orderService;
        const string secret = "";

        public PaymentService(ICartService cartService,
            IAuthService authService,
            IOrderService orderService)
        {
            StripeConfiguration.ApiKey = "sk_test_51MQoBrKCPLY6bQeYYah5ezUzeqAMIHhhuSB4A2Z1ecrmn1ybA4MW7ZsXwKiQCnlOqRLeBxR3ywCyq1KZ5C3R0EpE00DyoMnZ6o";
            _cartService = cartService;
            _authService = authService;
            _orderService = orderService;
        }

        public async Task<Session> CreateCheckourSession()
        {
            var products = (await _cartService.GetDbCartProducts()).Data;
            var lineItem = new List<SessionLineItemOptions>();
            products.ForEach(product => lineItem.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmountDecimal = product.Price * 100,
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = product.Title,
                        Images = new List<string> { product.ImageUrl }
                    }
                },
                Quantity = product.Quantity
            }));

            var options = new SessionCreateOptions
            {
                CustomerEmail = _authService.GetUserEmail(),
                ShippingAddressCollection = new SessionShippingAddressCollectionOptions
                {
                    AllowedCountries = new List<string>()
                    {
                        "US"
                    }
                },
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                LineItems = lineItem,
                Mode = "payment",
                SuccessUrl = "https://localhost:7173/order-success",
                CancelUrl = "https://localhost:7173/cart"
            };

            var service = new SessionService();
            Session session = service.Create(options);
            return session;
        }

        public async Task<ServiceResponse<bool>> FulfillOrder(HttpRequest request)
        {
            var json = await new StreamReader(request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    request.Headers["Stripe-Signature"],
                    secret
                    );

                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;
                    var user = await _authService.GetUserByEmail(session.CustomerEmail);
                    await _orderService.PlaceOrder(user.Id);
                }

                return new ServiceResponse<bool> { Data = true };
            }
            catch (StripeException e)
            {
                return new ServiceResponse<bool> { Data = false, Scecess = false, Message = e.Message };
            }
        }
    }
}
