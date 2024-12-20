using CodeBreaker.Bot.Endpoints;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CodeBreaker.Bot.Tests")]

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Swagger & EndpointDocumentation
builder.Services.AddEndpointsApiExplorer();

builder.AddApplicationServices();

var app = builder.Build();

app.UseSwagger();

app.MapDefaultEndpoints();
app.MapBotEndpoints();

app.Run();
