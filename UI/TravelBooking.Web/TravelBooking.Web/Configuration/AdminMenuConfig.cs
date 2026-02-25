namespace TravelBooking.Web.Configuration;

/// <summary>
/// Admin sidebar menü öğesi tanımı.
/// RequiredRoles boşsa tüm Admin kullanıcıları görür; doluysa sadece belirtilen roller görür.
/// </summary>
public class AdminMenuItem
{
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = "Index";
    public string Label { get; set; } = string.Empty;
    public string Icon { get; set; } = "fas fa-fw fa-circle";
    /// <summary>Boş veya null ise tüm Admin'lere görünür. Doluysa sadece bu rollerden biri olan kullanıcılar görür.</summary>
    public string[]? RequiredRoles { get; set; }
    public string? BadgeKey { get; set; }
    public bool IsExternal { get; set; }
    public string? ExternalUrl { get; set; }
}

/// <summary>
/// Admin sidebar menü grupları ve öğeleri.
/// </summary>
public class AdminMenuGroup
{
    public string Heading { get; set; } = string.Empty;
    public List<AdminMenuItem> Items { get; set; } = new();
}

/// <summary>
/// Admin sidebar menü yapılandırması. Role-based görünürlük için kullanılır.
/// </summary>
public static class AdminMenuConfig
{
    public static IReadOnlyList<AdminMenuGroup> GetMenuGroups() => new List<AdminMenuGroup>
    {
        new()
        {
            Heading = "Analytics & Reports",
            Items =
            [
                new() { Controller = "Dashboard", Action = "Index", Label = "Overview", Icon = "fas fa-fw fa-chart-line" }
            ]
        },
        new()
        {
            Heading = "Service Management",
            Items =
            [
                new() { Controller = "FlightsAdmin", Action = "Index", Label = "Flights", Icon = "fas fa-fw fa-plane-departure", RequiredRoles = ["Admin"] },
                new() { Controller = "AirportsAdmin", Action = "Index", Label = "Havalimanları", Icon = "fas fa-fw fa-map-marker-alt", RequiredRoles = ["Admin"] },
                new() { Controller = "Hotels", Action = "Index", Label = "Hotels", Icon = "fas fa-fw fa-hotel", RequiredRoles = ["Admin"] },
                new() { Controller = "Cars", Action = "Index", Label = "Cars", Icon = "fas fa-fw fa-car", RequiredRoles = ["Admin"] },
                new() { Controller = "Tours", Action = "Index", Label = "Tours", Icon = "fas fa-fw fa-map-marked-alt", RequiredRoles = ["Admin"] }
            ]
        },
        new()
        {
            Heading = "Reservation Management",
            Items =
            [
                new() { Controller = "ReservationsAdmin", Action = "Index", Label = "All Reservations", Icon = "fas fa-fw fa-ticket-alt", RequiredRoles = ["Admin"] }
            ]
        },
        new()
        {
            Heading = "Payment Management",
            Items =
            [
                new() { Controller = "Payments", Action = "Index", Label = "Transactions", Icon = "fas fa-fw fa-credit-card", RequiredRoles = ["Admin"] }
            ]
        },
        new()
        {
            Heading = "User Management",
            Items =
            [
                new() { Controller = "UsersAdmin", Action = "Index", Label = "Users", Icon = "fas fa-fw fa-users", RequiredRoles = ["Admin"] }
            ]
        },
        new()
        {
            Heading = "Communication & Content",
            Items =
            [
                new() { Controller = "MessagesAdmin", Action = "Index", Label = "Contact Messages", Icon = "fas fa-fw fa-envelope", RequiredRoles = ["Admin"], BadgeKey = "UnreadMessagesCount" },
                new() { Controller = "TestimonialsAdmin", Action = "Index", Label = "Testimonials", Icon = "fas fa-fw fa-star", RequiredRoles = ["Admin"], BadgeKey = "PendingTestimonialsCount" },
                new() { Controller = "News", Action = "Index", Label = "News Management", Icon = "fas fa-fw fa-newspaper", RequiredRoles = ["Admin"] }
            ]
        },
        new()
        {
            Heading = "API & System",
            Items =
            [
                new() { IsExternal = true, ExternalUrl = "https://localhost:7283/swagger", Label = "API Manager", Icon = "fas fa-fw fa-code", RequiredRoles = ["Admin"] },
                new() { Controller = "Settings", Action = "Index", Label = "Settings", Icon = "fas fa-fw fa-cogs", RequiredRoles = ["Admin"] }
            ]
        }
    };
}
