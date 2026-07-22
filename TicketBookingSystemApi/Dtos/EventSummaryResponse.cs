namespace TicketBookingSystemApi.Dtos
{
    public record EventSummaryResponse(
        int Id,
        string Name,
        DateTime StartsAt,
        int TotalSeats,
        int AvailableCount,
        int ReservedCount,
        int SoldCount);
}
