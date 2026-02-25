using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Application.Services;

//---Yolculara iliskin is kurallarini yoneten servis---//
public class PassengerManager : IPassengerService
{
    private readonly IUnitOfWork _unitOfWork;                                        //---Tum repository'leri yoneten yapi---//
    private readonly IValidator<Passenger> _validator;                                //---Yolcu dogrulama kurallari---//
    private readonly ILogger<PassengerManager> _logger;                               //---Logging servisi---//

    public PassengerManager(IUnitOfWork unitOfWork, IValidator<Passenger> validator, ILogger<PassengerManager> logger)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _logger = logger;
    }

    //---ID'ye gore yolcu getiren metot---//
    public async Task<DataResult<Passenger>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var passenger = await _unitOfWork.Passengers.GetByIdAsync(id, cancellationToken);

        if (passenger is null)
            return new ErrorDataResult<Passenger>(null!, "Yolcu bulunamadi.");

        return new SuccessDataResult<Passenger>(passenger);
    }

    //---Tum yolculari getiren metot---//
    public async Task<DataResult<IEnumerable<Passenger>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var passengers = await _unitOfWork.Passengers.GetAllAsync(cancellationToken);
        return new SuccessDataResult<IEnumerable<Passenger>>(passengers);
    }

    //---Tum yolculari pagination ile getiren metot---//
    public async Task<DataResult<PagedResult<Passenger>>> GetAllPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting passengers with pagination: Page {PageNumber}, Size {PageSize}", request.PageNumber, request.PageSize);
        
        var pagedResult = await _unitOfWork.Passengers.GetAllPagedAsync(request, cancellationToken);
        return new SuccessDataResult<PagedResult<Passenger>>(pagedResult);
    }

    //---Yeni yolcu ekleyen metot---//
    public async Task<Result> AddAsync(Passenger passenger, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding new passenger: {FirstName} {LastName}", passenger.PassengerFirstName, passenger.PassengerLastName);
        
        try
        {
            await _validator.ValidateAndThrowAsync(passenger);

            await _unitOfWork.Passengers.AddAsync(passenger, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Passenger added successfully: {PassengerId} - {FirstName} {LastName}", passenger.Id, passenger.PassengerFirstName, passenger.PassengerLastName);
            return new SuccessResult("Yolcu eklendi.");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? "Bilinmeyen hata";
            _logger.LogError(dbEx, "Database error while adding passenger: {FirstName} {LastName}. Inner: {InnerMessage}", 
                passenger.PassengerFirstName, passenger.PassengerLastName, innerMessage);
            
            if (dbEx.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Number == 515)
                    return new ErrorResult($"Yolcu eklenirken hata: Zorunlu alan eksik. SQL: {sqlEx.Message}");
                else if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                    return new ErrorResult($"Yolcu eklenirken hata: Bu kimlik numarasi zaten mevcut. SQL: {sqlEx.Message}");
            }
            
            return new ErrorResult($"Yolcu eklenirken veritabani hatasi: {innerMessage}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding passenger: {FirstName} {LastName}", passenger.PassengerFirstName, passenger.PassengerLastName);
            return new ErrorResult("Yolcu eklenirken bir hata olustu. Lutfen tekrar deneyin.");
        }
    }

    //---Mevcut yolcuyu guncelleyen metot---//
    public async Task<Result> UpdateAsync(Passenger passenger, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(passenger);

        await _unitOfWork.Passengers.UpdateAsync(passenger, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Yolcu guncellendi.");
    }

    //---Yolcuyu soft delete eden metot---//
    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Passengers.SoftDeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Yolcu silindi.");
    }
}

