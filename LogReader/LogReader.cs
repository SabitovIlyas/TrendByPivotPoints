using System.Collections.Generic;
using System.Linq;

namespace LogPreparator
{
    public class LogReader
    {
        public List<int> TimeStampIndexes { get; private set; }
        public List<string> Lines { get; private set; }
        string log = string.Empty;

        public LogReader(string log)
        {
            this.log = log;
        }

        public void SplitLogOnLines()
        {
            FillTimeStampIndexes();
            Lines = new List<string>();

            for (var i = 0; i < TimeStampIndexes.Count-1; i++)
            {
                Lines.Add(log.Substring(TimeStampIndexes[i],
                    TimeStampIndexes[i + 1] - TimeStampIndexes[i]));
            }
            Lines.Add(log.Substring(TimeStampIndexes.Last(),
                    log.Length - TimeStampIndexes.Last()));
        }

        private void FillTimeStampIndexes()
        {
            TimeStampIndexes = new List<int>();
            var numberCounter = 0;
            var numberTwoDots = 0;
            var numberDots = 0;

            for (var i = 0; i < log.Length; i++)
            {
                if (IsNumber(log[i]))
                {
                    numberCounter++;
                    continue;
                }

                if ((numberCounter == 2 || numberCounter==4) && IsTwoDots(log[i]))
                { 
                    numberTwoDots++;
                    continue;
                }

                if (numberCounter == 6 && numberTwoDots == 2 && IsDot(log[i]))
                {
                    numberDots++;
                    continue;
                }

                if (numberCounter == 8 && numberTwoDots == 2 && numberDots == 1)
                {
                    var countSymbols = numberCounter + numberTwoDots + numberDots;
                    TimeStampIndexes.Add(i - countSymbols);                    
                }

                numberCounter = 0;
                numberTwoDots = 0;
                numberDots = 0;
            }
        }

        private bool IsNumber(char symbol)
        {
            return symbol == '0' ||
                symbol == '1' ||
                symbol == '2' ||
                symbol == '3' ||
                symbol == '4' ||
                symbol == '5' ||
                symbol == '6' ||
                symbol == '7' ||
                symbol == '8' ||
                symbol == '9';
        }

        private bool IsTwoDots(char symbol)
        {
            return symbol == ':';                
        }

        private bool IsDot(char symbol)
        {
            return symbol == '.';
        }

        public void UseSubstringFilterIncluded(string[] filters)
        {
            var filteredLines = new List<string>();
            foreach (var line in Lines)
                for (var i = 0; i < filters.Length; i++)
                    if (line.Contains(filters[i]))
                    { 
                        filteredLines.Add(line);
                        break;
                    }

            Lines = filteredLines;
        }

        public void UseSubstringFilterExcluded(string[] filters)
        {
            var filteredLines = new List<string>();
            foreach (var line in Lines)
            {
                var flag = true;

                for (var i = 0; i < filters.Length; i++)                
                    if (line.Contains(filters[i]))
                    {
                        flag = false;
                        break; 
                    }
                
                if (flag)
                    filteredLines.Add(line);
            }

            Lines = filteredLines;
        }
    }
}