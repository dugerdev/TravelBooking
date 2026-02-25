using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Swashbuckle.AspNetCore.Annotations;

namespace TravelBooking.Api.Controllers;

//---Havalimani islemleri icin controller---//
[Route("api/[controller]")]
[Authorize]
[SwaggerTag("Havalimani islemleri icin endpoint'ler")]
public class AirportsController : BaseController
{
    private readonly IAirportService _airportService;                                //---Havalimani servisi---//
    private readonly IMapper _mapper;                                                 //---AutoMapper---//

    public AirportsController(IAirportService airportService, IMapper mapper)
    {
        _airportService = airportService;
        _mapper = mapper;
    }

    //---Tum havalimanlarini getir---//
    [HttpGet]
    [SwaggerOperation(Summary = "Tum havalimanlarini getir", Description = "Tum havalimanlarini listeler. Pagination destegi vardir")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<AirportDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SuccessDataResult<PagedResult<AirportDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetAll([FromQuery] PagedRequest? request, CancellationToken cancellationToken = default)
    {
        // Pagination varsa paginated endpoint kullan (PageNumber veya PageSize query string'de varsa)
        if (request != null && (Request.Query.ContainsKey("PageNumber") || Request.Query.ContainsKey("PageSize")))
        {
            var pagedResult = await _airportService.GetAllPagedAsync(request, cancellationToken);
            if (!pagedResult.Success)
                return BadRequest(pagedResult);

            if (pagedResult.Data == null)
                return NotFoundError("Sayfalanmis havalimani verisi bulunamadi.");

            var pagedAirportDtos = new PagedResult<AirportDto>(
                _mapper.Map<IEnumerable<AirportDto>>(pagedResult.Data.Items),
                pagedResult.Data.TotalCount,
                pagedResult.Data.PageNumber,
                pagedResult.Data.PageSize
            );

            return Ok(new SuccessDataResult<PagedResult<AirportDto>>(pagedAirportDtos));
        }

        var result = await _airportService.GetAllAsync(cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        var airportDtos = _mapper.Map<IEnumerable<AirportDto>>(result.Data);

        return Ok(new SuccessDataResult<IEnumerable<AirportDto>>(airportDtos));
    }

    //---Havalimani arama (autocomplete icin); giris yapmadan kullanilabilir---//
    [HttpGet("search")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Havalimani ara", Description = "IATA, isim veya sehir ile arama. Autocomplete icin kullanilir. Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<AirportDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<DataResult<IEnumerable<AirportDto>>>> Search(
        [FromQuery] string? query,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _airportService.SearchAsync(query ?? string.Empty, limit, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        if (result.Data == null)
            return NotFoundError("Arama sonucu bulunamadi.");
        var dtos = _mapper.Map<IEnumerable<AirportDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<AirportDto>>(dtos));
    }

    //---ID'ye gore havalimani getir---//
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "ID'ye gore havalimani getir", Description = "Belirtilen ID'ye sahip havalimani bilgilerini getirir")]
    [ProducesResponseType(typeof(SuccessDataResult<AirportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AirportDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataResult<AirportDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _airportService.GetByIdAsync(id, cancellationToken);
        
        if (!result.Success || result.Data == null)
            return NotFound(result);

        var airportDto = _mapper.Map<AirportDto>(result.Data);

        return Ok(new SuccessDataResult<AirportDto>(airportDto));
    }

    //---IATA koduna gore havalimani getir---//
    [HttpGet("iata/{iataCode}")]
    [SwaggerOperation(Summary = "IATA koduna gore havalimani getir", Description = "Belirtilen IATA koduna sahip havalimani bilgilerini getirir")]
    [ProducesResponseType(typeof(SuccessDataResult<AirportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<AirportDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataResult<AirportDto>>> GetByIATACode(string iataCode, CancellationToken cancellationToken = default)
    {
        var result = await _airportService.GetByIATACodeAsync(iataCode, cancellationToken);
        
        if (!result.Success || result.Data == null)
            return NotFound(result);

        var airportDto = _mapper.Map<AirportDto>(result.Data);

        return Ok(new SuccessDataResult<AirportDto>(airportDto));
    }

    //---Yeni havalimani ekle---//
    [HttpPost]
    [SwaggerOperation(Summary = "Yeni havalimani ekle", Description = "Yeni bir havalimani kaydi olusturur")]
    [ProducesResponseType(typeof(SuccessResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> Create([FromBody] CreateAirportDto createAirportDto, CancellationToken cancellationToken = default)
    {
        var airport = new Airport(
            createAirportDto.IATA_Code,
            createAirportDto.City,
            createAirportDto.Country,
            createAirportDto.Name
        );

        var result = await _airportService.AddAsync(airport, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    //---Havalimani guncelle---//
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Havalimani guncelle", Description = "Mevcut havalimani bilgilerini gunceller. IATA kodu degistirilemez.")]
    [ProducesResponseType(typeof(SuccessResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> Update(Guid id, [FromBody] UpdateAirportDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _airportService.GetByIdAsync(id, cancellationToken);
        if (!existing.Success || existing.Data == null)
            return NotFound(new ErrorResult("Havalimani bulunamadi."));

        existing.Data.UpdateDetails(dto.City, dto.Country, dto.Name);
        var result = await _airportService.UpdateAsync(existing.Data, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    //---Havalimani sil (soft delete)---//
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Havalimani sil", Description = "Havalimanini soft delete ile siler. Ucus iliskisi varsa silinemeyebilir.")]
    [ProducesResponseType(typeof(SuccessResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _airportService.GetByIdAsync(id, cancellationToken);
        if (!existing.Success || existing.Data == null)
            return NotFound(new ErrorResult("Havalimani bulunamadi."));

        var result = await _airportService.DeleteAsync(id, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}

