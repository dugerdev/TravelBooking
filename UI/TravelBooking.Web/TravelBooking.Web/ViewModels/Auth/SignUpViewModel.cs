using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Auth;

public class SignUpViewModel
{
    [Required(ErrorMessage = "E-posta gereklidir.")]
    [EmailAddress(ErrorMessage = "Gecerli bir e-posta adresi girin.")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kullanici adi gereklidir.")]
    [StringLength(100, MinimumLength = 2)]
    [Display(Name = "Kullanici Adi")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre gereklidir.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    [Display(Name = "Sifre")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Sifre Tekrar")]
    [Compare("Password", ErrorMessage = "Sifreler eslesmiyor.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
