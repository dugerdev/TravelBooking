using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Account;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Kullanici adi gereklidir")]
    [StringLength(50, ErrorMessage = "Kullanici adi en fazla 50 karakter olabilir")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email gereklidir")]
    [EmailAddress(ErrorMessage = "Gecerli bir email adresi girin")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Gecerli bir telefon numarasi girin")]
    public string? PhoneNumber { get; set; }
}
