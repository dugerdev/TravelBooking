using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Swashbuckle.AspNetCore.Annotations;

namespace TravelBooking.Api.Controllers;

[Route("api/account")]
[Authorize]
[SwaggerTag("Kullanici hesap islemleri icin endpoint'ler")]
public sealed class AccountController : BaseController
{
    private readonly IUserProfileService _userProfileService;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<AccountController> _logger;
    private readonly IMapper _mapper;

    public AccountController(
        IUserProfileService userProfileService,
        IPasswordService passwordService,
        ILogger<AccountController> logger,
        IMapper mapper)
    {
        _userProfileService = userProfileService;
        _passwordService = passwordService;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet("profile")]
    [SwaggerOperation(Summary = "Kullanici profilini getir", Description = "Giris yapmis kullanicinin profil bilgilerini getirir")]
    [ProducesResponseType(typeof(SuccessDataResult<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataResult<UserProfileDto>>> GetProfile(CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserIdOrThrow();
        var result = await _userProfileService.GetCurrentUserProfileAsync(userId, cancellationToken);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("profile")]
    [SwaggerOperation(Summary = "Kullanici profilini guncelle", Description = "Giris yapmis kullanicinin profil bilgilerini gunceller")]
    [ProducesResponseType(typeof(SuccessResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> UpdateProfile([FromBody] UpdateProfileDto dto, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserIdOrThrow();
        var result = await _userProfileService.UpdateProfileAsync(userId, dto, cancellationToken);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("reservations")]
    [SwaggerOperation(Summary = "Kullanicinin rezervasyonlarini getir", Description = "Giris yapmis kullanicinin tum rezervasyonlarini pagination ile getirir")]
    [ProducesResponseType(typeof(SuccessDataResult<PagedResult<ReservationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataResult<PagedResult<ReservationDto>>>> GetMyReservations(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserIdOrThrow();
        var result = await _userProfileService.GetUserReservationsAsync(userId, request, cancellationToken);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpGet("reservations/{id}")]
    [SwaggerOperation(Summary = "Rezervasyon detaylarini getir", Description = "Giris yapmis kullanicinin belirtilen rezervasyon detaylarini getirir")]
    [ProducesResponseType(typeof(SuccessDataResult<ReservationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<ReservationDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DataResult<ReservationDto>>> GetReservationDetails(
        Guid id,
        [FromServices] IReservationService reservationService,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserIdOrThrow();
        var result = await reservationService.GetByIdAsync(id, cancellationToken);

        if (!result.Success || result.Data == null)
            return NotFound(result);

        // Kullanicinin kendi rezervasyonunu gormesini kontrol et
        var authCheck = EnsureAuthorizedForResource(result.Data.AppUserId);
        if (authCheck != null)
            return authCheck;

        var reservationDto = _mapper.Map<ReservationDto>(result.Data);
        reservationDto.Tickets ??= [];
        reservationDto.Passengers ??= [];
        reservationDto.Payments ??= [];

        return Ok(new SuccessDataResult<ReservationDto>(reservationDto));
    }

    [HttpPost("change-password")]
    [SwaggerOperation(Summary = "Sifre degistir", Description = "Giris yapmis kullanicinin sifresini degistirir")]
    [ProducesResponseType(typeof(SuccessResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserIdOrThrow();
        var result = await _passwordService.ChangePasswordAsync(userId, dto, cancellationToken);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
