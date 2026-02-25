using TravelBooking.Web.Constants;
using TravelBooking.Web.DTOs.Common;
using TravelBooking.Web.DTOs.Testimonials;
using System.Text;
using System.Text.Json;

namespace TravelBooking.Web.Services;

public class TestimonialService(HttpClient httpClient, ILogger<TestimonialService> logger)
{
    private readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<ApiResult<List<TestimonialDto>>> GetApprovedTestimonialsAsync()
    {
        try
        {
            var response = await httpClient.GetAsync("api/testimonials/approved");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResult<List<TestimonialDto>>>(content, jsonOptions);
                return result ?? new ApiResult<List<TestimonialDto>> { Success = false, Message = "Failed to deserialize response." };
            }

            return new ApiResult<List<TestimonialDto>> { Success = false, Message = $"API Error: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching approved testimonials");
            return new ApiResult<List<TestimonialDto>> { Success = false, Message = "An error occurred while loading testimonials." };
        }
    }

    // Admin methods
    public async Task<ApiResult<List<TestimonialDto>>> GetAllTestimonialsAsync(string token)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var response = await httpClient.GetAsync(ApiEndpoints.AdminTestimonials);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResult<List<TestimonialDto>>>(content, jsonOptions);
                return result ?? new ApiResult<List<TestimonialDto>> { Success = false, Message = "Failed to deserialize response." };
            }

            return new ApiResult<List<TestimonialDto>> { Success = false, Message = $"API Error: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching all testimonials");
            return new ApiResult<List<TestimonialDto>> { Success = false, Message = "An error occurred while loading testimonials." };
        }
    }

    public async Task<ApiResult<List<TestimonialDto>>> GetPendingTestimonialsAsync(string token)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var response = await httpClient.GetAsync("api/admin/testimonialsadmin/pending");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ApiResult<List<TestimonialDto>>>(content, jsonOptions);
                return result ?? new ApiResult<List<TestimonialDto>> { Success = false, Message = "Failed to deserialize response." };
            }

            return new ApiResult<List<TestimonialDto>> { Success = false, Message = $"API Error: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching pending testimonials");
            return new ApiResult<List<TestimonialDto>> { Success = false, Message = "Bekleyen referanslar yuklenirken bir hata olustu." };
        }
    }

    public async Task<ApiResult<bool>> ApproveTestimonialAsync(Guid id, string token)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var response = await httpClient.PostAsync(ApiEndpoints.AdminTestimonialApprove(id), null);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return new ApiResult<bool> { Success = true, Data = true, Message = "Testimonial approved successfully." };
            }

            return new ApiResult<bool> { Success = false, Message = $"API Error: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error approving testimonial");
            return new ApiResult<bool> { Success = false, Message = "An error occurred while approving testimonial." };
        }
    }

    public async Task<ApiResult<bool>> RejectTestimonialAsync(Guid id, string? reason, string token)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(reason ?? ""),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync($"api/admin/testimonialsadmin/{id}/reject", jsonContent);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return new ApiResult<bool> { Success = true, Data = true, Message = "Testimonial rejected successfully." };
            }

            return new ApiResult<bool> { Success = false, Message = $"API Error: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error rejecting testimonial");
            return new ApiResult<bool> { Success = false, Message = "Referans reddedilirken bir hata olustu." };
        }
    }

    public async Task<ApiResult<bool>> DeleteTestimonialAsync(Guid id, string token)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var response = await httpClient.DeleteAsync(ApiEndpoints.AdminTestimonialDelete(id));
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return new ApiResult<bool> { Success = true, Data = true, Message = "Testimonial deleted successfully." };
            }

            return new ApiResult<bool> { Success = false, Message = $"API Error: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting testimonial");
            return new ApiResult<bool> { Success = false, Message = "An error occurred while deleting testimonial." };
        }
    }

    public async Task<ApiResult<bool>> BulkApproveAsync(List<Guid> ids, string token)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(ids),
                Encoding.UTF8,
                "application/json");

            var response = await httpClient.PostAsync(ApiEndpoints.AdminTestimonialsBulkApprove, jsonContent);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return new ApiResult<bool> { Success = true, Data = true, Message = "Testimonials approved successfully." };
            }

            return new ApiResult<bool> { Success = false, Message = $"API Error: {response.StatusCode}" };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk approving testimonials");
            return new ApiResult<bool> { Success = false, Message = "An error occurred while bulk approving testimonials." };
        }
    }
}
