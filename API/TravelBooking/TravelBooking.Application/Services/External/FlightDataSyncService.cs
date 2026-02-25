using TravelBooking.Application.Abstractions.External;
using TravelBooking.Application.Abstractions.Persistence;
using TravelBooking.Application.Common;
using TravelBooking.Application.Contracts;
using TravelBooking.Application.Dtos.External;
using TravelBooking.Application.Mappings;
using TravelBooking.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TravelBooking.Application.Services.External;

public class FlightDataSyncService : IFlightDataSyncService
{
    private readonly IExternalFlightApiClient _externalApiClient;
    private readonly IFlightService _flightService;
    private readonly IAirportService _airportService;
    private readonly ILogger<FlightDataSyncService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly bool _autoCreateAirports;

    public FlightDataSyncService(
        IExternalFlightApiClient externalApiClient,
        IFlightService flightService,
        IAirportService airportService,
        ILogger<FlightDataSyncService> logger,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _externalApiClient = externalApiClient;
        _flightService = flightService;
        _airportService = airportService;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _autoCreateAirports = configuration.GetValue<bool>("FlightSync:AutoCreateAirports", false);
    }

    public async Task<Result> SyncFlightsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting flight sync...");

            var externalFlights = await _externalApiClient.GetFlightsAsync(cancellationToken);
            var syncResult = await ProcessAndSaveFlightsAsync(externalFlights, cancellationToken);

