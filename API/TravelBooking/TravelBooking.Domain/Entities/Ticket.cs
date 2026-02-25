using TravelBooking.Domain.Common;
using TravelBooking.Domain.Enums;
using TravelBooking.Domain.Events;
using System;
using System.Collections.Generic;

namespace TravelBooking.Domain.Entities;

/// <summary>
/// Represents a ticket entity in the domain.
/// Contains ticket information linking flights, reservations, and passengers.
/// </summary>
public class Ticket : BaseEntity
{
    /// <summary>
    /// Gets the ID of the flight this ticket is for.
    /// </summary>
    public Guid FlightId { get; private set; }

    /// <summary>
    /// Gets the navigation property to the flight.
    /// </summary>
    public Flight Flight { get; private set; } = null!;

    /// <summary>
    /// Gets the ID of the reservation this ticket belongs to.
    /// </summary>
    public Guid ReservationId { get; private set; }

    /// <summary>
    /// Gets the navigation property to the reservation.
    /// </summary>
    public Reservation Reservation { get; private set; } = null!;

    /// <summary>
    /// Gets the ID of the passenger this ticket is for.
    /// </summary>
    public Guid PassengerId { get; private set; }

    /// <summary>
    /// Gets the navigation property to the passenger.
    /// </summary>
    public Passenger Passenger { get; private set; } = null!;

    /// <summary>
    /// Gets the email address of the passenger.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the contact phone number of the passenger.
    /// </summary>
    public string ContactPhoneNumber { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the seat class for this ticket (Economy, Business, First Class).
    /// </summary>
    public SeatClass SeatClass { get; private set; }

    /// <summary>
    /// Gets the assigned seat number, if any.
    /// </summary>
    public string? SeatNumber { get; private set; }

    /// <summary>
    /// Gets the baggage option selected for this ticket (Light, Standard, Extra).
    /// </summary>
    public BaggageOption BaggageOption { get; private set; } = BaggageOption.Light;

    /// <summary>
    /// Gets the base ticket price for the flight and seat class.
    /// </summary>
    public decimal TicketPrice { get; private set; }

    /// <summary>
    /// Gets the baggage fee for this ticket.
    /// </summary>
    public decimal BaggageFee { get; private set; }

    /// <summary>
    /// Gets the current status of the ticket (Reserved, Cancelled, Used).
    /// </summary>
    public TicketStatus TicketStatus { get; private set; } = TicketStatus.Reserved;

    /// <summary>
    /// Gets the date and time when the ticket was cancelled, if applicable.
    /// </summary>
    public DateTime? CancelledAt { get; private set; }

    /// <summary>
    /// Protected parameterless constructor for Entity Framework.
    /// </summary>
    protected Ticket() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ticket"/> class.
    /// </summary>
    /// <param name="flightId">The ID of the flight.</param>
    /// <param name="reservationId">The ID of the reservation.</param>
    /// <param name="passengerId">The ID of the passenger.</param>
    /// <param name="email">The passenger's email address.</param>
    /// <param name="contactPhoneNumber">The passenger's contact phone number.</param>
    /// <param name="seatClass">The seat class for the ticket.</param>
    /// <param name="baggageOption">The baggage option selected.</param>
    /// <param name="ticketPrice">The base ticket price.</param>
    /// <param name="baggageFee">The baggage fee.</param>
    public Ticket(
        Guid flightId,
        Guid reservationId,
        Guid passengerId,
        string email,
        string contactPhoneNumber,
        SeatClass seatClass,
        BaggageOption baggageOption,
        decimal ticketPrice,
        decimal baggageFee)
    {
        //---Not: Validasyonlar Application katmaninda yapilacaktir.
        FlightId = flightId;
        ReservationId = reservationId;
        PassengerId = passengerId;
        Email = email;
        ContactPhoneNumber = contactPhoneNumber;
        SeatClass = seatClass;
        BaggageOption = baggageOption;
        TicketPrice = ticketPrice;
        BaggageFee = baggageFee;
        TicketStatus = TicketStatus.Reserved;
    }

    /// <summary>
    /// Assigns a seat number to this ticket.
    /// </summary>
    /// <param name="seatNumber">The seat number to assign.</param>
    public void AssignSeat(string seatNumber)
    {
        SeatNumber = seatNumber;
    }

    /// <summary>
    /// Updates the status of the ticket.
    /// Triggers a domain event when the ticket is cancelled.
    /// </summary>
    /// <param name="status">The new ticket status.</param>
    public void UpdateStatus(TicketStatus status)
    {
        TicketStatus = status;
        if (status == TicketStatus.Cancelled)
        {
            CancelledAt = DateTime.UtcNow;
            AddDomainEvent(new TicketCancelledEvent(this.Id, this.FlightId, this.ReservationId));
        }
    }

    /// <summary>
    /// Updates the baggage option and associated fee for this ticket.
    /// </summary>
    /// <param name="option">The new baggage option.</param>
    /// <param name="newBaggageFee">The new baggage fee.</param>
    public void UpdateBaggageOption(BaggageOption option, decimal newBaggageFee)
    {
        BaggageOption = option;
        BaggageFee = newBaggageFee;
    }

    /// <summary>
    /// Sets the navigation property to the flight.
    /// </summary>
    /// <param name="flight">The flight to associate with this ticket.</param>
    internal void SetFlight(Flight flight)
    {
        Flight = flight;
    }

    /// <summary>
    /// Sets the navigation property to the reservation.
    /// </summary>
    /// <param name="reservation">The reservation to associate with this ticket.</param>
    internal void SetReservation(Reservation reservation)
    {
        Reservation = reservation;
    }

    /// <summary>
    /// Sets the navigation property to the passenger.
    /// </summary>
    /// <param name="passenger">The passenger to associate with this ticket.</param>
    internal void SetPassenger(Passenger passenger)
    {
        Passenger = passenger;
    }
}