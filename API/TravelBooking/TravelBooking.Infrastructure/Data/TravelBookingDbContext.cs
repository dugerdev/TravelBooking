using TravelBooking.Domain.Entities;
using TravelBooking.Domain.Identity;
using TravelBooking.Domain.Identity.Tokens;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TravelBooking.Infrastructure.Data;

//---EF Core DbContext sinifi---//
//---Uygulamanin veritabani baglamini temsil eder---//
public class TravelBookingDbContext : IdentityDbContext<AppUser>
{
    public TravelBookingDbContext(DbContextOptions<TravelBookingDbContext> options) : base(options)
    {
    }

    //---DbSet tanimlari - her entity icin bir tabloyu temsil eder---//
    public DbSet<Flight> Flights { get; set; } = null!;
    public DbSet<Reservation> Reservations { get; set; } = null!;
    public DbSet<Ticket> Tickets { get; set; } = null!;
    public DbSet<Passenger> Passengers { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Airport> Airports { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Hotel> Hotels { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Car> Cars { get; set; } = null!;
    public DbSet<Tour> Tours { get; set; } = null!;
    public DbSet<NewsArticle> News { get; set; } = null!;
    public DbSet<ContactMessage> ContactMessages { get; set; } = null!;
    public DbSet<Testimonial> Testimonials { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //---Tum IEntityTypeConfiguration implementasyonlarini assembly uzerinden uygular---//
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TravelBookingDbContext).Assembly);
    }
}
