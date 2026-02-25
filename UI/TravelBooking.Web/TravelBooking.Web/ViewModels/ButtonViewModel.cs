namespace TravelBooking.Web.ViewModels;

public class ButtonViewModel
{
    public string Text { get; set; } = string.Empty;
    public string Variant { get; set; } = "primary";
    public string Icon { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string Size { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public bool Submit { get; set; }
    public bool Disabled { get; set; }
}
