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
[SwaggerTag("Otel islemleri icin endpoint'ler")]
public class HotelsController : BaseController
{
    private readonly IHotelService _hotelService;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateHotelDto> _validator;

    public HotelsController(IHotelService hotelService, IMapper mapper, IValidator<CreateHotelDto> validator)
    {
        _hotelService = hotelService;
        _mapper = mapper;
        _validator = validator;
    }

    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Tum otelleri getir", Description = "Tum otelleri listeler. Pagination destegi vardir. Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<HotelDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SuccessDataResult<PagedResult<HotelDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll([FromQuery] PagedRequest? request, CancellationToken cancellationToken = default)
    {
        if (request != null)
        {
            var pagedResult = await _hotelService.GetAllPagedAsync(request, cancellationToken);
            if (!pagedResult.Success)
                return BadRequest(pagedResult);

            if (pagedResult.Data == null)
                return NotFoundError("Sayfalanmis otel verisi bulunamadi.");

            var pagedHotelDtos = new PagedResult<HotelDto>(
                _mapper.Map<IEnumerable<HotelDto>>(pagedResult.Data.Items),
                pagedResult.Data.TotalCount,
                pagedResult.Data.PageNumber,
                pagedResult.Data.PageSize
            );

            return Ok(new SuccessDataResult<PagedResult<HotelDto>>(pagedHotelDtos));
        }

        var result = await _hotelService.GetAllAsync(cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        var hotelDtos = _mapper.Map<IEnumerable<HotelDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<HotelDto>>(hotelDtos));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "ID'ye gore otel getir", Description = "Belirtilen ID'ye sahip otel bilgilerini getirir (odalar dahil). Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<HotelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<HotelDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataResult<HotelDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _hotelService.GetByIdAsync(id, cancellationToken);
        
        if (!result.Success || result.Data == null)
            return NotFound(result);

        var hotelDto = _mapper.Map<HotelDto>(result.Data);
        return Ok(new SuccessDataResult<HotelDto>(hotelDto));
    }

    [HttpGet("search")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Otel ara", Description = "Sehir, minimum yildiz ve maksimum fiyata gore otel ara. Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<HotelDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<DataResult<IEnumerable<HotelDto>>>> Search(
        [FromQuery] string? city,
        [FromQuery] int? minStarRating,
        [FromQuery] decimal? maxPricePerNight,
        CancellationToken cancellationToken = default)
    {
        var result = await _hotelService.SearchHotelsAsync(city, minStarRating, maxPricePerNight, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Arama sonucu bulunamadi.");

        var hotelDtos = _mapper.Map<IEnumerable<HotelDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<HotelDto>>(hotelDtos));
    }

    [HttpPost("search/advanced")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Gelismis otel arama", Description = "19 farkli filtre ile detayli otel arama. Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<PagedResult<HotelDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<DataResult<PagedResult<HotelDto>>>> AdvancedSearch(
        [FromBody] HotelSearchFilterDto filters,
        CancellationToken cancellationToken = default)
    {
        var result = await _hotelService.SearchHotelsWithFiltersAsync(filters, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Arama sonucu bulunamadi.");

        var pagedHotelDtos = new PagedResult<HotelDto>(
            _mapper.Map<IEnumerable<HotelDto>>(result.Data.Items),
            result.Data.TotalCount,
            result.Data.PageNumber,
            result.Data.PageSize
        );

        return Ok(new SuccessDataResult<PagedResult<HotelDto>>(pagedHotelDtos, result.Message));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Yeni otel olustur", Description = "Yeni bir otel kaydi olusturur. Sadece Admin.")]
    [ProducesResponseType(typeof(SuccessResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> Create([FromBody] CreateHotelDto dto, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        var hotel = new Hotel(
            dto.Name,
            dto.City,
            dto.Country,
            dto.Address,
            dto.StarRating,
            new Money(dto.PricePerNight, dto.Currency),
            dto.ImageUrl,
            dto.Description,
            dto.HasFreeWifi,
            dto.HasParking,
            dto.HasPool,
            dto.HasRestaurant
        );

        var result = await _hotelService.AddAsync(hotel, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Otel guncelle", Description = "Mevcut oteli gunceller. Sadece Admin.")]
    public async Task<ActionResult<Result>> Update(Guid id, [FromBody] CreateHotelDto dto, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        var existingResult = await _hotelService.GetByIdAsync(id, cancellationToken);
        if (!existingResult.Success || existingResult.Data == null)
            return NotFound(existingResult);

        var hotel = existingResult.Data;
        hotel.Update(
            dto.Name,
            dto.City,
            dto.Country,
            dto.Address,
            dto.StarRating,
            new Money(dto.PricePerNight, dto.Currency),
            dto.ImageUrl,
            dto.Description,
            dto.HasFreeWifi,
            dto.HasParking,
            dto.HasPool,
            dto.HasRestaurant
        );

        var result = await _hotelService.UpdateAsync(hotel, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Otel sil", Description = "Oteli soft delete ile siler. Sadece Admin.")]
    public async Task<ActionResult<Result>> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _hotelService.DeleteAsync(id, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("clear-cache")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Cache temizle", Description = "Otel cache'ini temizler. Development icin.")]
    public ActionResult ClearCache()
    {
        _hotelService.ClearCache();
        return Ok(new SuccessResult("Cache temizlendi."));
    }
}
