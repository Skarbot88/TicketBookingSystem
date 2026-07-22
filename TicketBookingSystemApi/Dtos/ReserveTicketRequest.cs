namespace TicketBookingSystemApi.Dtos
{
    /// <summary>Request body for reserving a ticket.</summary>
    /// <param name="HolderName">Name of the person the ticket should be reserved under. Required.</param>
    public record ReserveTicketRequest(string HolderName);
}
