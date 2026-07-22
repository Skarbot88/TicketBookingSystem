using System.Net;
using System.Net.Http.Json;
using TicketBookingSystemApi.Dtos;
using TicketBookingSystemApi.Models;

namespace TicketBookingSystemApi.Tests
{
    public class ReserveEndpointTests(TicketApiFactory factory) : IClassFixture<TicketApiFactory>
    {
        [Fact]
        public async Task Reserve_AvailableTicket_ReturnsCreatedAndTrimsHolderName()
        {
            var eventId = await factory.SeedEventAsync(new Ticket { Status = TicketStatus.Available });
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                $"/api/events/{eventId}/reserve", new { holderName = "  Alice  " });
            var ticket = await response.Content.ReadFromJsonAsync<TicketReservationResponse>();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("Alice", ticket!.HolderName); 
            Assert.Equal("Reserved", ticket.Status);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Reserve_BlankHolderName_ReturnsBadRequest(string holderName)
        {
            var eventId = await factory.SeedEventAsync(new Ticket { Status = TicketStatus.Available });
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                $"/api/events/{eventId}/reserve", new { holderName });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Reserve_UnknownEvent_ReturnsNotFound()
        {
            var client = factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                "/api/events/999999/reserve", new { holderName = "Alice" });

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Reserve_NoTicketsAvailable_ReturnsConflict()
        {
            var eventId = await factory.SeedEventAsync(
                new Ticket { Status = TicketStatus.Sold, HolderName = "Existing Holder" });

            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync(
                $"/api/events/{eventId}/reserve", new { holderName = "Alice" });

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task Reserve_ExpiredReservation_CanBeReReservedByAnotherHolder()
        {
            var eventId = await factory.SeedEventAsync(new Ticket
            {
                Status = TicketStatus.Reserved,
                HolderName = "Original Holder",
                ReservedAt = DateTime.UtcNow.AddMinutes(-15)
            });

            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync(
                $"/api/events/{eventId}/reserve", new { holderName = "New Holder" });
            var ticket = await response.Content.ReadFromJsonAsync<TicketReservationResponse>();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal("New Holder", ticket!.HolderName);
        }

        [Fact]
        public async Task Reserve_ThenGetEvent_ReflectsUpdatedCounts()
        {
            var eventId = await factory.SeedEventAsync(
                new Ticket { Status = TicketStatus.Available },
                new Ticket { Status = TicketStatus.Available });

            var client = factory.CreateClient();
            await client.PostAsJsonAsync($"/api/events/{eventId}/reserve", new { holderName = "Alice" });

            var response = await client.GetAsync($"/api/events/{eventId}");
            var summary = await response.Content.ReadFromJsonAsync<EventSummaryResponse>();

            Assert.Equal(1, summary!.AvailableCount);
            Assert.Equal(1, summary.ReservedCount);
        }
    }
}
