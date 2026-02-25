using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Swashbuckle.AspNetCore.Annotations;
namespace TravelBooking.Api.Controllers;

//---Bilet islemleri icin controller---//
[Route("api/[controller]")]
[Authorize]
[SwaggerTag("Bilet islemleri icin endpoint'ler")]
public class TicketsController : BaseController
{
    private readonly ITicketService _ticketService;
    private readonly IReservationService _reservationService;
    private readonly IMapper _mapper;

    public TicketsController(ITicketService ticketService, IReservationService reservationService, IMapper mapper)
    {
        _ticketService = ticketService;
        _reservationService = reservationService;
        _mapper = mapper;
    }

    //---ID'ye gore bilet getir---//
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "ID'ye gore bilet getir", Description = "Belirtilen ID'ye sahip bilet bilgilerini getirir")]
    [ProducesResponseType(typeof(SuccessDataResult<TicketDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<TicketDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataResult<TicketDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _ticketService.GetByIdAsync(id, cancellationToken);
        
        if (!result.Success || result.Data == null)
            return NotFound(result);

        // Check authorization: user must own the reservation or be Admin
        var reservationResult = await _reservationService.GetByIdAsync(result.Data.ReservationId, cancellationToken);
        
        if (reservationResult.Success && reservationResult.Data != null)
        {
            var authCheck = EnsureAuthorizedForResource(reservationResult.Data.AppUserId);
            if (authCheck != null)
                return authCheck;
        }

        var ticketDto = _mapper.Map<TicketDto>(result.Data);
        return Ok(new SuccessDataResult<TicketDto>(ticketDto));
    }

    //---Rezervasyon ID'sine gore biletleri getir---//
    [HttpGet("reservation/{reservationId}")]
    [SwaggerOperation(Summary = "Rezervasyon ID'sine gore biletleri getir", Description = "Belirtilen rezervasyona ait tum biletleri getirir")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<TicketDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataResult<IEnumerable<TicketDto>>>> GetByReservationId(Guid reservationId, CancellationToken cancellationToken = default)
    {
        // Check authorization: user must own the reservation or be Admin
        var reservationResult = await _reservationService.GetByIdAsync(reservationId, cancellationToken);
        
        if (reservationResult.Success && reservationResult.Data != null)
        {
            var authCheck = EnsureAuthorizedForResource(reservationResult.Data.AppUserId);
            if (authCheck != null)
                return authCheck;
        }

        var result = await _ticketService.GetByReservationIdAsync(reservationId, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Bilet verisi bulunamadi.");

        var ticketDtos = _mapper.Map<IEnumerable<TicketDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<TicketDto>>(ticketDtos));
    }

    //---Ucus ID'sine gore biletleri getir---//
    [HttpGet("flight/{flightId}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Ucus ID'sine gore biletleri getir", Description = "Belirtilen ucusa ait tum biletleri getirir")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<TicketDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataResult<IEnumerable<TicketDto>>>> GetByFlightId(Guid flightId, CancellationToken cancellationToken = default)
    {
        var result = await _ticketService.GetByFlightIdAsync(flightId, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Bilet verisi bulunamadi.");

        var ticketDtos = _mapper.Map<IEnumerable<TicketDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<TicketDto>>(ticketDtos));
    }

    //---Yolcu ID'sine gore biletleri getir---//
    [HttpGet("passenger/{passengerId}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Yolcu ID'sine gore biletleri getir", Description = "Belirtilen yolcuya ait tum biletleri getirir")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<TicketDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataResult<IEnumerable<TicketDto>>>> GetByPassengerId(Guid passengerId, CancellationToken cancellationToken = default)
    {
        var result = await _ticketService.GetByPassengerIdAsync(passengerId, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Bilet verisi bulunamadi.");

        var ticketDtos = _mapper.Map<IEnumerable<TicketDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<TicketDto>>(ticketDtos));
    }

    //---Tum biletleri getir---//
    [HttpGet]
    [SwaggerOperation(Summary = "Tum biletleri getir", Description = "Tum biletleri listeler. Pagination destegi vardir")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<TicketDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SuccessDataResult<PagedResult<TicketDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataResult<IEnumerable<TicketDto>>>> GetAll([FromQuery] PagedRequest? request, CancellationToken cancellationToken = default)
    {
        // Only Admin can see all tickets
        if (!IsAdmin())
            return StatusCode(StatusCodes.Status403Forbidden,
                new ErrorResult("Tum biletleri goruntuleme yetkiniz yok. Sadece Admin."));

        if (request != null)
        {
            var pagedResult = await _ticketService.GetAllPagedAsync(request, cancellationToken);
            if (!pagedResult.Success)
                return BadRequest(pagedResult);

            if (pagedResult.Data == null)
                return NotFoundError("Sayfalanmis bilet verisi bulunamadi.");

            var pagedTicketDtos = new PagedResult<TicketDto>(
                _mapper.Map<IEnumerable<TicketDto>>(pagedResult.Data.Items),
                pagedResult.Data.TotalCount,
                pagedResult.Data.PageNumber,
                pagedResult.Data.PageSize
            );

            return Ok(new SuccessDataResult<PagedResult<TicketDto>>(pagedTicketDtos));
        }

        var result = await _ticketService.GetAllAsync(cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Bilet verisi bulunamadi.");

        var ticketDtos = _mapper.Map<IEnumerable<TicketDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<TicketDto>>(ticketDtos));
    }

    //---Yeni bilet ekle---//
    [HttpPost]
    public async Task<ActionResult<Result>> Create([FromBody] CreateTicketDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            return BadRequestError("Bilet bilgileri bos olamaz.");

        if (!dto.ReservationId.HasValue || dto.ReservationId.Value == Guid.Empty)
            return BadRequestError("ReservationId zorunludur (standalone bilet olusturma icin).");

        if (dto.FlightId == Guid.Empty)
            return BadRequestError("FlightId zorunludur.");

        if (dto.PassengerId == Guid.Empty)
            return BadRequestError("PassengerId zorunludur.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequestError("Email zorunludur.");

        if (dto.TicketPrice < 0 || dto.BaggageFee < 0)
            return BadRequestError("Bilet fiyati ve bagaj ucreti negatif olamaz.");

        var reservationResult = await _reservationService.GetByIdAsync(dto.ReservationId.Value, cancellationToken);
        if (!reservationResult.Success || reservationResult.Data == null)
            return BadRequestError("Rezervasyon bulunamadi.");

        var authCheck = EnsureAuthorizedForResource(reservationResult.Data.AppUserId);
        if (authCheck != null)
            return authCheck;

        var ticket = new Ticket(
            dto.FlightId,
            dto.ReservationId.Value,
            dto.PassengerId,
            dto.Email,
            dto.ContactPhoneNumber,
            dto.SeatClass,
            dto.BaggageOption,
            dto.TicketPrice,
            dto.BaggageFee
        );

        if (!string.IsNullOrWhiteSpace(dto.SeatNumber))
        {
            ticket.AssignSeat(dto.SeatNumber);
        }

        var result = await _ticketService.AddAsync(ticket, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(new SuccessDataResult<Guid>(ticket.Id, result.Message));
    }

    //---Bileti iptal et---//
    [HttpPost("{id}/cancel")]
    [SwaggerOperation(Summary = "Bileti iptal et", Description = "Belirtilen bileti iptal eder")]
    [ProducesResponseType(typeof(SuccessResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<Result>> CancelTicket(Guid id, CancellationToken cancellationToken = default)
    {
        // Once bileti al ve yetkilendirme kontrolu yap
        var ticketResult = await _ticketService.GetByIdAsync(id, cancellationToken);
        if (!ticketResult.Success || ticketResult.Data == null)
            return NotFound(new ErrorResult("Bilet bulunamadi."));

        // Bilet sahibinin rezervasyonunu kontrol et
        var reservationResult = await _reservationService.GetByIdAsync(ticketResult.Data.ReservationId, cancellationToken);
        if (!reservationResult.Success || reservationResult.Data == null)
            return NotFound(new ErrorResult("Rezervasyon bulunamadi."));

        // Yetkilendirme: Sadece rezervasyon sahibi veya admin iptal edebilir
        var authCheck = EnsureAuthorizedForResource(reservationResult.Data.AppUserId);
        if (authCheck != null)
            return authCheck;

        var result = await _ticketService.CancelTicketAsync(id, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    //---Bilete koltuk numarasi ata---//
    [HttpPost("{id}/assign-seat")]
    [SwaggerOperation(Summary = "Bilete koltuk numarasi ata", Description = "Belirtilen bilete koltuk numarasi atar")]
    [ProducesResponseType(typeof(SuccessResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> AssignSeat(Guid id, [FromBody] AssignSeatRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            return BadRequestError("Koltuk bilgileri bos olamaz.");

        if (string.IsNullOrWhiteSpace(request.SeatNumber))
            return BadRequestError("Koltuk numarasi zorunludur.");

        var result = await _ticketService.AssignSeatAsync(id, request.SeatNumber, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}

//---Koltuk atama icin request model---//
public class AssignSeatRequest
{
    public string SeatNumber { get; set; } = string.Empty;
}
