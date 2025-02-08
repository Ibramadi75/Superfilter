using System.Linq.Expressions;
using Database;
using Database.Models;
using Serilog;
using Microsoft.OpenApi.Models;
using SuperFilter;
using Dto;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.MapGet("/user", () => "Hello World!");

app.MapPost("/ComplexFilter", (FilteringDto filteringDto) =>
{
    Fake fake = new Fake();
    var query = fake.GetUsers();
    
    var propertiesToSortAndFilter = new Dictionary<string, Expression<Func<User, object>>>
    {
        { nameof(User.Id), x => x.Id! },
        { nameof(User.Name), x => x.Name! },
        { nameof(User.MoneyAmount), x => x.MoneyAmount! }
    };

    //query = query.SortProperties(filters, propertiesToSortAndFilter);

    // query = query.FilterProperty(filteringDto, x => x.Id);
    //foreach (var expression in propertiesToSortAndFilter)
    //    query = query.FilterProperty(filteringDto, x => expression);

    return query;
});

app.Run();