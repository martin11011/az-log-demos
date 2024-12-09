using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lab1
{
    public class Calculation
    {
        public Guid Id { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public int Result { get; set; }
        public string Operation { get; set; } = string.Empty;
    }
}