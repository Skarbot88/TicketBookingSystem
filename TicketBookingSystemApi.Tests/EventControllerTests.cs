using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TicketBookingSystemApi.Dtos;
using TicketBookingSystemApi.Models;

namespace TicketBookingSystemApi.Tests
{
    public class EventEndpointTests(TicketApiFactory factory) : IClassFixture<TicketApiFactory>
    {
        [Fact]
        public async Task GetEvent_MixOfStatuses_CountsExpiredReservationAsAvailable()
        {
            // This is the test that actually proves the 10-minute expiry rule works,
            // not just that the counting arithmetic is right. Without the expired
            // ticket here, a bug that ignored ReservedAt entirely would still pass.
            var expiredReservedAt = DateTime.UtcNow.AddMinutes(-11);
            var freshReservedAt = DateTime.UtcNow.AddMinutes(-2);

            var eventId = await factory.SeedEventAsync(
                new Ticket { Status = TicketStatus.Available },
                new Ticket { Status = TicketStatus.Available },
                new Ticket { Status = TicketStatus.Reserved, HolderName = "Alice", ReservedAt = freshReservedAt },
                new Ticket { Status = TicketStatus.Reserved, HolderName = "Bob", ReservedAt = expiredReservedAt },
                new Ticket { Status = TicketStatus.Sold, HolderName = "Carol" });

            var client = factory.CreateClient();
            var response = await client.GetAsync($"/api/events/{eventId}");
            var summary = await response.Content.ReadFromJsonAsync<EventSummaryResponse>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, summary!.AvailableCount);   // 2 genuinely available + Bob's expired one
            Assert.Equal(1, summary.ReservedCount);     // only Alice's, not yet expired
            Assert.Equal(1, summary.SoldCount);
        }

        [Fact]
        public async Task GetEvent_UnknownId_ReturnsNotFound()
        {
            var client = factory.CreateClient();
            var response = await client.GetAsync("/api/events/999999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
