using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using TravelBooking.Web.Configuration;
using TravelBooking.Web.ViewModels.Stripe;

namespace TravelBooking.Web.Controllers;

public class StripeController(IOptions<StripeOptions> stripeOptions) : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateCheckoutSession([FromForm] StripeCheckoutRequest request)
    {
        if (string.IsNullOrEmpty(stripeOptions.Value.SecretKey))
        {
            TempData["ErrorMessage"] = "Stripe is not configured. Please check the Stripe:SecretKey setting.";
            return RedirectToAction("Index", "Home");
        }

        if (request.AmountKurus <= 0)
        {
            TempData["ErrorMessage"] = "Invalid payment amount.";
            return RedirectToAction("Index", "Home");
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems =
            [
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = request.AmountKurus,
                        Currency = request.Currency.Trim().ToLowerInvariant(),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = string.IsNullOrEmpty(request.ProductName) ? "TravelBooking Reservation" : request.ProductName
                        }
                    },
                    Quantity = 1
                }
            ],
            Mode = "payment",
            SuccessUrl = baseUrl + "/Stripe/Success?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = baseUrl + "/Stripe/Cancel"
        };

        var service = new SessionService();
        Session session = service.Create(options);

        return Redirect(session.Url);
    }

    [HttpGet]
    public IActionResult Success(string? session_id)
    {
        if (!string.IsNullOrEmpty(session_id))
        {
            try
            {
                var service = new SessionService();
                var session = service.Get(session_id);
                ViewBag.PaymentStatus = session.PaymentStatus;
                ViewBag.AmountTotal = session.AmountTotal.HasValue ? session.AmountTotal.Value / 100m : 0;
                ViewBag.Currency = session.Currency?.ToUpperInvariant() ?? "TRY";
            }
            catch (StripeException)
            {
                ViewBag.PaymentStatus = "unknown";
                ViewBag.AmountTotal = 0;
                ViewBag.Currency = "TRY";
            }
        }
        return View();
    }

    [HttpGet]
    public IActionResult Cancel()
    {
        return View();
    }
}
