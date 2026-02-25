using TravelBooking.Application.Contracts;
using TravelBooking.Application.Common;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Swashbuckle.AspNetCore.Annotations;
using FluentValidation;

namespace TravelBooking.Api.Controllers;

[Route("api/[controller]")]
[SwaggerTag("Haber islemleri icin endpoint'ler")]
public class NewsController : BaseController
{
    private readonly INewsService _newsService;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateNewsDto> _validator;

    public NewsController(INewsService newsService, IMapper mapper, IValidator<CreateNewsDto> validator)
    {
        _newsService = newsService;
        _mapper = mapper;
        _validator = validator;
    }

    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Tum haberleri getir", Description = "Tum haberleri listeler. Pagination destegi vardir. Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<List<NewsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SuccessDataResult<PagedResult<NewsDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll([FromQuery] PagedRequest? request, CancellationToken cancellationToken = default)
    {
        if (request != null)
        {
            var pagedResult = await _newsService.GetAllPagedAsync(request, cancellationToken);
            if (!pagedResult.Success)
                return BadRequest(pagedResult);

            if (pagedResult.Data == null)
                return NotFoundError("Sayfalanmis haber verisi bulunamadi.");

            var pagedNewsDtos = new PagedResult<NewsDto>(
                _mapper.Map<List<NewsDto>>(pagedResult.Data.Items),
                pagedResult.Data.TotalCount,
                pagedResult.Data.PageNumber,
                pagedResult.Data.PageSize
            );

            return Ok(new SuccessDataResult<PagedResult<NewsDto>>(pagedNewsDtos));
        }

        var result = await _newsService.GetPublishedAsync(cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Haber verisi bulunamadi.");

        var newsDtos = _mapper.Map<List<NewsDto>>(result.Data);
        return Ok(new SuccessDataResult<List<NewsDto>>(newsDtos));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "ID'ye gore haber getir", Description = "Belirtilen ID'ye sahip haber bilgilerini getirir ve goruntulenme sayisini artirir. Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<NewsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDataResult<NewsDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataResult<NewsDto>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _newsService.GetByIdAsync(id, incrementViewCount: true, cancellationToken);
        
        if (!result.Success || result.Data == null)
            return NotFound(result);

        var newsDto = _mapper.Map<NewsDto>(result.Data);
        return Ok(new SuccessDataResult<NewsDto>(newsDto));
    }

    [HttpGet("search")]
    [AllowAnonymous]
    [SwaggerOperation(Summary = "Haber ara", Description = "Baslik/ozet ve kategoriye gore haber ara. Giris gerekmez.")]
    [ProducesResponseType(typeof(SuccessDataResult<List<NewsDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<DataResult<List<NewsDto>>>> Search(
        [FromQuery] string? query,
        [FromQuery] string? category,
        CancellationToken cancellationToken = default)
    {
        var result = await _newsService.SearchNewsAsync(query, category, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Arama sonucu bulunamadi.");

        var newsDtos = _mapper.Map<List<NewsDto>>(result.Data);
        return Ok(new SuccessDataResult<List<NewsDto>>(newsDtos));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Yeni haber olustur", Description = "Yeni bir haber kaydi olusturur. Sadece Admin.")]
    public async Task<ActionResult<Result>> Create([FromBody] CreateNewsDto dto, CancellationToken cancellationToken = default)
    {
        var news = new NewsArticle(
            dto.Title,
            dto.Summary,
            dto.Content,
            dto.Category,
            dto.PublishDate,
            dto.Author,
            dto.ImageUrl,
            dto.Tags
        );

        if (dto.IsPublished)
            news.Publish();

        var result = await _newsService.AddAsync(news, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Haber guncelle", Description = "Mevcut haberi gunceller. Sadece Admin.")]
    public async Task<ActionResult<Result>> Update(Guid id, [FromBody] CreateNewsDto dto, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        var existingResult = await _newsService.GetByIdAsync(id, incrementViewCount: false, cancellationToken);
        if (!existingResult.Success || existingResult.Data == null)
            return NotFound(existingResult);

        var news = existingResult.Data;
        news.Update(
            dto.Title,
            dto.Summary,
            dto.Content,
            dto.Category,
            dto.PublishDate,
            dto.Author,
            dto.ImageUrl,
            dto.Tags,
            dto.IsPublished
        );

        var result = await _newsService.UpdateAsync(news, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(Summary = "Haber sil", Description = "Haberi soft delete ile siler. Sadece Admin.")]
    public async Task<ActionResult<Result>> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _newsService.DeleteAsync(id, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