            _logger.LogInformation("Flight sync completed. Processed {Count} flights.", externalFlights.Count());
            return syncResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flight sync failed");
            return new ErrorResult($"Flight sync failed: {ex.Message}");
        }
    }

    public async Task<Result> SyncFlightsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting flight sync for date range {StartDate} to {EndDate}...", startDate, endDate);

            var externalFlights = await _externalApiClient.GetFlightsByDateRangeAsync(startDate, endDate, cancellationToken);
            var syncResult = await ProcessAndSaveFlightsAsync(externalFlights, cancellationToken);

            _logger.LogInformation("Flight sync completed. Processed {Count} flights.", externalFlights.Count());
            return syncResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flight sync failed for date range");
            return new ErrorResult($"Flight sync failed: {ex.Message}");
        }
    }

    public async Task<DataResult<Guid>> ImportSingleExternalFlightAsync(ExternalFlightDto dto, bool autoCreateAirports, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.DepartureAirportIATA) || string.IsNullOrWhiteSpace(dto.ArrivalAirportIATA))
                return new ErrorDataResult<Guid>(Guid.Empty, "Kalkis ve varis havalimani IATA kodlari zorunludur.");
            if (dto.DepartureAirportIATA.Length != 3 || dto.ArrivalAirportIATA.Length != 3)
                return new ErrorDataResult<Guid>(Guid.Empty, "IATA kodlari 3 karakter olmalidir.");

            Guid depId;
            var depResult = await _airportService.GetByIATACodeAsync(dto.DepartureAirportIATA, cancellationToken);
            if (!depResult.Success)
            {
                if (!autoCreateAirports)
                    return new ErrorDataResult<Guid>(Guid.Empty, $"Kalkis havalimani bulunamadi: {dto.DepartureAirportIATA}. Auto-create kapali.");
                var newDep = new Airport(dto.DepartureAirportIATA, dto.DepartureCity ?? "Unknown", dto.DepartureCountry ?? "Unknown", dto.DepartureAirportName ?? dto.DepartureAirportIATA);
                var createDep = await _airportService.AddAsync(newDep, cancellationToken);
                if (!createDep.Success)
                    return new ErrorDataResult<Guid>(Guid.Empty, $"Kalkis havalimani olusturulamadi: {dto.DepartureAirportIATA}");
                depResult = await _airportService.GetByIATACodeAsync(dto.DepartureAirportIATA, cancellationToken);
                if (!depResult.Success || depResult.Data == null)
                    return new ErrorDataResult<Guid>(Guid.Empty, $"Kalkis havalimani olusturuldu ancak alinamadi: {dto.DepartureAirportIATA}");
                depId = depResult.Data.Id;
            }
            else
            {
                if (depResult.Data == null)
                    return new ErrorDataResult<Guid>(Guid.Empty, $"Kalkis havalimani verisi alinamadi: {dto.DepartureAirportIATA}");
                depId = depResult.Data.Id;
            }

            Guid arrId;
            var arrResult = await _airportService.GetByIATACodeAsync(dto.ArrivalAirportIATA, cancellationToken);
            if (!arrResult.Success)
            {
                if (!autoCreateAirports)
                    return new ErrorDataResult<Guid>(Guid.Empty, $"Varis havalimani bulunamadi: {dto.ArrivalAirportIATA}. Auto-create kapali.");
                var newArr = new Airport(dto.ArrivalAirportIATA, dto.ArrivalCity ?? "Unknown", dto.ArrivalCountry ?? "Unknown", dto.ArrivalAirportName ?? dto.ArrivalAirportIATA);
                var createArr = await _airportService.AddAsync(newArr, cancellationToken);
                if (!createArr.Success)
                    return new ErrorDataResult<Guid>(Guid.Empty, $"Varis havalimani olusturulamadi: {dto.ArrivalAirportIATA}");
                arrResult = await _airportService.GetByIATACodeAsync(dto.ArrivalAirportIATA, cancellationToken);
                if (!arrResult.Success || arrResult.Data == null)
                    return new ErrorDataResult<Guid>(Guid.Empty, $"Varis havalimani olusturuldu ancak alinamadi: {dto.ArrivalAirportIATA}");
                arrId = arrResult.Data.Id;
            }
            else
            {
                if (arrResult.Data == null)
                    return new ErrorDataResult<Guid>(Guid.Empty, $"Varis havalimani verisi alinamadi: {dto.ArrivalAirportIATA}");
                arrId = arrResult.Data.Id;
            }

            var flight = dto.ToDomainEntity(depId, arrId);
            var upsert = await _flightService.UpsertAsync(flight, cancellationToken);
            if (!upsert.Success)
                return new ErrorDataResult<Guid>(Guid.Empty, upsert.Message);

            var get = await _flightService.GetByFlightNumberAsync(flight.FlightNumber, flight.ScheduledDeparture, cancellationToken);
            if (!get.Success || get.Data == null)
                return new ErrorDataResult<Guid>(Guid.Empty, "Ucus kaydedildi ancak Id alinamadi.");
            return new SuccessDataResult<Guid>(get.Data.Id, "Ucus basariyla ice aktarildi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ImportSingleExternalFlightAsync failed for {FlightNumber}", dto.FlightNumber);
            return new ErrorDataResult<Guid>(Guid.Empty, $"Ice aktarma hatasi: {ex.Message}");
        }
    }

    private async Task<Result> ProcessAndSaveFlightsAsync(
        IEnumerable<ExternalFlightDto> externalFlights,
        CancellationToken cancellationToken)
    {
        var flightsToUpsert = new List<Flight>();
        var processedCount = 0;
        var errorCount = 0;

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var externalFlight in externalFlights)
            {
                try
                {
                    //---Kalkis havalimanini kontrol et veya olustur---//
                    var departureAirportResult = await _airportService.GetByIATACodeAsync(externalFlight.DepartureAirportIATA, cancellationToken);
                    if (!departureAirportResult.Success)
                    {
                        if (_autoCreateAirports)
                        {
                            //---Havalimanini otomatik olustur---//
                            var newDepartureAirport = new Airport(
                                externalFlight.DepartureAirportIATA,
                                externalFlight.DepartureAirportName ?? externalFlight.DepartureAirportIATA,
                                externalFlight.DepartureCity ?? "Unknown",
                                externalFlight.DepartureCountry ?? "Unknown"
                            );
                            var createResult = await _airportService.AddAsync(newDepartureAirport, cancellationToken);
                            if (createResult.Success)
                            {
                                departureAirportResult = await _airportService.GetByIATACodeAsync(externalFlight.DepartureAirportIATA, cancellationToken);
                                _logger.LogInformation("Auto-created departure airport: {IATA} - {Name}", 
                                    externalFlight.DepartureAirportIATA, newDepartureAirport.Name);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to auto-create departure airport: {IATA}", externalFlight.DepartureAirportIATA);
                                errorCount++;
                                continue;
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Departure airport not found: {IATA}. Auto-create is disabled.", externalFlight.DepartureAirportIATA);
                            errorCount++;
                            continue;
                        }
                    }

                    //---Varis havalimanini kontrol et veya olustur---//
                    var arrivalAirportResult = await _airportService.GetByIATACodeAsync(externalFlight.ArrivalAirportIATA, cancellationToken);
                    if (!arrivalAirportResult.Success)
                    {
                        if (_autoCreateAirports)
                        {
                            //---Havalimanini otomatik olustur---//
                            var newArrivalAirport = new Airport(
                                externalFlight.ArrivalAirportIATA,
                                externalFlight.ArrivalAirportName ?? externalFlight.ArrivalAirportIATA,
                                externalFlight.ArrivalCity ?? "Unknown",
                                externalFlight.ArrivalCountry ?? "Unknown"
                            );
                            var createResult = await _airportService.AddAsync(newArrivalAirport, cancellationToken);
                            if (createResult.Success)
                            {
                                arrivalAirportResult = await _airportService.GetByIATACodeAsync(externalFlight.ArrivalAirportIATA, cancellationToken);
                                _logger.LogInformation("Auto-created arrival airport: {IATA} - {Name}", 
                                    externalFlight.ArrivalAirportIATA, newArrivalAirport.Name);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to auto-create arrival airport: {IATA}", externalFlight.ArrivalAirportIATA);
                                errorCount++;
                                continue;
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Arrival airport not found: {IATA}. Auto-create is disabled.", externalFlight.ArrivalAirportIATA);
                            errorCount++;
                            continue;
                        }
                    }

                    // Map to domain entity
                    if (departureAirportResult.Data == null || arrivalAirportResult.Data == null)
                    {
                        _logger.LogWarning("Airport data is null for flight {FlightNumber}", externalFlight.FlightNumber);
                        errorCount++;
                        continue;
                    }

                    var flight = externalFlight.ToDomainEntity(
                        departureAirportResult.Data.Id,
                        arrivalAirportResult.Data.Id
                    );

                    flightsToUpsert.Add(flight);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing flight {FlightNumber}", externalFlight.FlightNumber);
                    errorCount++;
                }
            }

            // Bulk upsert flights
            foreach (var flight in flightsToUpsert)
            {
                await _flightService.UpsertAsync(flight, cancellationToken);
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var message = $"Sync completed. Processed: {processedCount}, Errors: {errorCount}";
            _logger.LogInformation(message);

            return new SuccessResult(message);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error during flight sync transaction");
            return new ErrorResult($"Flight sync failed: {ex.Message}");
        }
    }
}
