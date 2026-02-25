using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;

namespace TravelBooking.Application.Contracts;

public interface ITestimonialService
{
    Task<DataResult<List<TestimonialDto>>> GetAllAsync();
    Task<DataResult<List<TestimonialDto>>> GetApprovedAsync();
    Task<DataResult<List<TestimonialDto>>> GetPendingAsync();
    Task<DataResult<PagedResult<TestimonialDto>>> GetPagedAsync(PagedRequest request);
    Task<DataResult<TestimonialDto>> GetByIdAsync(Guid id);
    Task<DataResult<TestimonialDto>> CreateAsync(CreateTestimonialDto dto);
    Task<DataResult<TestimonialDto>> UpdateAsync(Guid id, UpdateTestimonialDto dto);
    Task<Result> DeleteAsync(Guid id);
    Task<Result> ApproveAsync(Guid id, string approvedBy);
    Task<Result> RejectAsync(Guid id, string? reason = null);
    Task<Result> BulkApproveAsync(List<Guid> ids, string approvedBy);
    Task<Result> BulkRejectAsync(List<Guid> ids, string? reason = null);
}
