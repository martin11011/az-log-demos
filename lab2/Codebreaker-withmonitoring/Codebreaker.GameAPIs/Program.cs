using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeyedSingleton("Codebreaker.GameAPIs", (services, _) => new ActivitySource("Codebreaker.GameAPIs", "1.0.0"));

builder.AddServiceDefaults();

// Swagger/EndpointDocumentation
builder.Services.AddEndpointsApiExplorer();

// Application Services

builder.AddApplicationServices();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v3/swagger.json", "v3");
});

await app.CreateOrUpdateDatabaseAsync();

app.MapGameEndpoints();

app.Run();
