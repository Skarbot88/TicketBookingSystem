using System.Net;
using System.Net.Http.Json;
using TicketBookingSystemApi.Models;

namespace TicketBookingSystemApi.Tests
{
    public class ConcurrencyTests(TicketApiFactory factory) : IClassFixture<TicketApiFactory>
    {
        [Fact]
        public async Task Reserve_TwoConcurrentRequestsForLastTicket_ExactlyOneSucceeds()
        {
            // Arrange: an event with exactly one ticket left to fight over
            var eventId = await factory.SeedEventWithSingleAvailableTicketAsync();
            var client = factory.CreateClient();

            // Act: fire both requests at the same time - Task.WhenAll starts both before
            // awaiting either, so they genuinely race against the same row
            var requestA = client.PostAsJsonAsync(
                $"/api/events/{eventId}/reserve", new { holderName = "Alice" });
            var requestB = client.PostAsJsonAsync(
                $"/api/events/{eventId}/reserve", new { holderName = "Bob" });

            var responses = await Task.WhenAll(requestA, requestB);

            // Assert: exactly one winner, exactly one conflict - never both succeeding,
            // never both failing
            var createdCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
            var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

            Assert.Equal(1, createdCount);
            Assert.Equal(1, conflictCount);
        }

        [Fact]
        public async Task Purchase_TwoConcurrentPurchaseAttemptsSameTicket_ExactlyOneSucceeds()
        {
            var @event = await factory.SeedEventReturningEntityAsync(new Ticket
            {
                Status = TicketStatus.Reserved,
                HolderName = "Alice",
                ReservedAt = DateTime.UtcNow.AddMinutes(-2)
            });
            var ticketId = @event.Tickets[0].Id;
            var client = factory.CreateClient();

            var requestA = client.PostAsJsonAsync($"/api/tickets/{ticketId}/purchase", new { holderName = "Alice" });
            var requestB = client.PostAsJsonAsync($"/api/tickets/{ticketId}/purchase", new { holderName = "Alice" });

            var responses = await Task.WhenAll(requestA, requestB);

            Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.OK));
            Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Conflict));
        }
    }
}
