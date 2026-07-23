using System.Net;
using System.Net.Http.Json;
using TicketBookingSystemApi.Dtos;
using TicketBookingSystemApi.Models;

namespace TicketBookingSystemApi.Tests
{
    public class PurchaseEndpointTests(TicketApiFactory factory) : IClassFixture<TicketApiFactory>
    {
        [Fact]
        public async Task Purchase_ReservedBySameHolder_ReturnsOkAndMarksSold()
        {
            var @event = await factory.SeedEventReturningEntityAsync(new Ticket
            {
                Status = TicketStatus.Reserved,
                HolderName = "Alice",
                ReservedAt = DateTime.UtcNow.AddMinutes(-2)
            });
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                $"/api/tickets/{@event.Tickets[0].Id}/purchase", new { holderName = "Alice" });
            var body = await response.Content.ReadFromJsonAsync<TicketPurchaseResponse>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Sold", body!.Status);
        }

        [Fact]
        public async Task Purchase_DifferentHolder_ReturnsConflict()
        {
            var @event = await factory.SeedEventReturningEntityAsync(new Ticket
            {
                Status = TicketStatus.Reserved,
                HolderName = "Alice",
                ReservedAt = DateTime.UtcNow.AddMinutes(-2)
            });
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                $"/api/tickets/{@event.Tickets[0].Id}/purchase", new { holderName = "Bob" });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Purchase_TicketStillAvailable_ReturnsConflict()
        {
            var @event = await factory.SeedEventReturningEntityAsync(new Ticket { Status = TicketStatus.Available });
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                $"/api/tickets/{@event.Tickets[0].Id}/purchase", new { holderName = "Alice" });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Purchase_ExpiredReservation_ReturnsConflict()
        {
            // Distinct from the wrong-holder test - same holder, but the 10-minute
            // window has passed, which must fail too, not silently succeed.
            var @event = await factory.SeedEventReturningEntityAsync(new Ticket
            {
                Status = TicketStatus.Reserved,
                HolderName = "Alice",
                ReservedAt = DateTime.UtcNow.AddMinutes(-15)
            });
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                $"/api/tickets/{@event.Tickets[0].Id}/purchase", new { holderName = "Alice" });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Purchase_UnknownTicketId_ReturnsNotFound()
        {
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                "/api/tickets/999999/purchase", new { holderName = "Alice" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Purchase_BlankHolderName_ReturnsBadRequest(string holderName)
        {
            var @event = await factory.SeedEventReturningEntityAsync(new Ticket
            {
                Status = TicketStatus.Reserved,
                HolderName = "Alice",
                ReservedAt = DateTime.UtcNow.AddMinutes(-2)
            });
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                $"/api/tickets/{@event.Tickets[0].Id}/purchase", new { holderName });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
