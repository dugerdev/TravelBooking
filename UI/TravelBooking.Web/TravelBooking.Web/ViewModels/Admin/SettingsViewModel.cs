using System.ComponentModel.DataAnnotations;

namespace TravelBooking.Web.ViewModels.Admin;

public class SettingsViewModel
{
    [Required]
    [Display(Name = "Site Adi")]
    public string SiteName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Site Email")]
    public string SiteEmail { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Destek Email")]
    public string SupportEmail { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Destek Telefonu")]
    public string? SupportPhone { get; set; }

    [Display(Name = "Bakim Modu")]
    public bool MaintenanceMode { get; set; }

    [Display(Name = "Kayit Izni")]
    public bool AllowRegistration { get; set; }

    [Display(Name = "Email Dogrulama Zorunlu")]
    public bool EmailVerificationRequired { get; set; }

    [Required]
    [Display(Name = "Varsayilan Para Birimi")]
    public string DefaultCurrency { get; set; } = "TRY";

    [Required]
    [Display(Name = "Varsayilan Dil")]
    public string DefaultLanguage { get; set; } = "tr-TR";
}
