using AutoMapper;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TravelBooking.Application.Services;

public class TestimonialManager : ITestimonialService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TestimonialManager(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    private IRepository<Testimonial> _repository => _unitOfWork.Testimonials;

    public async Task<DataResult<List<TestimonialDto>>> GetAllAsync()
    {
        try
        {
            var testimonials = await _repository.GetAllAsync(default);
            var list = testimonials as List<Testimonial> ?? testimonials.ToList();
            var dtos = _mapper.Map<List<TestimonialDto>>(list);
            return new SuccessDataResult<List<TestimonialDto>>(dtos, "Testimonials retrieved successfully.");
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<List<TestimonialDto>>(new List<TestimonialDto>(), $"Error retrieving testimonials: {ex.Message}");
        }
    }

    public async Task<DataResult<List<TestimonialDto>>> GetApprovedAsync()
    {
        try
        {
            var testimonials = await _repository.FindAsync(t => t.IsApproved, default);
            var list = testimonials.OrderByDescending(t => t.CreatedDate).ToList();
            var dtos = _mapper.Map<List<TestimonialDto>>(list);
            return new SuccessDataResult<List<TestimonialDto>>(dtos, "Approved testimonials retrieved successfully.");
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<List<TestimonialDto>>(new List<TestimonialDto>(), $"Error retrieving approved testimonials: {ex.Message}");
        }
    }

    public async Task<DataResult<List<TestimonialDto>>> GetPendingAsync()
    {
        try
        {
            var testimonials = await _repository.FindAsync(t => !t.IsApproved, default);
            var list = testimonials.OrderBy(t => t.CreatedDate).ToList();
            var dtos = _mapper.Map<List<TestimonialDto>>(list);
            return new SuccessDataResult<List<TestimonialDto>>(dtos, "Pending testimonials retrieved successfully.");
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<List<TestimonialDto>>(new List<TestimonialDto>(), $"Error retrieving pending testimonials: {ex.Message}");
        }
    }

    public async Task<DataResult<PagedResult<TestimonialDto>>> GetPagedAsync(PagedRequest request)
    {
        try
        {
            var query = _unitOfWork.Context.Set<Testimonial>().Where(t => !t.IsDeleted);

            var totalCount = await query.CountAsync();

            var testimonials = await query
                .OrderByDescending(t => t.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<TestimonialDto>>(testimonials);

            var result = new PagedResult<TestimonialDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return new SuccessDataResult<PagedResult<TestimonialDto>>(result, "Testimonials retrieved successfully.");
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<PagedResult<TestimonialDto>>(new PagedResult<TestimonialDto>(), $"Error retrieving testimonials: {ex.Message}");
        }
    }

    public async Task<DataResult<TestimonialDto>> GetByIdAsync(Guid id)
    {
        try
        {
            var testimonial = await _repository.GetByIdAsync(id, default);
            if (testimonial == null)
                return new ErrorDataResult<TestimonialDto>(new TestimonialDto(), "Testimonial not found.");

            var dto = _mapper.Map<TestimonialDto>(testimonial);
            return new SuccessDataResult<TestimonialDto>(dto, "Testimonial retrieved successfully.");
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<TestimonialDto>(new TestimonialDto(), $"Error retrieving testimonial: {ex.Message}");
        }
    }

    public async Task<DataResult<TestimonialDto>> CreateAsync(CreateTestimonialDto dto)
    {
        try
        {
            var testimonial = new Testimonial(
                dto.CustomerName,
                dto.Location,
                dto.Comment,
                dto.Rating,
                dto.AvatarUrl ?? ""
            );

            await _repository.AddAsync(testimonial, default);
            await _unitOfWork.SaveChangesAsync(default);

            var resultDto = _mapper.Map<TestimonialDto>(testimonial);
            return new SuccessDataResult<TestimonialDto>(resultDto, "Testimonial created successfully.");
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<TestimonialDto>(new TestimonialDto(), $"Error creating testimonial: {ex.Message}");
        }
    }

    public async Task<DataResult<TestimonialDto>> UpdateAsync(Guid id, UpdateTestimonialDto dto)
    {
        try
        {
            var testimonial = await _repository.GetByIdAsync(id);
            if (testimonial == null)
                return new ErrorDataResult<TestimonialDto>(new TestimonialDto(), "Testimonial not found.");

            testimonial.Update(
                dto.CustomerName,
                dto.Location,
                dto.Comment,
                dto.Rating,
                dto.AvatarUrl
            );

            await _repository.UpdateAsync(testimonial, default);
            await _unitOfWork.SaveChangesAsync();

            var resultDto = _mapper.Map<TestimonialDto>(testimonial);
            return new SuccessDataResult<TestimonialDto>(resultDto, "Testimonial updated successfully.");
        }
        catch (Exception ex)
        {
            return new ErrorDataResult<TestimonialDto>(new TestimonialDto(), $"Error updating testimonial: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        try
        {
            var testimonial = await _repository.GetByIdAsync(id, default);
            if (testimonial == null)
                return new ErrorResult("Testimonial not found.");

            await _repository.SoftDeleteAsync(id, default);
            await _unitOfWork.SaveChangesAsync(default);

            return new SuccessResult("Testimonial deleted successfully.");
        }
        catch (Exception ex)
        {
            return new ErrorResult($"Error deleting testimonial: {ex.Message}");
        }
    }

    public async Task<Result> ApproveAsync(Guid id, string approvedBy)
    {
        try
        {
            var testimonial = await _repository.GetByIdAsync(id);
            if (testimonial == null)
                return new ErrorResult("Testimonial not found.");

            testimonial.Approve(approvedBy);
            await _repository.UpdateAsync(testimonial, default);
            await _unitOfWork.SaveChangesAsync();

            return new SuccessResult("Testimonial approved successfully.");
        }
        catch (Exception ex)
        {
            return new ErrorResult($"Error approving testimonial: {ex.Message}");
        }
    }

    public async Task<Result> RejectAsync(Guid id, string? reason = null)
    {
        try
        {
            var testimonial = await _repository.GetByIdAsync(id, default);
            if (testimonial == null)
                return new ErrorResult("Testimonial not found.");

            testimonial.Reject(reason);
            await _repository.UpdateAsync(testimonial, default);
            await _unitOfWork.SaveChangesAsync(default);

            return new SuccessResult("Testimonial rejected successfully.");
        }
        catch (Exception ex)
        {
            return new ErrorResult($"Error rejecting testimonial: {ex.Message}");
        }
    }

    public async Task<Result> BulkApproveAsync(List<Guid> ids, string approvedBy)
    {
        try
        {
            var testimonials = await _repository.FindAsync(t => ids.Contains(t.Id), default);
            var list = testimonials.ToList();

            foreach (var testimonial in list)
            {
                testimonial.Approve(approvedBy);
                await _repository.UpdateAsync(testimonial, default);
            }

            await _unitOfWork.SaveChangesAsync(default);

            return new SuccessResult($"{testimonials.Count()} testimonials approved successfully.");
        }
        catch (Exception ex)
        {
            return new ErrorResult($"Error bulk approving testimonials: {ex.Message}");
        }
    }

    public async Task<Result> BulkRejectAsync(List<Guid> ids, string? reason = null)
    {
        try
        {
            var testimonials = await _repository.FindAsync(t => ids.Contains(t.Id), default);
            var list = testimonials.ToList();

            foreach (var testimonial in list)
            {
                testimonial.Reject(reason);
                await _repository.UpdateAsync(testimonial, default);
            }

            await _unitOfWork.SaveChangesAsync(default);

            return new SuccessResult($"{list.Count} testimonials rejected successfully.");
        }
        catch (Exception ex)
        {
            return new ErrorResult($"Error bulk rejecting testimonials: {ex.Message}");
        }
    }
}
