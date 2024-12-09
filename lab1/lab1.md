# Lab 1

With all the labs, you have write access to the repo and can add your own code to the _labs_ folder. Use a subdirectory with your name to avoid conflicts.

## Logging

- Add Microsoft.Extensions.Hosting to use a dependency injection container
- Create a console application to write log data using the `ILogger` and `ILoggerFactory` interfaces.
- Add strongly typed logging using the `LoggerMessage` attribute.
- Add EF Core to see these logs
- Configure the logs using `appsettings.json`
- Add a JSON Console log provider and customize the log output to indented

## Metrics

- Add custom metrics information using the `Meter` class
- Add counters and a Histogram
- Monitor the custom counts and built-in counts using `dotnet counters`

## Distributed Tracing

- Add an `ActivitiySource`, and start and stop activities
- Add baggage and tag information to activities
- Fire events with activities
- Add an `ActivitiyListener` to watch activities
