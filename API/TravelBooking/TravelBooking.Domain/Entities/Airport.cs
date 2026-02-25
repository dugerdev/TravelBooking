using TravelBooking.Domain.Common;
using System.Collections.Generic;


namespace TravelBooking.Domain.Entities;

/// <summary>
/// Represents an airport entity in the domain.
/// Contains airport information and manages flight relationships.
/// </summary>
public class Airport : BaseEntity
{
    /// <summary>
    /// Gets the IATA (International Air Transport Association) code for the airport (e.g., IST, SAW, JFK).
    /// </summary>
    public string IATA_Code { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the city where the airport is located.
    /// </summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the country where the airport is located.
    /// </summary>
    public string Country { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the full name of the airport.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    private readonly List<Flight> _departureFlights = [];
    /// <summary>
    /// Gets the collection of flights departing from this airport.
    /// </summary>
    public IReadOnlyCollection<Flight> DepartureFlights => _departureFlights;

    private readonly List<Flight> _arrivalsFlights = [];
    /// <summary>
    /// Gets the collection of flights arriving at this airport.
    /// </summary>
    public IReadOnlyCollection<Flight> ArrivalFlights => _arrivalsFlights;

    /// <summary>
    /// Protected parameterless constructor for Entity Framework.
    /// </summary>
    protected Airport() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Airport"/> class.
    /// </summary>
    /// <param name="iataCode">The IATA code for the airport.</param>
    /// <param name="city">The city where the airport is located.</param>
    /// <param name="country">The country where the airport is located.</param>
    /// <param name="name">The full name of the airport.</param>
    public Airport(string iataCode, string city, string country, string name)
    {
        IATA_Code = iataCode.Trim().ToUpperInvariant();
        City = city.Trim();
        Country = country.Trim();
        Name = name.Trim();
    }

    /// <summary>
    /// Updates the airport details.
    /// </summary>
    /// <param name="city">The city where the airport is located.</param>
    /// <param name="country">The country where the airport is located.</param>
    /// <param name="name">The full name of the airport.</param>
    public void UpdateDetails(string city, string country, string name)
    {
        City = city.Trim();
        Country = country.Trim();
        Name = name.Trim();
    }

    /// <summary>
    /// Adds a departure flight to this airport.
    /// </summary>
    /// <param name="flight">The flight departing from this airport.</param>
    internal void AddDepartureFlight(Flight flight)
    {
        if (!_departureFlights.Contains(flight))
            _departureFlights.Add(flight);
    }

    /// <summary>
    /// Adds an arrival flight to this airport.
    /// </summary>
    /// <param name="flight">The flight arriving at this airport.</param>
    internal void AddArrivalFlight(Flight flight)
    {
        if (!_arrivalsFlights.Contains(flight))
            _arrivalsFlights.Add(flight);
    }
}
