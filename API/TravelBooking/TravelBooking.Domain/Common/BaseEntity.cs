using System;
using System.Collections.Generic;
using TravelBooking.Domain.Events; 
using System.ComponentModel.DataAnnotations.Schema;



namespace TravelBooking.Domain.Common;

/// <summary>
/// Base entity class that provides common functionality for all domain entities.
/// Includes audit fields, status management, and domain event handling.
/// </summary>
public abstract class BaseEntity : IHasDomainEvents
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets the date and time when the entity was first created.
    /// </summary>
    public DateTime CreatedDate { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the ID of the user who created the entity.
    /// </summary>
    public string? CreatedBy { get; private set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedDate { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity is active in the system.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the entity has been soft deleted.
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

    /// <summary>
    /// Gets the collection of domain events associated with this entity.
    /// </summary>
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the entity's event collection.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }   

    /// <summary>
    /// Clears all domain events from the entity's event collection.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}