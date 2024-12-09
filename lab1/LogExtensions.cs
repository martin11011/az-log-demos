using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace logging
{
    public static partial class LogExtensions
    {
        [LoggerMessage(LogLevel.Information, "Adding {a} and {b}")]
        public static partial void CalculationStarted(this ILogger logger, int a, int b);

    }
}