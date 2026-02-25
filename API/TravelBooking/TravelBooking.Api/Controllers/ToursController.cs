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
[SwaggerTag("Tur islemleri icin endpoint'ler")]
public class ToursController : BaseController
{
    private readonly ITourService _tourService;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateTourDto> _validator;

    public ToursController(ITourService tourService, IMapper mapper, IValidator<CreateTourDto> validator)
    {
        _tourService = tourService;
        _mapper = mapper;
        _validator = validator;
    }

    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Tum turlari getir", Description = "Tum turlari listeler. Pagination destegi vardir. Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<TourDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SuccessDataResult<PagedResult<TourDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll([FromQuery] PagedRequest? request, CancellationToken cancellationToken = default)
    {
        if (request != null)
        {
            var pagedResult = await _tourService.GetAllPagedAsync(request, cancellationToken);
            if (!pagedResult.Success)
                return BadRequest(pagedResult);

            if (pagedResult.Data == null)
                return NotFoundError("Sayfalanmis tur verisi bulunamadi.");

            var pagedTourDtos = new PagedResult<TourDto>(
                _mapper.Map<IEnumerable<TourDto>>(pagedResult.Data.Items),
                pagedResult.Data.TotalCount,
                pagedResult.Data.PageNumber,
                pagedResult.Data.PageSize
            );

            return Ok(new SuccessDataResult<PagedResult<TourDto>>(pagedTourDtos));
        }

        var result = await _tourService.GetAllAsync(cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        var tourDtos = _mapper.Map<IEnumerable<TourDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<TourDto>>(tourDtos));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "ID'ye gore tur getir", Description = "Belirtilen ID'ye sahip tur bilgilerini getirir. Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<TourDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<TourDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataResult<TourDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _tourService.GetByIdAsync(id, cancellationToken);
        
        if (!result.Success || result.Data == null)
            return NotFound(result);

        var tourDto = _mapper.Map<TourDto>(result.Data);
        return Ok(new SuccessDataResult<TourDto>(tourDto));
    }

    [HttpGet("search")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Tur ara", Description = "Destinasyon ve sureye gore tur ara. Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<TourDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<DataResult<IEnumerable<TourDto>>>> Search(
        [FromQuery] string? destination,
        [FromQuery] int? minDuration,
        [FromQuery] int? maxDuration,
        CancellationToken cancellationToken = default)
    {
        var result = await _tourService.SearchToursAsync(destination, minDuration, maxDuration, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Arama sonucu bulunamadi.");

        var tourDtos = _mapper.Map<IEnumerable<TourDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<TourDto>>(tourDtos));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Yeni tur olustur", Description = "Yeni bir tur kaydi olusturur. Sadece Admin.")]
    public async Task<ActionResult<Result>> Create([FromBody] CreateTourDto dto, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        var tour = new Tour(
            dto.Name,
            dto.Destination,
            dto.Duration,
            new Money(dto.Price, dto.Currency),
            dto.ImageUrl,
            dto.Description,
            dto.Difficulty,
            dto.MaxGroupSize,
            dto.Highlights,
            dto.Included
        );

        var result = await _tourService.AddAsync(tour, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Tur guncelle", Description = "Mevcut turu gunceller. Sadece Admin.")]
    public async Task<ActionResult<Result>> Update(Guid id, [FromBody] CreateTourDto dto, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        var existingResult = await _tourService.GetByIdAsync(id, cancellationToken);
        if (!existingResult.Success || existingResult.Data == null)
            return NotFound(existingResult);

        var tour = existingResult.Data;
        tour.Update(
            dto.Name,
            dto.Destination,
            dto.Duration,
            new Money(dto.Price, dto.Currency),
            dto.ImageUrl,
            dto.Description,
            dto.Difficulty,
            dto.MaxGroupSize,
            dto.Highlights,
            dto.Included
        );

        var result = await _tourService.UpdateAsync(tour, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Tur sil", Description = "Turu soft delete ile siler. Sadece Admin.")]
    public async Task<ActionResult<Result>> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _tourService.DeleteAsync(id, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
