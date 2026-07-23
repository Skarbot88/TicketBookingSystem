namespace TicketBookingSystemApi.Dtos
{
    /// <summary>Request body for purchasing a reserved ticket.</summary>
    /// <param name="HolderName">Must match the name the ticket was reserved under.</param>
    public record PurchaseTicketRequest(string HolderName);
}
