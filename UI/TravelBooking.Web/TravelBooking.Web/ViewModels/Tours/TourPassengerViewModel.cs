using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Tours;

public class TourPassengerViewModel
{
    [Required(ErrorMessage = "Cinsiyet gereklidir")]
    [Display(Name = "Cinsiyet")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ad gereklidir")]
    [StringLength(100)]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad gereklidir")]
    [StringLength(100)]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ulke gereklidir")]
    [StringLength(10)]
    [Display(Name = "Ulke")]
    public string Country { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Dogum Tarihi")]
    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress(ErrorMessage = "Gecerli bir e-posta girin")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon gereklidir")]
    [Display(Name = "Telefon")]
    public string Phone { get; set; } = string.Empty;
}
