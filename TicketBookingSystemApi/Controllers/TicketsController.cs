using Microsoft.AspNetCore.Mvc;
using TicketBookingSystemApi.Dtos;
using TicketBookingSystemApi.Interfaces;
using TicketBookingSystemApi.Models.enums;

namespace TicketBookingSystemApi.Controllers
{
    /// <summary>Endpoints for actions on a single ticket.</summary>
    [ApiController]
    [Route("api/tickets")]
    [Produces("application/json")]
    public class TicketsController(
        ITicketPurchaseService purchaseService,
        ILogger<TicketsController> logger) : ControllerBase
    {
        /// <summary>
        /// Converts a reserved ticket to purchased ("Sold"), if reserved by the same holder.
        /// </summary>
        /// <param name="id">The ticket to purchase.</param>
        /// <param name="request">The holder name; must match who the ticket was reserved under.</param>
        /// <response code="200">The ticket was purchased and is returned in the body.</response>
        /// <response code="400"><paramref name="request"/>.HolderName was missing or blank.</response>
        /// <response code="404">No ticket exists with the given id.</response>
        /// <response code="409">The ticket isn't currently reserved, its reservation expired, or it was reserved under a different name.</response>
        [HttpPost("{id:int}/purchase")]
        [ProducesResponseType(typeof(TicketPurchaseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Purchase(int id, PurchaseTicketRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.HolderName))
            {
                return Problem(title: "holderName is required", statusCode: StatusCodes.Status400BadRequest);
            }

            var result = await purchaseService.PurchaseAsync(id, request.HolderName.Trim());

            if (!result.Success)
            {
                return result.Error switch
                {
                    PurchaseFailureReason.TicketNotFound => NotFound(),
                    PurchaseFailureReason.NotReserved => Problem(
                        title: "Ticket is not currently reserved", statusCode: StatusCodes.Status409Conflict),
                    PurchaseFailureReason.Expired => Problem(
                        title: "Reservation has expired", statusCode: StatusCodes.Status409Conflict),
                    PurchaseFailureReason.WrongHolder => Problem(
                        title: "Ticket was reserved by a different holder", statusCode: StatusCodes.Status409Conflict),
                    _ => Problem("Unexpected error", statusCode: StatusCodes.Status500InternalServerError)
                };
            }

            logger.LogInformation("POST /api/tickets/{TicketId}/purchase - ok", id);
            return Ok(result.Ticket);
        }
    }
}
