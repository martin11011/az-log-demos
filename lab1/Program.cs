using logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
// See https://aka.ms/new-console-template for more information


var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddTransient<Calculator>();
builder.Services.AddLogging(configure => configure.AddJsonConsole(c => c.JsonWriterOptions = new()
{
    Indented = true
}));
// in memory db
builder.Services.AddDbContext<CalculationContext>(options => options.UseInMemoryDatabase("Calculation"));

using var host = builder.Build();

var calculator = host.Services.GetRequiredService<Calculator>();

for (int i = 0; i < 10; i++)
{
    await calculator.Add(i, i + 1);
    await Task.Delay(1000);
}
