using Microsoft.EntityFrameworkCore;
using TicketBookingSystemApi.Data;
using TicketBookingSystemApi.Interfaces;
using TicketBookingSystemApi.Models;

namespace TicketBookingSystemApi.Repositories
{
	public class EventRepository(TicketBookingDataContext db) : IEventRepository
	{
		public async Task<Event?> GetWithTicketsAsync(int eventId)
		{
			return await db.Events
				.AsNoTracking()
				.Include(e => e.Tickets)
				.FirstOrDefaultAsync(e => e.Id == eventId);
		}
	}
}
