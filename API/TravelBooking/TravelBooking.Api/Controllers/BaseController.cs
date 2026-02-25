using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelBooking.Application.Common;
using AutoMapper;

namespace TravelBooking.Api.Controllers;

/// <summary>
/// Base controller class that provides common functionality for all API controllers.
/// </summary>
[ApiController]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the authenticated user's ID from the JWT token claims.
    /// </summary>
    /// <returns>The user ID if authenticated, otherwise null.</returns>
    protected string? GetAuthenticatedUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value;
    }

    /// <summary>
    /// Gets the authenticated user's ID or throws an UnauthorizedAccessException if not found.
    /// </summary>
    /// <returns>The user ID.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user ID is not found.</exception>
    protected string GetAuthenticatedUserIdOrThrow()
    {
        var userId = GetAuthenticatedUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new UnauthorizedAccessException("Kullanici kimligi dogrulanamadi. Lutfen giris yapin.");
        }
        return userId;
    }

    /// <summary>
    /// Checks if the current user has the Admin role.
    /// </summary>
    /// <returns>True if the user is an Admin, otherwise false.</returns>
    protected bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    /// <summary>
    /// Checks if the current user is authorized to access a resource owned by the specified user ID.
    /// </summary>
    /// <param name="resourceOwnerId">The ID of the resource owner.</param>
    /// <returns>True if authorized (user is Admin or owns the resource), otherwise false.</returns>
    protected bool IsAuthorizedForResource(string resourceOwnerId)
    {
        if (IsAdmin())
            return true;

        var userId = GetAuthenticatedUserId();
        return userId == resourceOwnerId;
    }

    /// <summary>
    /// Returns an Unauthorized result if the user is not authenticated.
    /// </summary>
    /// <returns>UnauthorizedResult if not authenticated, otherwise null.</returns>
    protected ActionResult? EnsureAuthenticated()
    {
        var userId = GetAuthenticatedUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new ErrorResult("Kullanici kimligi dogrulanamadi. Lutfen giris yapin."));
        }
        return null;
    }

    /// <summary>
    /// Returns a Forbidden result if the user is not authorized to access the resource.
    /// </summary>
    /// <param name="resourceOwnerId">The ID of the resource owner.</param>
    /// <returns>ForbidResult if not authorized, otherwise null.</returns>
    protected ActionResult? EnsureAuthorizedForResource(string resourceOwnerId)
    {
        if (!IsAuthorizedForResource(resourceOwnerId))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new ErrorResult("Bu kaynaga erisim yetkiniz yok."));
        }
        return null;
    }

    /// <summary>
    /// Creates a standardized error response for validation failures.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>BadRequest result with ErrorResult.</returns>
    protected ActionResult BadRequestError(string message)
    {
        return BadRequest(new ErrorResult(message));
    }

    /// <summary>
    /// Creates a standardized error response for not found scenarios.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <returns>NotFound result with ErrorResult.</returns>
    protected ActionResult NotFoundError(string message)
    {
        return NotFound(new ErrorResult(message));
    }

    /// <summary>
    /// Handles service result and returns appropriate ActionResult.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    /// <param name="result">The service result.</param>
    /// <param name="onSuccess">Action to perform on success.</param>
    /// <returns>ActionResult based on result success status.</returns>
    protected ActionResult HandleServiceResult<T>(DataResult<T> result, Func<T, ActionResult> onSuccess)
    {
        if (!result.Success)
        {
            if (result.Data == null)
                return NotFound(result);
            return BadRequest(result);
        }

        if (result.Data == null)
            return NotFoundError("Veri bulunamadi.");

        return onSuccess(result.Data);
    }

    /// <summary>
    /// Handles paginated service result and maps to DTO paginated result.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TDto">The DTO type.</typeparam>
    /// <param name="pagedResult">The paginated service result.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <returns>ActionResult with paginated DTO result.</returns>
    protected ActionResult HandlePagedResult<TEntity, TDto>(
        DataResult<PagedResult<TEntity>> pagedResult,
        IMapper mapper)
    {
        if (!pagedResult.Success)
            return BadRequest(pagedResult);

        if (pagedResult.Data == null)
            return NotFoundError("Sayfalanmis veri bulunamadi.");

        var pagedDtos = new PagedResult<TDto>(
            mapper.Map<IEnumerable<TDto>>(pagedResult.Data.Items),
            pagedResult.Data.TotalCount,
            pagedResult.Data.PageNumber,
            pagedResult.Data.PageSize
        );

        return Ok(new SuccessDataResult<PagedResult<TDto>>(pagedDtos));
    }

    /// <summary>
    /// Handles non-paginated service result and maps to DTO list.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TDto">The DTO type.</typeparam>
    /// <param name="result">The service result.</param>
    /// <param name="mapper">The AutoMapper instance.</param>
    /// <returns>ActionResult with DTO list result.</returns>
    protected ActionResult HandleListResult<TEntity, TDto>(
        DataResult<IEnumerable<TEntity>> result,
        IMapper mapper)
    {
        if (!result.Success)
            return BadRequest(result);

        if (result.Data == null)
            return NotFoundError("Veri bulunamadi.");

        var dtos = mapper.Map<IEnumerable<TDto>>(result.Data);
        return Ok(new SuccessDataResult<IEnumerable<TDto>>(dtos));
    }
}
