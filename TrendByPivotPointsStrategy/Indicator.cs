using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingSystems
{
    public class Indicator
    {
        public int BarNumber { get { return barNumber; } set { barNumber = value; } }
        public double Value { get { return value; } set { this.value = value; } }

        private int barNumber;
        private double value;

        public Indicator()
        {         
        }

        public Indicator(int barNumber, double value)
        {
            this.barNumber = barNumber;
            this.value = value;
        }
    }
}
