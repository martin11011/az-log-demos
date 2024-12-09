using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lab1;
using Microsoft.Extensions.Logging;

namespace logging
{
    public class Calculator(ILogger<Calculator> logger, CalculationContext context)
    {
        public async Task<int> Add(int a, int b)
        {
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

            return result;
        }
    }
}