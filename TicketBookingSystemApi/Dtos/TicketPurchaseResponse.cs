namespace TicketBookingSystemApi.Dtos
{
    /// <summary>Details of a ticket that was just purchased.</summary>
    /// <param name="TicketId">The purchased ticket's id.</param>
    /// <param name="EventId">The event the ticket belongs to.</param>
    /// <param name="HolderName">Name the ticket was purchased under.</param>
    /// <param name="Status">The ticket's current status (expected to be "Sold").</param>
    public record TicketPurchaseResponse(int TicketId, int EventId, string HolderName, string Status);
}
