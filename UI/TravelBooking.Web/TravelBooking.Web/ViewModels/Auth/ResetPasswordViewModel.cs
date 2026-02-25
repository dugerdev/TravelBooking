using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Auth;

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi girin")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Token gereklidir")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni sifre gereklidir")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Sifre en az 8 karakter olmalidir")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre onayi gereklidir")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Sifreler eslesmiyor")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
