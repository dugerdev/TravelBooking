using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelBooking.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("api/admin/[controller]")]
[ApiController]
public class TestimonialsAdminController : BaseController
{
    private readonly ITestimonialService _testimonialService;

    public TestimonialsAdminController(ITestimonialService testimonialService)
    {
        _testimonialService = testimonialService;
    }

    /// <summary>
    /// Get all testimonials
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _testimonialService.GetAllAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get approved testimonials
    /// </summary>
    [HttpGet("approved")]
    public async Task<IActionResult> GetApproved()
    {
        var result = await _testimonialService.GetApprovedAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get pending testimonials (awaiting approval)
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var result = await _testimonialService.GetPendingAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get paginated testimonials
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] PagedRequest request)
    {
        var result = await _testimonialService.GetPagedAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get testimonial by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _testimonialService.GetByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create new testimonial
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTestimonialDto dto)
    {
        var result = await _testimonialService.CreateAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result) : BadRequest(result);
    }

    /// <summary>
    /// Update testimonial
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTestimonialDto dto)
    {
        var result = await _testimonialService.UpdateAsync(id, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete testimonial
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _testimonialService.DeleteAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Approve testimonial
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var userId = GetAuthenticatedUserIdOrThrow();
        var result = await _testimonialService.ApproveAsync(id, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Reject testimonial
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] string? reason = null)
    {
        var result = await _testimonialService.RejectAsync(id, reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Bulk approve testimonials
    /// </summary>
    [HttpPost("bulk-approve")]
    public async Task<IActionResult> BulkApprove([FromBody] List<Guid> ids)
    {
        var userId = GetAuthenticatedUserIdOrThrow();
        var result = await _testimonialService.BulkApproveAsync(ids, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Bulk reject testimonials
    /// </summary>
    [HttpPost("bulk-reject")]
    public async Task<IActionResult> BulkReject([FromBody] List<Guid> ids, [FromQuery] string? reason = null)
    {
        var result = await _testimonialService.BulkRejectAsync(ids, reason);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
