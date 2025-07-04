using Microsoft.EntityFrameworkCore;
using SaccoService.Core.Interfaces;
using SaccoService.Core.Mappings;
using SaccoService.Infrastructure.Data;
using SaccoService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(SaccoProfile));

// Add Entity Framework
builder.Services.AddDbContext<SaccoDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=sacco.db"));

// Add repositories
builder.Services.AddScoped<ISaccoRepository, SaccoRepository>();
builder.Services.AddScoped<ISaccoMemberRepository, SaccoMemberRepository>();
builder.Services.AddScoped<ISaccoCommitteeRepository, SaccoCommitteeRepository>();
builder.Services.AddScoped<ISaccoMeetingRepository, SaccoMeetingRepository>();
builder.Services.AddScoped<ISaccoFinancialRepository, SaccoFinancialRepository>();
builder.Services.AddScoped<ISaccoShareRepository, SaccoShareRepository>();
builder.Services.AddScoped<ISaccoLoanRepository, SaccoLoanRepository>();

// Add services
builder.Services.AddScoped<ISaccoService, SaccoService.Core.Services.SaccoService>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SaccoDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
