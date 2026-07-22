using Microsoft.AspNetCore.Mvc;
using TicketBookingSystemApi.Dtos;
using TicketBookingSystemApi.Interfaces;
using TicketBookingSystemApi.Services;

namespace TicketBookingSystemApi.Controllers;


/// <summary>
/// Endpoints for viewing event ticket availability and reserving tickets.
/// </summary>
[ApiController]
[Route("api/events")]
public class EventsController(
    IEventService eventService,
    ITicketReservationService reservationService,
    ILogger<EventsController> logger) : ControllerBase
{

    /// <summary>
    /// Gets ticket availability counts for a single event.
    /// </summary>
    /// <param name="id">The event's id.</param>
    /// <returns>
    /// A summary of the event including how many tickets are available,
    /// reserved (not yet expired), and sold. A reservation older than 10
    /// minutes counts toward "available", not "reserved".
    /// </returns>
    /// <response code="200">The event summary was found and returned.</response>
    /// <response code="404">No event exists with the given id.</response>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        var summary = await eventService.GetSummaryAsync(id);
        if (summary is null)
        {
            logger.LogWarning("GET /api/events/{EventId} - not found", id);
            return NotFound();
        }

        logger.LogInformation("GET /api/events/{EventId} - ok", id);
        return Ok(summary);
    }

    /// <summary>
    /// Reserves one available ticket for the given event on behalf of a named holder.
    /// </summary>
    /// <remarks>
    /// Picks any ticket currently <c>Available</c>, or one whose prior reservation
    /// has expired (older than 10 minutes). If two requests race for the last
    /// available ticket, exactly one succeeds; the other receives 409.
    /// </remarks>
    /// <param name="id">The event to reserve a ticket for.</param>
    /// <param name="request">The holder name to reserve the ticket under.</param>
    /// <response code="201">A ticket was reserved and is returned in the body.</response>
    /// <response code="400"><paramref name="request"/>.HolderName was missing or blank.</response>
    /// <response code="404">No event exists with the given id.</response>
    /// <response code="409">No tickets are currently available for this event.</response>
    [HttpPost("{id:int}/reserve")]
    public async Task<IActionResult> Reserve(int id, ReserveTicketRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.HolderName))
        {
            return Problem(title: "holderName is required", statusCode: StatusCodes.Status400BadRequest);
        }

        var result = await reservationService.ReserveAsync(id, request.HolderName.Trim());

        if (!result.Success)
        {
            return result.Error switch
            {
                ReservationError.EventNotFound => NotFound(),
                ReservationError.NoTicketsAvailable => Problem(
                    title: "No tickets available for this event",
                    statusCode: StatusCodes.Status409Conflict),
                _ => Problem("Unexpected error", statusCode: StatusCodes.Status500InternalServerError)
            };
        }

        return Created($"/api/events/{id}", result.Ticket);
    }
}