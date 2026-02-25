using TravelBooking.Domain.Common;
using TravelBooking.Domain.Enums;
using System;
using System.Collections.Generic;


namespace TravelBooking.Domain.Entities;

/// <summary>
/// Represents a passenger entity in the domain.
/// Contains passenger personal information and manages ticket relationships.
/// </summary>
public class Passenger: BaseEntity 
{
    /// <summary>
    /// Gets the first name of the passenger.
    /// </summary>
    public string PassengerFirstName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the last name of the passenger.
    /// </summary>
    public string PassengerLastName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the national identification number (e.g., Turkish ID number or foreign ID number).
    /// </summary>
    public string NationalNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the passport number of the passenger.
    /// </summary>
    public string PassportNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the date of birth of the passenger.
    /// </summary>
    public DateTime DateOfBirth { get; private set; }

    /// <summary>
    /// Gets the type of passenger (Adult, Child, Infant).
    /// </summary>
    public PassengerType PassengerType { get; private set; } = PassengerType.Adult;

    private readonly List<Ticket> _tickets = [];
    /// <summary>
    /// Gets the collection of tickets associated with this passenger.
    /// </summary>
    public IReadOnlyCollection<Ticket> Tickets => _tickets;

    /// <summary>
    /// Protected parameterless constructor for Entity Framework.
    /// </summary>
    protected Passenger() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Passenger"/> class.
    /// </summary>
    /// <param name="firstName">The first name of the passenger.</param>
    /// <param name="lastName">The last name of the passenger.</param>
    /// <param name="nationalNumber">The national identification number.</param>
    /// <param name="passportNumber">The passport number.</param>
    /// <param name="dateOfBirth">The date of birth.</param>
    /// <param name="passengerType">The type of passenger.</param>
    public Passenger(
        string firstName,
        string lastName,
        string nationalNumber,
        string passportNumber,
        DateTime dateOfBirth,
        PassengerType passengerType)
    {
        PassengerFirstName = firstName.Trim();
        PassengerLastName = lastName.Trim();
        NationalNumber = nationalNumber.Trim();
        PassportNumber = passportNumber.Trim();
        DateOfBirth = dateOfBirth;
        PassengerType = passengerType;
    }

    /// <summary>
    /// Updates the personal information of the passenger.
    /// </summary>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <param name="nationalNumber">The national identification number.</param>
    /// <param name="passportNumber">The passport number.</param>
    public void UpdatePersonalInfo(string firstName, string lastName, string nationalNumber, string passportNumber)
    {
        PassengerFirstName = firstName.Trim();
        PassengerLastName = lastName.Trim();
        NationalNumber = nationalNumber.Trim();
        PassportNumber = passportNumber.Trim();
    }

    /// <summary>
    /// Adds a ticket to this passenger.
    /// </summary>
    /// <param name="ticket">The ticket to add.</param>
    public void AddTicket(Ticket ticket)
    {
        if (!_tickets.Contains(ticket))
            _tickets.Add(ticket);
    }
}

