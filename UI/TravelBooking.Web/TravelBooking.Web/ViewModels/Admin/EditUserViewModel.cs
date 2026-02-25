using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Admin;

public class EditUserViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kullanici adi gereklidir")]
    [StringLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email gereklidir")]
    [EmailAddress(ErrorMessage = "Gecerli bir email adresi girin")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Gecerli bir telefon numarasi girin")]
    public string? PhoneNumber { get; set; }
}
