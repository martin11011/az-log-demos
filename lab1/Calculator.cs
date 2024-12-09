using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lab1;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics;


namespace logging
{
    public class Calculator
    {
        private ILogger<Calculator> logger;
        private CalculationContext context;
        public static readonly ActivitySource ActivitySource = new("DiagnosticsSample", "1.0");

        private static readonly Meter s_meter = new("DiagnosticsSample", "1.0.0");
        private static int s_observableCalculationValue = 0;
        private static readonly ObservableCounter<int> s_observableCalculationsCounter
            = s_meter.CreateObservableCounter("calculations_counter", () => s_observableCalculationValue);
        private static readonly Histogram<decimal> s_calculationDuration = s_meter.CreateHistogram<decimal>("calculation_duration", "Histogram of calculation durations");
        private static readonly Counter<int> s_calculationErrors = s_meter.CreateCounter<int>("calculation_errors", "Number of calculation errors");
        private static readonly Counter<int> s_calculationCount = s_meter.CreateCounter<int>("calculation_count", "Number of calculations");
        private readonly ActivityListener _listener;

        public Calculator(ILogger<Calculator> logger, CalculationContext context)
        {
            this.logger = logger;
            this.context = context;
            _listener = new ActivityListener
            {
                ShouldListenTo = activitySource => activitySource.Name == "DiagnosticsSample",
                Sample = (ref ActivityCreationOptions<ActivityContext> options) => ActivitySamplingResult.AllData,
                SampleUsingParentId = (ref ActivityCreationOptions<string> options) => ActivitySamplingResult.AllData,
                ActivityStarted = activity =>
                {
                    logger.LogInformation("Activity started: {ActivityName}", activity.DisplayName);
                },
                ActivityStopped = activity =>
                {
                    logger.LogInformation("Activity stopped: {ActivityName}", activity.DisplayName);
                }
            };

            ActivitySource.AddActivityListener(_listener);
        }


        public async Task<int> Add(int a, int b)
        {
            using var calculationActivity = ActivitySource.StartActivity("Addition", ActivityKind.Internal);
            calculationActivity?.AddBaggage("a", a.ToString());
            calculationActivity?.AddBaggage("b", b.ToString());
            calculationActivity?.SetTag("operation", "addition");
            calculationActivity?.AddEvent(new ActivityEvent("Adding two numbers"));

            s_calculationCount.Add(1);
            s_observableCalculationValue++;
            var sw = Stopwatch.StartNew();

            logger.CalculationStarted(a, b);

            var result = a + b;

            logger.LogInformation("Added {a} and {b}, result: {result}", a, b, result);

            context.Calculations.Add(new Calculation
            {
                Id = Guid.NewGuid(),
                A = a,
                B = b,
                Result = result,
                Operation = "Add"
            });
            await context.SaveChangesAsync();

            sw.Stop();
            s_calculationDuration.Record(sw.ElapsedMilliseconds);

            return result;
        }
    }
}