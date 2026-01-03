using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDocView(options =>
{
    options.DefaultSchema = "docviewdb";
    options.DefaultTagName = "docview";
    options.LoadEntityTypesByDocvTableAttribute(new List<Assembly>
    {
        typeof(Program).Assembly
    });

    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    var files = dir.GetFiles("*.xml");
    options.AddXmlDocument(files.Select(s => s.FullName));
});


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.UseDocViewUI();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
