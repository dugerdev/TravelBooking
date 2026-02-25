using System;
using System.Collections.Generic;
using TravelBooking.Domain.Enums;
using System.Linq;                  //---ValueObject icin gereklidir.

namespace TravelBooking.Domain.Common;

/// <summary>
/// Represents a monetary value with an amount and currency.
/// This is a value object used for financial calculations.
/// </summary>
public class Money : ValueObject
{
    /// <summary>
    /// Gets the monetary amount.
    /// </summary>
    public decimal Amount { get; private set; }

    /// <summary>
    /// Gets the currency type.
    /// </summary>
    public Currency Currency { get; private set; }

    /// <summary>
    /// Private parameterless constructor for Entity Framework.
    /// </summary>
    private Money()
    {
        Amount = 0;
        Currency = Currency.TRY;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The currency type.</param>
    public Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Adds another money value to this instance.
    /// Both money values must have the same currency.
    /// </summary>
    /// <param name="other">The money value to add.</param>
    /// <returns>A new Money instance with the sum of both amounts.</returns>
    /// <exception cref="ArgumentNullException">Thrown when other is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when currencies don't match.</exception>
    public Money Add(Money other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));
        
        //---Para birimi kontrolu (Domain Invariant)---//
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Farkli para birimleri toplanamaz. Mevcut: {Currency}, Eklenen: {other.Currency}");
        
        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Gets the components used for equality comparison.
    /// Two money instances are equal if they have the same amount and currency.
    /// </summary>
    /// <returns>The equality components.</returns>
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }   
}
