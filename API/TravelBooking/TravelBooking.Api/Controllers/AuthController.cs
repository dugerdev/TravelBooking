using TravelBooking.Api.Models;
using TravelBooking.Api.Services;
using TravelBooking.Api.Services.Auth;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Dtos;
using TravelBooking.Domain.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IPasswordService _passwordService;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<AppUser> userManager,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        IPasswordService passwordService,
        IEmailVerificationService emailVerificationService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _passwordService = passwordService;
        _emailVerificationService = emailVerificationService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("signup")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Signup(SignupRequest request, CancellationToken cancellationToken)
    {
        var user = new AppUser
        {
            Email = request.Email,
            UserName = request.UserName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });

        // default role
        await _userManager.AddToRoleAsync(user, "User");

        var token = await _tokenService.CreateTokenAsync(user, cancellationToken);
        var (refreshToken, refreshExpiresAtUtc) = await _refreshTokenService.IssueAsync(user.Id, cancellationToken);
        token.RefreshToken = refreshToken;
        token.RefreshTokenExpiresAtUtc = refreshExpiresAtUtc;
        return Ok(token);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        AppUser? user = null;

        if (!string.IsNullOrWhiteSpace(request.UserNameOrEmail))
        {
            user = await _userManager.FindByNameAsync(request.UserNameOrEmail)
                   ?? await _userManager.FindByEmailAsync(request.UserNameOrEmail);
        }

        if (user is null)
            return Unauthorized(new { Message = "Invalid credentials" });

        var isValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isValid)
            return Unauthorized(new { Message = "Invalid credentials" });

        var token = await _tokenService.CreateTokenAsync(user, cancellationToken);
        var (refreshToken, refreshExpiresAtUtc) = await _refreshTokenService.IssueAsync(user.Id, cancellationToken);
        token.RefreshToken = refreshToken;
        token.RefreshTokenExpiresAtUtc = refreshExpiresAtUtc;
        return Ok(token);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var (userId, newRefreshToken, newRefreshExpiresAtUtc) = await _refreshTokenService.RotateAsync(request.RefreshToken, cancellationToken);

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return Unauthorized(new { Message = "Invalid refresh token" });

        var token = await _tokenService.CreateTokenAsync(user, cancellationToken);
        token.RefreshToken = newRefreshToken;
        token.RefreshTokenExpiresAtUtc = newRefreshExpiresAtUtc;
        return Ok(token);
    }

    [HttpPost("logout")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await _refreshTokenService.RevokeAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword([FromBody] ResetPasswordRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _passwordService.ResetPasswordRequestAsync(dto.Email, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        var result = await _passwordService.ResetPasswordAsync(dto, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("send-verification-email")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> SendVerificationEmail([FromBody] SendVerificationEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await _emailVerificationService.SendVerificationEmailAsync(request.Email, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request, CancellationToken cancellationToken)
    {
        var result = await _emailVerificationService.VerifyEmailAsync(request.Email, request.Token, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("dev/reset-admin-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetAdminPassword(CancellationToken cancellationToken)
    {
        if (!HttpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
            return NotFound();

        var adminEmail = _configuration["Seed:AdminEmail"];
        var adminUserName = _configuration["Seed:AdminUserName"];
        var adminPassword = _configuration["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminUserName) && string.IsNullOrWhiteSpace(adminEmail))
            return BadRequest(new { Message = "Seed:AdminUserName or Seed:AdminEmail must be set." });

        if (string.IsNullOrWhiteSpace(adminPassword))
            return BadRequest(new { Message = "Seed:AdminPassword is empty." });

        var admin = await _userManager.FindByNameAsync(adminUserName!)
                    ?? await _userManager.FindByEmailAsync(adminEmail!);

        if (admin is null)
            return NotFound(new { Message = "Admin user not found." });

        var hasPassword = await _userManager.HasPasswordAsync(admin);
        if (hasPassword)
        {
            var removeResult = await _userManager.RemovePasswordAsync(admin);
            if (!removeResult.Succeeded)
            {
                _logger.LogError("Admin password remove failed: {Errors}", string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                return BadRequest(new { Message = "Failed to remove admin password." });
            }
        }

        var addResult = await _userManager.AddPasswordAsync(admin, adminPassword);
        if (!addResult.Succeeded)
        {
            _logger.LogError("Admin password add failed: {Errors}", string.Join(", ", addResult.Errors.Select(e => e.Description)));
            return BadRequest(new { Message = "Failed to set admin password." });
        }

        _logger.LogInformation("Admin password reset successfully for {UserName}", admin.UserName);
        return Ok(new { Message = "Admin password reset successfully." });
    }
}
