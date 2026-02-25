using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TravelBooking.Application.Services;

public class ContactMessageManager : IContactMessageService
{
    private readonly IUnitOfWork _unitOfWork;

    public ContactMessageManager(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DataResult<ContactMessage>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.ContactMessages.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return new ErrorDataResult<ContactMessage>(null!, "Message not found.");
        return new SuccessDataResult<ContactMessage>(entity);
    }

    public async Task<DataResult<IEnumerable<ContactMessage>>> GetAllAsync(string? statusFilter = null, string? searchQuery = null, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Context.Set<ContactMessage>().Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            if (statusFilter.Equals("unread", StringComparison.OrdinalIgnoreCase))
                query = query.Where(c => !c.IsRead);
            else if (statusFilter.Equals("read", StringComparison.OrdinalIgnoreCase))
                query = query.Where(c => c.IsRead);
        }

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var q = searchQuery.Trim();
            query = query.Where(c =>
                c.Name.Contains(q) ||
                c.Email.Contains(q) ||
                (c.Subject != null && c.Subject.Contains(q)) ||
                (c.Message != null && c.Message.Contains(q)));
        }

        var list = await query.OrderByDescending(c => c.CreatedDate).ToListAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<ContactMessage>>(list);
    }

    public async Task<Result> AddAsync(ContactMessage contactMessage, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ContactMessages.AddAsync(contactMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new SuccessResult("Contact message submitted successfully.");
    }

    public async Task<Result> MarkAsReadAsync(Guid id, string readBy, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.ContactMessages.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return new ErrorResult("Message not found.");
        entity.MarkAsRead(readBy);
        await _unitOfWork.ContactMessages.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new SuccessResult("Message marked as read.");
    }

    public async Task<Result> AddResponseAsync(Guid id, string response, string responseBy, CancellationToken cancellationToken = default)
    {
        var entity = await _unitOfWork.ContactMessages.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return new ErrorResult("Message not found.");
        entity.AddResponse(response, responseBy);
        await _unitOfWork.ContactMessages.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new SuccessResult("Response added successfully.");
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.ContactMessages.SoftDeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new SuccessResult("Message deleted successfully.");
    }

    public async Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        var list = await _unitOfWork.ContactMessages.FindAsync(c => !c.IsRead, cancellationToken);
        return list.Count();
    }
}
