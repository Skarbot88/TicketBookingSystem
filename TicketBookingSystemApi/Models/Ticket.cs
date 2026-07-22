namespace TicketBookingSystemApi.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        public string? HolderName { get; set; }
        public TicketStatus Status { get; set; }
        public DateTime? ReservedAt { get; set; }
    }
}
