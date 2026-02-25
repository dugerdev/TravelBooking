using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Swashbuckle.AspNetCore.Annotations;

namespace TravelBooking.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[SwaggerTag("Iletisim formu mesajlari")]
public class ContactMessagesController : BaseController
{
    private readonly IContactMessageService _contactMessageService;
    private readonly IMapper _mapper;

    public ContactMessagesController(IContactMessageService contactMessageService, IMapper mapper)
    {
        _contactMessageService = contactMessageService;
        _mapper = mapper;
    }

    /// <summary>Create a contact message (public form).</summary>
    [HttpPost]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Iletisim formu gonder", Description = "Ziyaretci iletisim formunu doldurup gonderir. Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<ContactMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataResult<ContactMessageDto>>> Create([FromBody] CreateContactMessageDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new ContactMessage(dto.Name, dto.Email, dto.Phone ?? "", dto.Subject ?? "General Inquiry", dto.Message);
        var result = await _contactMessageService.AddAsync(entity, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        var created = await _contactMessageService.GetByIdAsync(entity.Id, cancellationToken);
        var dtoResult = _mapper.Map<ContactMessageDto>(created.Data);
        return Ok(new SuccessDataResult<ContactMessageDto>(dtoResult, result.Message));
    }

    /// <summary>Get all contact messages (admin).</summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Tum mesajlari listele", Description = "Admin: filtre ve arama ile mesaj listesi.")]
    [ProducesResponseType(typeof(SuccessDataResult<IEnumerable<ContactMessageDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<DataResult<IEnumerable<ContactMessageDto>>>> GetAll(
        [FromQuery] string? statusFilter = null,
        [FromQuery] string? searchQuery = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _contactMessageService.GetAllAsync(statusFilter, searchQuery, cancellationToken);
        if (!result.Success || result.Data == null)
            return BadRequest(result);
        var dtos = _mapper.Map<IEnumerable<ContactMessageDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<ContactMessageDto>>(dtos, result.Message));
    }

    /// <summary>Get unread count (admin).</summary>
    [HttpGet("unread-count")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Okunmamis mesaj sayisi")]
    [ProducesResponseType(typeof(SuccessDataResult<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<DataResult<int>>> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var count = await _contactMessageService.GetUnreadCountAsync(cancellationToken);
        return Ok(new SuccessDataResult<int>(count));
    }

    /// <summary>Get message by id (admin).</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Mesaj detayi")]
    [ProducesResponseType(typeof(SuccessDataResult<ContactMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataResult<ContactMessageDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _contactMessageService.GetByIdAsync(id, cancellationToken);
        if (!result.Success || result.Data == null)
            return NotFound(result);
        var dto = _mapper.Map<ContactMessageDto>(result.Data);
        return Ok(new SuccessDataResult<ContactMessageDto>(dto, result.Message));
    }

    /// <summary>Mark message as read (admin).</summary>
    [HttpPost("{id:guid}/mark-read")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Okundu isaretle")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> MarkAsRead(Guid id, [FromBody] MarkAsReadRequest request, CancellationToken cancellationToken = default)
    {
        var readBy = request?.ReadBy ?? GetAuthenticatedUserId() ?? "Admin";
        var result = await _contactMessageService.MarkAsReadAsync(id, readBy, cancellationToken);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>Add response to message (admin).</summary>
    [HttpPost("{id:guid}/response")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Yanit ekle")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> AddResponse(Guid id, [FromBody] AddResponseRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Response))
            return BadRequest(new ErrorResult("Response is required."));
        var responseBy = request.ResponseBy ?? GetAuthenticatedUserId() ?? "Admin";
        var result = await _contactMessageService.AddResponseAsync(id, request.Response, responseBy, cancellationToken);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>Delete message (admin).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Mesaji sil")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result>> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _contactMessageService.DeleteAsync(id, cancellationToken);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }
}

public class MarkAsReadRequest
{
    public string? ReadBy { get; set; }
}

public class AddResponseRequest
{
    public string Response { get; set; } = string.Empty;
    public string? ResponseBy { get; set; }
}
