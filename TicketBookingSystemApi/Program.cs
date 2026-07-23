using Microsoft.EntityFrameworkCore;
using TicketBookingSystemApi.Data;
using TicketBookingSystemApi.Interfaces;
using TicketBookingSystemApi.Repositories;
using TicketBookingSystemApi.Services;

var builder = WebApplication.CreateBuilder(args);

const string FrontendCorsPolicy = "FrontendCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:4173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();

//swagger gen 
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddControllers();
builder.Services.AddScoped<IEventRepository,EventRepository>();
builder.Services.AddScoped<IEventService,EventService>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ITicketReservationService, TicketReservationService>();
builder.Services.AddScoped<ITicketPurchaseService, TicketPurchaseService>();

builder.Services.AddDbContext<TicketBookingDataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Tickets")
        ?? "Server=localhost,1433;Database=TicketBooking;User Id=sa;Password=Dev_Only_Pa55word!;TrustServerCertificate=True;")
);

var app = builder.Build();

// Configure the HTTP request pipeline. 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(FrontendCorsPolicy);
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TicketBookingDataContext>();
    await db.Database.EnsureCreatedAsync();
    await DbSeeder.SeedAsync(db);
}

app.Run();
