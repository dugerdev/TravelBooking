using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Common;
using TravelBooking.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Swashbuckle.AspNetCore.Annotations;

namespace TravelBooking.Api.Controllers;

//---Odeme islemleri icin controller---//
[Route("api/[controller]")]
[Authorize]
[SwaggerTag("Odeme islemleri icin endpoint'ler")]
public class PaymentsController : BaseController
{
    private readonly IPaymentService _paymentService;
    private readonly IReservationService _reservationService;
    private readonly IMapper _mapper;

    public PaymentsController(IPaymentService paymentService, IReservationService reservationService, IMapper mapper)
    {
        _paymentService = paymentService;
        _reservationService = reservationService;
        _mapper = mapper;
    }

    //---ID'ye gore odeme getir---//
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "ID'ye gore odeme getir", Description = "Belirtilen ID'ye sahip odeme bilgilerini getirir")]
    [ProducesResponseType(typeof(SuccessDataResult<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<PaymentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataResult<PaymentDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _paymentService.GetByIdAsync(id, cancellationToken);
        
        if (!result.Success || result.Data == null)
            return NotFound(result);

        // Check authorization: user must own the reservation or be Admin
        if (result.Data.Reservation != null)
        {
            var authCheck = EnsureAuthorizedForResource(result.Data.Reservation.AppUserId);
            if (authCheck != null)
                return authCheck;
        }
        else if (!IsAdmin())
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new ErrorResult("Odeme bilgisi eksik. Sadece Admin erisebilir."));
        }

        var paymentDto = _mapper.Map<PaymentDto>(result.Data);
        return Ok(new SuccessDataResult<PaymentDto>(paymentDto));
    }

    //---Rezervasyon ID'sine gore odemeleri getir---//
    [HttpGet("reservation/{reservationId}")]
    [SwaggerOperation(Summary = "Rezervasyon ID'sine gore odemeleri getir", Description = "Belirtilen rezervasyona ait tum odemeleri getirir")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<PaymentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataResult<IEnumerable<PaymentDto>>>> GetByReservationId(Guid reservationId, CancellationToken cancellationToken = default)
    {
        // Check authorization: user must own the reservation or be Admin
        var reservationResult = await _reservationService.GetByIdAsync(reservationId, cancellationToken);
        
        if (reservationResult.Success && reservationResult.Data != null)
        {
            var authCheck = EnsureAuthorizedForResource(reservationResult.Data.AppUserId);
            if (authCheck != null)
                return authCheck;
        }

        var result = await _paymentService.GetByReservationIdAsync(reservationId, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Odeme verisi bulunamadi.");

        var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<PaymentDto>>(paymentDtos));
    }

    //---Tum odemeleri getir---//
    [HttpGet]
    [SwaggerOperation(Summary = "Tum odemeleri getir", Description = "Tum odemeleri listeler. Pagination destegi vardir")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<PaymentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SuccessDataResult<PagedResult<PaymentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataResult<IEnumerable<PaymentDto>>>> GetAll([FromQuery] PagedRequest? request, CancellationToken cancellationToken = default)
    {
        // Only Admin can see all payments
        if (!IsAdmin())
            return StatusCode(StatusCodes.Status403Forbidden,
                new ErrorResult("Tum odemeleri goruntuleme yetkiniz yok. Sadece Admin."));

        if (request != null)
        {
            var pagedResult = await _paymentService.GetAllPagedAsync(request, cancellationToken);
            if (!pagedResult.Success)
                return BadRequest(pagedResult);

            if (pagedResult.Data == null)
                return NotFoundError("Sayfalanmis odeme verisi bulunamadi.");

            var pagedPaymentDtos = new PagedResult<PaymentDto>(
                _mapper.Map<IEnumerable<PaymentDto>>(pagedResult.Data.Items),
                pagedResult.Data.TotalCount,
                pagedResult.Data.PageNumber,
                pagedResult.Data.PageSize
            );

            return Ok(new SuccessDataResult<PagedResult<PaymentDto>>(pagedPaymentDtos));
        }

        var result = await _paymentService.GetAllAsync(cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Odeme verisi bulunamadi.");

        var paymentDtos = _mapper.Map<IEnumerable<PaymentDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<PaymentDto>>(paymentDtos));
    }

    //---Yeni odeme ekle---//
    [HttpPost]
    [SwaggerOperation(Summary = "Yeni odeme ekle", Description = "Yeni bir odeme kaydi olusturur")]
    [ProducesResponseType(typeof(SuccessDataResult<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> Create([FromBody] CreatePaymentDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return BadRequestError("Odeme bilgileri bos olamaz.");

        if (!dto.ReservationId.HasValue || dto.ReservationId.Value == Guid.Empty)
            return BadRequestError("ReservationId zorunludur (standalone odeme olusturma icin).");

        if (dto.TransactionAmount <= 0)
            return BadRequestError("Odeme tutari sifirdan buyuk olmalidir.");

        if (string.IsNullOrWhiteSpace(dto.TransactionId))
            return BadRequestError("TransactionId zorunludur.");

        var reservationResult = await _reservationService.GetByIdAsync(dto.ReservationId.Value, cancellationToken);
        if (!reservationResult.Success || reservationResult.Data == null)
            return BadRequestError("Rezervasyon bulunamadi.");

        var authCheck = EnsureAuthorizedForResource(reservationResult.Data.AppUserId);
        if (authCheck != null)
            return authCheck;

        if (dto.TransactionAmount > reservationResult.Data.TotalPrice)
            return BadRequestError("Odeme tutari rezervasyon toplam tutarini asamaz.");

        var money = new Money(dto.TransactionAmount, dto.Currency);
        var payment = new Payment(
            dto.ReservationId.Value,
            money,
            dto.PaymentMethod,
            dto.TransactionId,
            dto.TransactionType
        );

        var result = await _paymentService.AddAsync(payment, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(new SuccessDataResult<Guid>(payment.Id, result.Message));
    }

    //---Odeme islemi yap---//
    [HttpPost("process")]
    [SwaggerOperation(Summary = "Odeme islemi yap", Description = "Rezervasyon icin odeme islemini gerceklestirir")]
    [ProducesResponseType(typeof(SuccessResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> ProcessPayment(
        [FromBody] ProcessPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            return BadRequestError("Odeme bilgileri bos olamaz.");

        if (request.ReservationId == Guid.Empty)
            return BadRequestError("ReservationId zorunludur.");

        if (request.Amount <= 0)
            return BadRequestError("Odeme tutari sifirdan buyuk olmalidir.");

        if (string.IsNullOrWhiteSpace(request.TransactionId))
            return BadRequestError("TransactionId zorunludur.");

        var reservationResult = await _reservationService.GetByIdAsync(request.ReservationId, cancellationToken);
        if (!reservationResult.Success || reservationResult.Data == null)
            return BadRequestError("Rezervasyon bulunamadi.");

        var authCheck = EnsureAuthorizedForResource(reservationResult.Data.AppUserId);
        if (authCheck != null)
            return authCheck;

        if (request.Amount > reservationResult.Data.TotalPrice)
            return BadRequestError("Odeme tutari rezervasyon toplam tutarini asamaz.");

        var result = await _paymentService.ProcessPaymentAsync(
            request.ReservationId,
            request.Amount,
            request.PaymentMethod,
            request.TransactionId,
            cancellationToken);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

//---Odeme islemi icin request model---//
public class ProcessPaymentRequest
{
    public Guid ReservationId { get; set; }
    public decimal Amount { get; set; }
    public Domain.Enums.PaymentMethod PaymentMethod { get; set; }
    public string TransactionId { get; set; } = string.Empty;
}
