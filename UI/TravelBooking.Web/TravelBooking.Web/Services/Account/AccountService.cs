using TravelBooking.Web.Constants;
using TravelBooking.Web.DTOs.Account;
using TravelBooking.Web.Services.TravelBookingApi;

namespace TravelBooking.Web.Services.Account;

public class AccountService : IAccountService
{
    private readonly ITravelBookingApiClient _api;

    public AccountService(ITravelBookingApiClient api)
    {
        _api = api;
    }

    public async Task<(bool Success, string Message, UserProfileDto? Profile)> GetProfileAsync(CancellationToken ct = default)
    {
        var res = await _api.GetAsync<UserProfileDto>(ApiEndpoints.AccountProfile, ct);
        if (res == null)
            return (false, "Profil alinamadi.", null);
        if (!res.Success)
            return (false, res.Message ?? "", null);
        return (true, res.Message ?? "", res.Data);
    }

    public async Task<(bool Success, string Message)> UpdateProfileAsync(UpdateProfileDto dto, CancellationToken ct = default)
    {
        var res = await _api.PutAsync<object>(ApiEndpoints.AccountProfile, dto, ct);
        if (res == null)
            return (false, "Profil guncellenemedi.");
        return res.Success ? (true, res.Message ?? "Profil guncellendi.") : (false, res.Message ?? "Guncelleme basarisiz.");
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken ct = default)
    {
        var res = await _api.PostAsync<object>(ApiEndpoints.AccountChangePassword, dto, ct);
        if (res == null)
            return (false, "Sifre degistirilemedi.");
        return res.Success ? (true, res.Message ?? "Sifre degistirildi.") : (false, res.Message ?? "Sifre degisikligi basarisiz.");
    }

    public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        var dto = new { Email = email };
        var res = await _api.PostAsync<object>(ApiEndpoints.AuthForgotPassword, dto, ct);
        if (res == null)
            return (false, "Istek gonderilemedi.");
        return res.Success ? (true, res.Message ?? "Sifre sifirlama baglantisi gonderildi.") : (false, res.Message ?? "Istek basarisiz.");
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken ct = default)
    {
        var dto = new { Email = email, Token = token, NewPassword = newPassword };
        var res = await _api.PostAsync<object>(ApiEndpoints.AuthResetPassword, dto, ct);
        if (res == null)
            return (false, "Sifre sifirlanamadi.");
        return res.Success ? (true, res.Message ?? "Sifre sifirlandi.") : (false, res.Message ?? "Sifre sifirlama basarisiz.");
    }
}
