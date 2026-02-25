using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Swashbuckle.AspNetCore.Annotations;

namespace TravelBooking.Api.Controllers;

//---Yolcu islemleri icin controller---//
[Route("api/[controller]")]
[Authorize]
[SwaggerTag("Yolcu islemleri icin endpoint'ler")]
public class PassengersController : BaseController
{
    private readonly IPassengerService _passengerService;                            //---Yolcu servisi---//
    private readonly IMapper _mapper;                                                 //---AutoMapper---//

    public PassengersController(IPassengerService passengerService, IMapper mapper)
    {
        _passengerService = passengerService;
        _mapper = mapper;
    }

    //---Yeni yolcu ekle---//
    [HttpPost]
    [SwaggerOperation(Summary = "Yeni yolcu ekle", Description = "Yeni bir yolcu kaydi olusturur")]
    [ProducesResponseType(typeof(SuccessDataResult<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> Create([FromBody] CreatePassengerDto dto, CancellationToken cancellationToken = default)
    {
        var passenger = new Passenger(
            dto.PassengerFirstName,
            dto.PassengerLastName,
            dto.NationalNumber,
            dto.PassportNumber,
            dto.DateOfBirth,
            dto.PassengerType);

        var result = await _passengerService.AddAsync(passenger, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(new SuccessDataResult<Guid>(passenger.Id, result.Message));
    }

    //---Tum yolculari getir---//
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Tum yolculari getir", Description = "Tum yolculari listeler. Sadece Admin. Pagination destegi vardir")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<PassengerDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SuccessDataResult<PagedResult<PassengerDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetAll([FromQuery] PagedRequest? request, CancellationToken cancellationToken = default)
    {
        // Pagination varsa paginated endpoint kullan
        if (request != null)
        {
            var pagedResult = await _passengerService.GetAllPagedAsync(request, cancellationToken);
            if (!pagedResult.Success)
                return BadRequest(pagedResult);

            if (pagedResult.Data == null)
                return NotFoundError("Sayfalanmis yolcu verisi bulunamadi.");

            var pagedPassengerDtos = new PagedResult<PassengerDto>(
                _mapper.Map<IEnumerable<PassengerDto>>(pagedResult.Data.Items),
                pagedResult.Data.TotalCount,
                pagedResult.Data.PageNumber,
                pagedResult.Data.PageSize
            );

            return Ok(new SuccessDataResult<PagedResult<PassengerDto>>(pagedPassengerDtos));
        }

        var result = await _passengerService.GetAllAsync(cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Yolcu verisi bulunamadi.");

        //---Entity'leri DTO'ya cevir (AutoMapper ile)---//
        var passengerDtos = _mapper.Map<IEnumerable<PassengerDto>>(result.Data);

        return Ok(new SuccessDataResult<IEnumerable<PassengerDto>>(passengerDtos));
    }

    //---ID'ye gore yolcu getir---//
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "ID'ye gore yolcu getir", Description = "Belirtilen ID'ye sahip yolcu bilgilerini getirir")]
    [ProducesResponseType(typeof(SuccessDataResult<PassengerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<PassengerDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataResult<PassengerDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _passengerService.GetByIdAsync(id, cancellationToken);
        
        if (!result.Success || result.Data == null)
            return NotFound(result);

        var passengerDto = _mapper.Map<PassengerDto>(result.Data);

        return Ok(new SuccessDataResult<PassengerDto>(passengerDto));
    }
}

