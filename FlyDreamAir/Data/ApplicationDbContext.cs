using FlyDreamAir.Data.Db;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlyDreamAir.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<AddOn> AddOns { get; init; }
        public DbSet<Booking> Bookings { get; init; }
        public DbSet<Customer> Customers { get; init; }
        public DbSet<Flight> Flights { get; init; }
        public DbSet<Luggage> Luggage { get; init; }
        public DbSet<Meal> Meals { get; init; }
        public DbSet<OrderedAddOn> OrderedAddOns { get; init; }
        public DbSet<Payment> Payments { get; init; }
        public DbSet<ScheduledFlight> ScheduledFlights { get; init; }
        public DbSet<Seat> Seats { get; init; }
        public DbSet<Ticket> Tickets { get; init; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AddOn>(b =>
            {
                b.HasDiscriminator(e => e.Type)
                    .HasValue<AddOn>(nameof(AddOn))
                    .HasValue<Luggage>(nameof(Db.Luggage))
                    .HasValue<Meal>(nameof(Meal))
                    .HasValue<Seat>(nameof(Seat));
            });

            builder.Entity<Booking>(b =>
            {
                b.HasIndex(e => e.CancellationId).IsUnique();
                b.HasOne(e => e.Customer).WithMany();
                b.HasMany<Payment>().WithOne(e => e.Booking);
            });

            builder.Entity<Customer>(b =>
            {
                b.HasIndex(e => e.Email).IsUnique();
                b.HasIndex(e => e.UserName).IsUnique();
            });

            builder.Entity<Flight>(b =>
            {
                b.HasIndex(e => e.FlightId).IsUnique();
            });

            builder.Entity<OrderedAddOn>(b =>
            {
                b.HasKey(
                    nameof(OrderedAddOn.Ticket) + nameof(Ticket.Id),
                    nameof(OrderedAddOn.AddOn) + nameof(AddOn.Id)
                );
                b.HasOne(e => e.Ticket).WithMany();
                b.HasOne(e => e.AddOn).WithMany();
            });

            builder.Entity<Payment>(b =>
            {
                b.HasDiscriminator(e => e.Type)
                    .HasValue<Payment>(nameof(Payment))
                    .HasValue<CreditCardPayment>(nameof(CreditCardPayment));
            });

            builder.Entity<ScheduledFlight>(b =>
            {
                b.HasKey(
                    nameof(ScheduledFlight.Flight) + nameof(Flight.Id),
                    nameof(ScheduledFlight.DepartureTime)
                );
                b.HasOne(e => e.Flight).WithMany();
            });

            builder.Entity<Seat>(b =>
            {
                b.HasOne(e => e.Flight).WithMany();
            });

            builder.Entity<Ticket>(b =>
            {
                b.HasOne(e => e.Booking).WithMany();
                b.HasOne(e => e.Flight).WithMany();
                b.HasOne(e => e.Seat).WithMany();
            });
        }
    }
}
