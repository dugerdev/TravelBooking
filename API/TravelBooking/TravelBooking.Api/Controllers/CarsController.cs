using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Common;
using TravelBooking.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Swashbuckle.AspNetCore.Annotations;
using FluentValidation;

namespace TravelBooking.Api.Controllers;

[Route("api/[controller]")]
[SwaggerTag("Arac kiralama islemleri icin endpoint'ler")]
public class CarsController : BaseController
{
    private readonly ICarService _carService;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateCarDto> _validator;

    public CarsController(ICarService carService, IMapper mapper, IValidator<CreateCarDto> validator)
    {
        _carService = carService;
        _mapper = mapper;
        _validator = validator;
    }

    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Tum araclari getir", Description = "Tum araclari listeler. Pagination destegi vardir. Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<CarDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SuccessDataResult<PagedResult<CarDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll([FromQuery] PagedRequest? request, CancellationToken cancellationToken = default)
    {
        if (request != null)
        {
            var pagedResult = await _carService.GetAllPagedAsync(request, cancellationToken);
            if (!pagedResult.Success)
                return BadRequest(pagedResult);

            if (pagedResult.Data == null)
                return NotFoundError("Sayfalanmis arac verisi bulunamadi.");

            var pagedCarDtos = new PagedResult<CarDto>(
                _mapper.Map<IEnumerable<CarDto>>(pagedResult.Data.Items),
                pagedResult.Data.TotalCount,
                pagedResult.Data.PageNumber,
                pagedResult.Data.PageSize
            );

            return Ok(new SuccessDataResult<PagedResult<CarDto>>(pagedCarDtos));
        }

        var result = await _carService.GetAllAsync(cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        var carDtos = _mapper.Map<IEnumerable<CarDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<CarDto>>(carDtos));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "ID'ye gore arac getir", Description = "Belirtilen ID'ye sahip arac bilgilerini getirir. Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<CarDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<CarDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataResult<CarDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _carService.GetByIdAsync(id, cancellationToken);
        
        if (!result.Success || result.Data == null)
            return NotFound(result);

        var carDto = _mapper.Map<CarDto>(result.Data);
        return Ok(new SuccessDataResult<CarDto>(carDto));
    }

    [HttpGet("search")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Arac ara", Description = "Lokasyon, kategori ve maksimum fiyata gore arac ara. Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<CarDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<DataResult<IEnumerable<CarDto>>>> Search(
        [FromQuery] string? location,
        [FromQuery] string? category,
        [FromQuery] decimal? maxPricePerDay,
        CancellationToken cancellationToken = default)
    {
        var result = await _carService.SearchCarsAsync(location, category, maxPricePerDay, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Arama sonucu bulunamadi.");

        var carDtos = _mapper.Map<IEnumerable<CarDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<CarDto>>(carDtos));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Yeni arac olustur", Description = "Yeni bir arac kaydi olusturur. Sadece Admin.")]
    public async Task<ActionResult<Result>> Create([FromBody] CreateCarDto dto, CancellationToken cancellationToken = default)
    {
        // Input validation
        if (dto.PricePerDay <= 0)
            return BadRequest(new ErrorResult("Gunluk fiyat 0'dan buyuk olmalidir."));
        
        // Currency default degeri
        if (dto.Currency == 0) // Currency enum'in default degeri
            dto.Currency = Domain.Enums.Currency.USD;
        
        // Money value object'i olustur
        var pricePerDay = new Money(dto.PricePerDay, dto.Currency);
        
        // Null check ve validation
        if (pricePerDay == null)
            return BadRequest(new ErrorResult("Fiyat bilgisi olusturulamadi."));
        
        if (pricePerDay.Amount <= 0)
            return BadRequest(new ErrorResult("Fiyat miktari 0'dan buyuk olmalidir."));
        
        var car = new Car(
            dto.Brand,
            dto.Model,
            dto.Category,
            dto.Year,
            dto.FuelType,
            dto.Transmission,
            dto.Seats,
            dto.Doors,
            pricePerDay,
            dto.ImageUrl ?? string.Empty,
            dto.Location,
            dto.HasAirConditioning,
            dto.HasGPS
        );

        var result = await _carService.AddAsync(car, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Arac guncelle", Description = "Mevcut araci gunceller. Sadece Admin.")]
    public async Task<ActionResult<Result>> Update(Guid id, [FromBody] CreateCarDto dto, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        var existingResult = await _carService.GetByIdAsync(id, cancellationToken);
        if (!existingResult.Success || existingResult.Data == null)
            return NotFound(existingResult);

        var car = existingResult.Data;
        car.Update(
            dto.Brand,
            dto.Model,
            dto.Category,
            dto.Year,
            dto.FuelType,
            dto.Transmission,
            dto.Seats,
            dto.Doors,
            new Money(dto.PricePerDay, dto.Currency),
            dto.ImageUrl,
            dto.Location,
            dto.HasAirConditioning,
            dto.HasGPS
        );

        var result = await _carService.UpdateAsync(car, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Arac sil", Description = "Araci soft delete ile siler. Sadece Admin.")]
    public async Task<ActionResult<Result>> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _carService.DeleteAsync(id, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
