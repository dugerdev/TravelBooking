using TravelBooking.Web.Services;
using TravelBooking.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace TravelBooking.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class TestimonialsAdminController : Controller
{
    private readonly TestimonialService _testimonialService;
    private readonly ICookieHelper _cookieHelper;

    public TestimonialsAdminController(TestimonialService testimonialService, ICookieHelper cookieHelper)
    {
        _testimonialService = testimonialService;
        _cookieHelper = cookieHelper;
    }

    private string GetAuthToken()
    {
        return _cookieHelper.GetAccessToken() ?? string.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (User?.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account", new { area = "" });
        if (!User.IsInRole("Admin"))
            return RedirectToAction("Index", "Home", new { area = "" });

        var token = GetAuthToken();
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account", new { area = "" });

        var result = await _testimonialService.GetAllTestimonialsAsync(token);
        
        if (!result.Success)
        {
            ViewBag.ErrorMessage = result.Message;
            return View(new List<DTOs.Testimonials.TestimonialDto>());
        }

        return View(result.Data ?? new List<DTOs.Testimonials.TestimonialDto>());
    }

    [HttpGet]
    public async Task<IActionResult> Pending()
    {
        if (User?.Identity?.IsAuthenticated != true)
            return RedirectToAction("Login", "Account", new { area = "" });
        if (!User.IsInRole("Admin"))
            return RedirectToAction("Index", "Home", new { area = "" });

        var token = GetAuthToken();
        if (string.IsNullOrEmpty(token))
            return RedirectToAction("Login", "Account", new { area = "" });

        var result = await _testimonialService.GetPendingTestimonialsAsync(token);
        
        if (!result.Success)
        {
            ViewBag.ErrorMessage = result.Message;
            return View(new List<DTOs.Testimonials.TestimonialDto>());
        }

        return View(result.Data ?? new List<DTOs.Testimonials.TestimonialDto>());
    }

    [HttpPost]
    public async Task<IActionResult> Approve(Guid id)
    {
        var token = GetAuthToken();
        if (string.IsNullOrEmpty(token))
            return Json(new { success = false, message = "Unauthorized" });

        var result = await _testimonialService.ApproveTestimonialAsync(id, token);
        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> Reject(Guid id, string? reason = null)
    {
        var token = GetAuthToken();
        if (string.IsNullOrEmpty(token))
            return Json(new { success = false, message = "Unauthorized" });

        var result = await _testimonialService.RejectTestimonialAsync(id, reason, token);
        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        var token = GetAuthToken();
        if (string.IsNullOrEmpty(token))
            return Json(new { success = false, message = "Unauthorized" });

        var result = await _testimonialService.DeleteTestimonialAsync(id, token);
        return Json(new { success = result.Success, message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> BulkApprove([FromBody] List<Guid> ids)
    {
        var token = GetAuthToken();
        if (string.IsNullOrEmpty(token))
            return Json(new { success = false, message = "Unauthorized" });

        var result = await _testimonialService.BulkApproveAsync(ids, token);
        return Json(new { success = result.Success, message = result.Message });
    }
}
