namespace TicketBookingSystemApi.Dtos
{
    /// <summary>Details of a ticket that was just reserved.</summary>
    /// <param name="TicketId">The reserved ticket's id.</param>
    /// <param name="EventId">The event the ticket belongs to.</param>
    /// <param name="HolderName">Name the ticket was reserved under.</param>
    /// <param name="ReservedAt">UTC time the reservation was made. The reservation expires 10 minutes after this.</param>
    /// <param name="Status">The ticket's current status (expected to be "Reserved").</param>
    public record TicketReservationResponse(
     int TicketId,
     int EventId,
     string HolderName,
     DateTime ReservedAt,
     string Status);
}
