using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Swashbuckle.AspNetCore.Annotations;

namespace TravelBooking.Api.Controllers;

//---Rezervasyon islemleri icin controller---//
[Route("api/[controller]")]
[Authorize]
[SwaggerTag("Rezervasyon islemleri icin endpoint'ler")]
public class ReservationsController : BaseController
{
    private readonly IReservationService _reservationService;                        //---Rezervasyon servisi---//
    private readonly IMapper _mapper;                                                 //---AutoMapper---//

    public ReservationsController(IReservationService reservationService, IMapper mapper)
    {
        _reservationService = reservationService;
        _mapper = mapper;
    }

    //---Yeni rezervasyon ekle (bilet ve odeme ile birlikte)---//
    [HttpPost]
    [SwaggerOperation(Summary = "Yeni rezervasyon olustur", Description = "Yeni bir rezervasyon olusturur. Bilet ve odeme bilgileri varsa bunlari da olusturur")]
    [ProducesResponseType(typeof(SuccessDataResult<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> Create([FromBody] CreateReservationDto dto, CancellationToken cancellationToken = default)
    {
        //---GUVENLIK: JWT'den kullanici ID'sini al (DTO'dan gelen degeri kullanma)---//
        var authCheck = EnsureAuthenticated();
        if (authCheck != null)
            return authCheck;

        var authenticatedUserId = GetAuthenticatedUserIdOrThrow();
        //---DTO'daki AppUserId'yi authenticated user ile override et---//
        dto.AppUserId = authenticatedUserId;

        //---Ucus rezervasyonu: en az bilet veya yolcu listesi (harici ucus) zorunlu---//
        if (dto.Type == TravelBooking.Domain.Enums.ReservationType.Flight &&
            (dto.Tickets == null || dto.Tickets.Count == 0) &&
            (dto.Participants == null || dto.Participants.Count == 0))
            return BadRequest(new ErrorDataResult<Guid>(Guid.Empty, "Ucus rezervasyonu icin en az bir bilet veya yolcu bilgisi gereklidir."));

        //---Bilet veya odeme bilgisi varsa, gelismis is akisini kullan---//
        if ((dto.Tickets != null && dto.Tickets.Count > 0) || dto.Payment != null)
        {
            var result = await _reservationService.CreateReservationWithTicketsAndPaymentAsync(dto, cancellationToken);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        //---Basit rezervasyon olusturma (geriye donuk uyumluluk icin)---//
        var pnr = string.IsNullOrWhiteSpace(dto.PNR)
            ? Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()
            : dto.PNR;

        var reservation = new Reservation(pnr, dto.AppUserId, dto.TotalPrice, dto.Currency);
        var result2 = await _reservationService.AddAsync(reservation, cancellationToken);

        if (!result2.Success)
            return BadRequest(result2);

        return Ok(new SuccessDataResult<Guid>(reservation.Id, result2.Message));
    }

    /// <summary>
    /// Tum rezervasyonlari getirir. Admin tum rezervasyonlari, normal kullanicilar sadece kendi rezervasyonlarini gorur.
    /// </summary>
    /// <param name="cancellationToken">Iptal token'i.</param>
    /// <returns>Rezervasyon listesi.</returns>
    /// <response code="200">Rezervasyonlar basariyla getirildi.</response>
    /// <response code="400">Istek hatasi.</response>
    /// <response code="401">Yetkilendirme hatasi.</response>
    [HttpGet]
    [SwaggerOperation(Summary = "Tum rezervasyonlari getir", Description = "Admin: tum rezervasyonlar. Normal kullanici: sadece kendi rezervasyonlari.")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<ReservationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DataResult<IEnumerable<ReservationDto>>>> GetAll(CancellationToken cancellationToken = default)
    {
        var authenticatedUserId = GetAuthenticatedUserIdOrThrow();
        var isAdmin = IsAdmin();
        var result = isAdmin
            ? await _reservationService.GetAllAsync(cancellationToken)
            : await _reservationService.GetByUserIdAsync(authenticatedUserId, cancellationToken);

        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Rezervasyon verisi bulunamadi.");

        var reservationDtos = _mapper.Map<IEnumerable<ReservationDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<ReservationDto>>(reservationDtos));
    }

    /// <summary>
    /// ID'ye gore rezervasyon getirir. Sadece kendi rezervasyonunuz veya Admin erisebilir.
    /// </summary>
    /// <param name="id">Rezervasyon ID'si.</param>
    /// <param name="cancellationToken">Iptal token'i.</param>
    /// <returns>Rezervasyon detaylari.</returns>
    /// <response code="200">Rezervasyon basariyla getirildi.</response>
    /// <response code="404">Rezervasyon bulunamadi.</response>
    /// <response code="403">Bu rezervasyona erisim yetkiniz yok.</response>
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "ID'ye gore rezervasyon getir", Description = "Belirtilen ID'ye sahip rezervasyon bilgilerini getirir. Sadece kendi rezervasyonunuz veya Admin.")]
    [ProducesResponseType(typeof(SuccessDataResult<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<ReservationDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DataResult<ReservationDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _reservationService.GetByIdAsync(id, cancellationToken);
        if (!result.Success || result.Data == null)
            return NotFound(result);

        var authCheck = EnsureAuthorizedForResource(result.Data.AppUserId);
        if (authCheck != null)
            return authCheck;

        var reservationDto = _mapper.Map<ReservationDto>(result.Data);
        reservationDto.Tickets ??= [];
        reservationDto.Passengers ??= [];
        reservationDto.Payments ??= [];
        return Ok(new SuccessDataResult<ReservationDto>(reservationDto));
    }

    //---PNR'ye gore rezervasyon getir---//
    [HttpGet("pnr/{pnr}")]
    [SwaggerOperation(Summary = "PNR'ye gore rezervasyon getir", Description = "Belirtilen PNR koduna sahip rezervasyon bilgilerini getirir. Sadece kendi rezervasyonunuz veya Admin.")]
    [ProducesResponseType(typeof(SuccessDataResult<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<ReservationDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataResult<ReservationDto>>> GetByPNR(string pnr, CancellationToken cancellationToken = default)
    {
        var result = await _reservationService.GetByPNRAsync(pnr, cancellationToken);
        if (!result.Success || result.Data == null)
            return NotFound(result);

        var authCheck = EnsureAuthorizedForResource(result.Data.AppUserId);
        if (authCheck != null)
            return authCheck;

        var reservationDto = _mapper.Map<ReservationDto>(result.Data);
        reservationDto.Tickets ??= [];
        reservationDto.Passengers ??= [];
        reservationDto.Payments ??= [];
        return Ok(new SuccessDataResult<ReservationDto>(reservationDto));
    }

    //---Kullanici ID'sine gore rezervasyonlari getir---//
    [HttpGet("user/{userId}")]
    [SwaggerOperation(Summary = "Kullanici ID'sine gore rezervasyonlari getir", Description = "Belirtilen kullaniciya ait tum rezervasyonlari getirir. Pagination destegi vardir. Sadece kendi rezervasyonlarinizi veya Admin rolu ile tum rezervasyonlari goruntuleyebilirsiniz")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<ReservationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SuccessDataResult<PagedResult<ReservationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> GetByUserId(string userId, [FromQuery] PagedRequest? request, CancellationToken cancellationToken = default)
    {
        //---GUVENLIK: Kullanici sadece kendi rezervasyonlarini gorebilir, Admin tumunu gorebilir---//
        var authCheck = EnsureAuthorizedForResource(userId);
        if (authCheck != null)
            return authCheck;
        // Pagination varsa paginated endpoint kullan
        if (request != null)
        {
            var pagedResult = await _reservationService.GetByUserIdPagedAsync(userId, request, cancellationToken);
            if (!pagedResult.Success)
                return BadRequest(pagedResult);

            if (pagedResult.Data == null)
                return NotFoundError("Sayfalanmis rezervasyon verisi bulunamadi.");

            var pagedReservationDtos = new PagedResult<ReservationDto>(
                _mapper.Map<IEnumerable<ReservationDto>>(pagedResult.Data.Items),
                pagedResult.Data.TotalCount,
                pagedResult.Data.PageNumber,
                pagedResult.Data.PageSize
            );

            return Ok(new SuccessDataResult<PagedResult<ReservationDto>>(pagedReservationDtos));
        }

        var result = await _reservationService.GetByUserIdAsync(userId, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Rezervasyon verisi bulunamadi.");

        var reservationDtos = _mapper.Map<IEnumerable<ReservationDto>>(result.Data);

        return Ok(new SuccessDataResult<IEnumerable<ReservationDto>>(reservationDtos));
    }

    //---Rezervasyonu iptal et---//
    [HttpPost("{id}/cancel")]
    [SwaggerOperation(Summary = "Rezervasyonu iptal et", Description = "Belirtilen rezervasyonu iptal eder. Sadece kendi rezervasyonunuz veya Admin.")]
    [ProducesResponseType(typeof(SuccessResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> CancelReservation(Guid id, CancellationToken cancellationToken = default)
    {
        var get = await _reservationService.GetByIdAsync(id, cancellationToken);
        if (!get.Success || get.Data == null)
            return NotFound(get);

        var authCheck = EnsureAuthorizedForResource(get.Data.AppUserId);
        if (authCheck != null)
            return authCheck;

        var result = await _reservationService.CancelReservationAsync(id, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

