using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using lab1;
using Microsoft.EntityFrameworkCore;

namespace logging
{
    public class CalculationContext(DbContextOptions<CalculationContext> options) : DbContext(options)
    {
        public required DbSet<Calculation> Calculations { get; set; }
    }
}