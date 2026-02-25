using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Account;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Mevcut sifre gereklidir")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni sifre gereklidir")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Sifre en az 6 karakter olmalidir")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sifre onayi gereklidir")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Yeni sifre ve onay eslesmiyor")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
