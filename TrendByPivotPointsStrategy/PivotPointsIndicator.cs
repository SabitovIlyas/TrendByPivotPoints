﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using TSLab.DataSource;

namespace TrendByPivotPointsStrategy
{
    public class PivotPointsIndicator
    {
        public List<Indicator> GetLows(List<Bar> bars, int leftLocal, int rightLocal)
        {
            var result = new List<Indicator>();            

            for (var i = leftLocal; i<bars.Count - rightLocal ;i++)
            {
                var low = true;
                for(var j = i-leftLocal; j<i;j++)
                {
                    if (bars[i].Low>=bars[j].Low)
                    {
                        low = false;
                        break;
                    }
                }

                if (low == true)
                {
                    for (var j = i + 1; j <= i + rightLocal; j++)
                    {
                        if (bars[j].Low < bars[i].Low)
                        {
                            low = false;
                            break;
                        }                        
                    }
                }

                if (low == true)
                    result.Add(new Indicator() { BarNumber = i, Value = bars[i].Low });
            }

            return result;
        }

        public List<Indicator> GetHighs(List<Bar> bars, int leftLocal, int rightLocal)
        {
            var result = new List<Indicator>();

            for (var i = leftLocal; i < bars.Count - rightLocal; i++)
            {
                var high = true;
                for (var j = i - leftLocal; j < i; j++)
                {
                    if (bars[i].High <= bars[j].High)
                    {
                        high = false;
                        break;
                    }
                }

                if (high == true)
                {
                    for (var j = i + 1; j <= i + rightLocal; j++)
                    {
                        if (bars[j].High > bars[i].High)
                        {
                            high = false;
                            break;
                        }
                    }
                }

                if (high == true)
                    result.Add(new Indicator() { BarNumber = i, Value = bars[i].High });
            }

            return result;
        }
    }
}
