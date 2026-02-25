using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Auth;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-posta veya kullanici adi gereklidir.")]
    [Display(Name = "E-posta veya Kullanici Adi")]
    public string UserNameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre gereklidir.")]
    [DataType(DataType.Password)]
    [Display(Name = "Sifre")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Beni hatirla")]
    public bool RememberMe { get; set; }
}
