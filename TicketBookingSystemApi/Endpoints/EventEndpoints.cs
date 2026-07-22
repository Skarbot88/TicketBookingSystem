using System.Runtime.CompilerServices;
using TicketBookingSystemApi.Interfaces;

namespace TicketBookingSystemApi.Endpoints
{
    public static class EventEndpoints
    {
        public static void MapEventEndpoints(this WebApplication app)
        {
            app.MapGet("/api/events/{id:int}", async (int id, IEventService eventService, ILogger<Program> logger) =>
            {
                var summary = await eventService.GetSummaryAsync(id);

                if (summary is null)
                {
                    logger.LogWarning("GET /api/events/{EventId} - not found", id);
                    return Results.NotFound();
                }

                logger.LogInformation("GET /api/events/{EventId} - ok", id);
                return Results.Ok(summary);
            });
        }
    }
}
