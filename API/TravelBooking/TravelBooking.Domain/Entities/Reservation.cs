using TravelBooking.Domain.Enums;
using TravelBooking.Domain.Common;
using TravelBooking.Domain.Events;  // ← EKLENDI
using System;
using System.Collections.Generic;
using TravelBooking.Domain.Identity;

namespace TravelBooking.Domain.Entities;

/// <summary>
/// Represents a reservation aggregate root in the domain.
/// A reservation contains tickets and payments, and manages the overall booking state.
/// </summary>
public class Reservation : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Gets the PNR (Passenger Name Record) - the unique reservation reference number.
    /// </summary>
    public string PNR { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the ID of the user who made the reservation.
    /// </summary>
    public string AppUserId { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the navigation property to the user who made the reservation.
    /// </summary>
    public AppUser AppUser { get; private set; } = null!;

    /// <summary>
    /// Gets the total price of the reservation.
    /// </summary>
    public Decimal TotalPrice { get; private set; }

    /// <summary>
    /// Gets the currency in which the reservation is priced.
    /// </summary>
    public Currency Currency { get; private set; } = Currency.TRY;

    /// <summary>
    /// Gets the type of reservation (Flight, Hotel, Car, Tour).
    /// </summary>
    public ReservationType Type { get; private set; } = ReservationType.Flight;

    /// <summary>
    /// Gets the ID of the hotel if this is a hotel reservation.
    /// </summary>
    public Guid? HotelId { get; private set; }

    /// <summary>
    /// Gets the navigation property to the hotel if this is a hotel reservation.
    /// </summary>
    public Hotel? Hotel { get; private set; }

    /// <summary>
    /// Gets the ID of the car if this is a car rental reservation.
    /// </summary>
    public Guid? CarId { get; private set; }

    /// <summary>
    /// Gets the navigation property to the car if this is a car rental reservation.
    /// </summary>
    public Car? Car { get; private set; }

    /// <summary>
    /// Gets the ID of the tour if this is a tour reservation.
    /// </summary>
    public Guid? TourId { get; private set; }

    /// <summary>
    /// Gets the navigation property to the tour if this is a tour reservation.
    /// </summary>
    public Tour? Tour { get; private set; }

    /// <summary>
    /// Gets the current payment status of the reservation.
    /// </summary>
    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;

    /// <summary>
    /// Gets the payment method used for this reservation.
    /// </summary>
    public PaymentMethod PaymentMethod { get; private set; } = PaymentMethod.Card;

    /// <summary>
    /// Gets the current status of the reservation.
    /// </summary>
    public ReservationStatus Status { get; private set; } = ReservationStatus.Pending;

    /// <summary>
    /// Gets the date and time when the reservation was created.
    /// </summary>
    public DateTime ReservationDate { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the expiration date of the reservation, if applicable.
    /// </summary>
    public DateTime? ExpirationDate { get; private set; }

    /// <summary>
    /// Gets the flight route summary for external flights (e.g. "Istanbul - Antalya").
    /// </summary>
    public string? FlightRouteSummary { get; private set; }

    /// <summary>Alis tarihi (arac kiralama).</summary>
    public DateTime? CarPickUpDate { get; private set; }
    /// <summary>Birakis tarihi (arac kiralama).</summary>
    public DateTime? CarDropOffDate { get; private set; }
    /// <summary>Alis yeri (arac kiralama).</summary>
    public string? CarPickUpLocation { get; private set; }
    /// <summary>Birakis yeri (arac kiralama).</summary>
    public string? CarDropOffLocation { get; private set; }
        
    /// <summary>
    /// Gets the row version used for optimistic concurrency control.
    /// </summary>
    public byte[]? RowVersion { get; private set; }

    // Backing fields must be writable so EF Core can populate them when using Include() with PropertyAccessMode.Field
    private List<Ticket> _tickets = [];
    /// <summary>
    /// Gets the collection of tickets associated with this reservation.
    /// </summary>
    public IReadOnlyCollection<Ticket> Tickets => _tickets;

    private readonly List<Payment> _payments = [];
    /// <summary>
    /// Gets the collection of payments associated with this reservation.
    /// </summary>
    public IReadOnlyCollection<Payment> Payments => _payments;

    private List<Passenger> _passengers = [];
    /// <summary>
    /// Gets the collection of passengers/participants associated with this reservation (e.g. for Tour/Hotel/Car without tickets).
    /// </summary>
    public IReadOnlyCollection<Passenger> Passengers => _passengers;

    /// <summary>
    /// Protected parameterless constructor for Entity Framework.
    /// </summary>
    protected Reservation() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Reservation"/> class.
    /// </summary>
    /// <param name="pnr">The PNR (Passenger Name Record) code.</param>
    /// <param name="appUserId">The ID of the user making the reservation.</param>
    /// <param name="totalPrice">The total price of the reservation.</param>
    /// <param name="currency">The currency of the reservation.</param>
    /// <param name="type">The type of reservation (Flight, Hotel, Car, Tour).</param>
    public Reservation(string pnr, string appUserId, decimal totalPrice, Currency currency, ReservationType type = ReservationType.Flight)
    {
        //---Not: Validasyonlar Application katmaninda yapilacaktir.
        PNR = pnr;
        AppUserId = appUserId;
        TotalPrice = totalPrice;
        Currency = currency;
        Type = type;
        ReservationDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Sets the hotel for this reservation.
    /// </summary>
    /// <param name="hotelId">The hotel ID to set.</param>
    public void SetHotel(Guid hotelId)
    {
        HotelId = hotelId;
        Type = ReservationType.Hotel;
    }

    /// <summary>
    /// Sets the car for this reservation.
    /// </summary>
    /// <param name="carId">The car ID to set.</param>
    public void SetCar(Guid carId)
    {
        CarId = carId;
        Type = ReservationType.Car;
    }

    /// <summary>
    /// Sets the tour for this reservation.
    /// </summary>
    /// <param name="tourId">The tour ID to set.</param>
    public void SetTour(Guid tourId)
    {
        TourId = tourId;
        Type = ReservationType.Tour;
    }

    /// <summary>
    /// Sets the PNR (Passenger Name Record) code for the reservation.
    /// </summary>
    /// <param name="pnr">The PNR code to set.</param>
    public void SetPNR(string pnr)
    {
        PNR = pnr;
    }

    /// <summary>
    /// Sets the flight route summary for external flights (e.g. "Istanbul - Antalya").
    /// </summary>
    /// <param name="routeSummary">The route summary to set.</param>
    public void SetFlightRouteSummary(string? routeSummary)
    {
        FlightRouteSummary = string.IsNullOrWhiteSpace(routeSummary) ? null : routeSummary.Trim();
    }

    /// <summary>Arac kiralama alis/birakis tarih ve yer bilgilerini set eder.</summary>
    public void SetCarRentalDetails(DateTime? pickUpDate, DateTime? dropOffDate, string? pickUpLocation, string? dropOffLocation)
    {
        CarPickUpDate = pickUpDate;
        CarDropOffDate = dropOffDate;
        CarPickUpLocation = string.IsNullOrWhiteSpace(pickUpLocation) ? null : pickUpLocation.Trim();
        CarDropOffLocation = string.IsNullOrWhiteSpace(dropOffLocation) ? null : dropOffLocation.Trim();
    }

    /// <summary>
    /// Updates the total price of the reservation.
    /// </summary>
    /// <param name="newTotalPrice">The new total price.</param>
    public void UpdateTotalPrice(decimal newTotalPrice)
    {
        TotalPrice = newTotalPrice;
    }

    /// <summary>
    /// Updates the payment status of the reservation.
    /// Automatically updates the reservation status based on payment status changes.
    /// </summary>
    /// <param name="status">The new payment status.</param>
    public void UpdatePaymentStatus(PaymentStatus status)
    {
        PaymentStatus = status;
        if (status == PaymentStatus.Paid)
        {
            Status = ReservationStatus.Confirmed;
            AddDomainEvent(new ReservationConfirmedEvent(this.Id));
        }
        else if (status == PaymentStatus.Failed)
        {
            Status = ReservationStatus.PaymentFailed;
            AddDomainEvent(new PaymentFailedEvent(this.Id, this.PNR));
        }
    }

    /// <summary>
    /// Updates the payment method for the reservation.
    /// </summary>
    /// <param name="method">The new payment method.</param>
    public void UpdatePaymentMethod(PaymentMethod method)
    {
        PaymentMethod = method;
    }

    /// <summary>
    /// Sets the expiration date for the reservation.
    /// </summary>
    /// <param name="expirationDate">The expiration date to set.</param>
    public void SetExpirationDate(DateTime expirationDate)
    {
        ExpirationDate = expirationDate;
    }

    /// <summary>
    /// Adds a ticket to this reservation.
    /// </summary>
    /// <param name="ticket">The ticket to add.</param>
    public void AddTicket(Ticket ticket)
    {
        if (!_tickets.Contains(ticket))
            _tickets.Add(ticket);
    }

    /// <summary>
    /// Adds a passenger/participant to this reservation (e.g. for Tour/Hotel/Car bookings without tickets).
    /// </summary>
    /// <param name="passenger">The passenger to add.</param>
    public void AddPassenger(Passenger passenger)
    {
        if (passenger == null)
            throw new ArgumentNullException(nameof(passenger));
        if (!_passengers.Contains(passenger))
            _passengers.Add(passenger);
    }

    /// <summary>
    /// Cancels the reservation and updates its status accordingly.
    /// Triggers a domain event when cancellation occurs.
    /// </summary>
    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled)
            return;

        Status = ReservationStatus.Cancelled;
        UpdatePaymentStatus(PaymentStatus.Failed);
        
        AddDomainEvent(new ReservationCancelledEvent(this.Id, this.PNR));
    }

    /// <summary>
    /// Sets the row version for optimistic concurrency control.
    /// </summary>
    /// <param name="rowVersion">The row version byte array.</param>
    internal void SetRowVersion(byte[] rowVersion)
    {
        RowVersion = rowVersion;
    }

    /// <summary>
    /// Adds a payment to this reservation.
    /// </summary>
    /// <param name="payment">The payment to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when payment is null.</exception>
    public void AddPayment(Payment payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        if (!_payments.Contains(payment))
            _payments.Add(payment);
    }
}
