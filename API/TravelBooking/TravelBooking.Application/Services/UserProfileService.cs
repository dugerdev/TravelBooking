using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using AutoMapper;

namespace TravelBooking.Application.Services;

/// <summary>
/// Kullanici profil yonetimi servisi
/// Kullanicilarin profil bilgilerini yonetir ve kendi rezervasyonlarini goruntulemelerini saglar
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IReservationService _reservationService;
    private readonly IMapper _mapper;

    /// <summary>
    /// UserProfileService constructor'i
    /// Bagimliliklari enjekte eder (Dependency Injection)
    /// </summary>
    public UserProfileService(UserManager<AppUser> userManager, IReservationService reservationService, IMapper mapper)
    {
        _userManager = userManager;
        _reservationService = reservationService;
        _mapper = mapper;
    }

    public async Task<DataResult<UserProfileDto>> GetCurrentUserProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new ErrorDataResult<UserProfileDto>(null!, "Kullanici bulunamadi.");

        var profileDto = new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmed = user.PhoneNumberConfirmed,
            LockoutEnd = user.LockoutEnd?.DateTime,
            LockoutEnabled = user.LockoutEnabled,
            AccessFailedCount = user.AccessFailedCount
        };

        return new SuccessDataResult<UserProfileDto>(profileDto);
    }

    public async Task<Result> UpdateProfileAsync(string userId, UpdateProfileDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return new ErrorResult("Kullanici bulunamadi.");

        if (!string.IsNullOrWhiteSpace(dto.UserName) && dto.UserName != user.UserName)
        {
            var userNameExists = await _userManager.FindByNameAsync(dto.UserName);
            if (userNameExists != null && userNameExists.Id != userId)
                return new ErrorResult("Bu kullanici adi zaten kullaniliyor.");

            user.UserName = dto.UserName;
        }

        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            var emailExists = await _userManager.FindByEmailAsync(dto.Email);
            if (emailExists != null && emailExists.Id != userId)
                return new ErrorResult("Bu email adresi zaten kullaniliyor.");

            user.Email = dto.Email;
            user.EmailConfirmed = false; // Email degistiginde yeniden dogrulama gerekir
        }

        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
        {
            user.PhoneNumber = dto.PhoneNumber;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return new ErrorResult(string.Join(", ", result.Errors.Select(e => e.Description)));

        return new SuccessResult("Profil basariyla guncellendi.");
    }

    /// <summary>
    /// Kullanicinin rezervasyonlarini sayfalanmis sekilde getirir
    /// AutoMapper kullanarak Reservation entity'sini ReservationDto'ya donusturur
    /// </summary>
    public async Task<DataResult<PagedResult<ReservationDto>>> GetUserReservationsAsync(string userId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        // Kullanicinin rezervasyonlarini sayfalanmis sekilde getir
        var reservationsResult = await _reservationService.GetByUserIdPagedAsync(userId, request, cancellationToken);
        if (!reservationsResult.Success)
            return new ErrorDataResult<PagedResult<ReservationDto>>(null!, reservationsResult.Message);

        // AutoMapper kullanarak Reservation entity'lerini ReservationDto'ya donustur
        // Bu sayede manuel mapping yapmaya gerek kalmaz
        var pagedReservations = _mapper.Map<IEnumerable<ReservationDto>>(reservationsResult.Data.Items);

        // Sayfalanmis sonucu olustur
        var pagedResult = new PagedResult<ReservationDto>(
            pagedReservations,
            reservationsResult.Data.TotalCount,
            reservationsResult.Data.PageNumber,
            reservationsResult.Data.PageSize);

        return new SuccessDataResult<PagedResult<ReservationDto>>(pagedResult);
    }
}
