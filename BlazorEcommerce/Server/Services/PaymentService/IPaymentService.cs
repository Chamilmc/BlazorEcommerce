using Stripe.Checkout;

namespace BlazorEcommerce.Server.Services.PaymentService
{
    public interface IPaymentService
    {
        Task<Session> CreateCheckourSession();
        Task<ServiceResponse<bool>> FulfillOrder(HttpRequest request);
    }
}
