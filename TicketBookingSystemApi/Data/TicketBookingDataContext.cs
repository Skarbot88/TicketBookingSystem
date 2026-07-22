using Microsoft.EntityFrameworkCore;
using TicketBookingSystemApi.Models;

namespace TicketBookingSystemApi.Data
{
    public class TicketBookingDataContext(DbContextOptions<TicketBookingDataContext> options) : DbContext(options)
    {
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Ticket> Tickets => Set<Ticket>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasMany(e => e.Tickets)
                      .WithOne(t => t.Event)
                      .HasForeignKey(t => t.EventId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasIndex(t => new { t.EventId, t.Status });
            });
        }
    }
}
