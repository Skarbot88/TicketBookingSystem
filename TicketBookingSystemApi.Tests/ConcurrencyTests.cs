using System.Net;
using System.Net.Http.Json;

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
    }
}
