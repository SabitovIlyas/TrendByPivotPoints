using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OxyPlot;
using OxyPlot.Series;

namespace CandlesGraphicsWinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // Создаем объект для графика
            var plotModel = new PlotModel { Title = "Японские свечи" };

            // Данные для построения графика
            var data = new List<CandleStickData>
            {
                new CandleStickData(new DateTime(2024,01,01,09,30,00),  5425,  5500,  5400,  5475,  10000),
                new CandleStickData(new DateTime(2024,01,02,09,30,00),  5000,  5100,  4950,  5050,  12345),
                new CandleStickData(new DateTime(2024,01,03,09,31,00),  5060,  5150,  5040,  5130,  12567),
                new CandleStickData(new DateTime(2024,01,04,09,32,00),  5140,  5200,  5120,  5180,  12789),
                new CandleStickData(new DateTime(2024,01,05,09,33,00),  5190,  5250,  5170,  5230,  13012),
                new CandleStickData(new DateTime(2024,01,06,09,34,00),  5240,  5300,  5220,  5280,  13235),
                new CandleStickData(new DateTime(2024,01,07,09,35,00),  5290,  5350,  5270,  5330,  13458),
                new CandleStickData(new DateTime(2024,01,08,09,36,00),  5340,  5400,  5320,  5380,  13681),
                new CandleStickData(new DateTime(2024,01,09,09,37,00),  5390,  5450,  5370,  5430,  13904),
                new CandleStickData(new DateTime(2024,01,10,09,38,00),  5440,  5500,  5420,  5480,  14128),
                new CandleStickData(new DateTime(2024,01,11,09,39,00),  5490,  5550,  5470,  5530,  14351),
                new CandleStickData(new DateTime(2024,01,12,09,40,00),  5540,  5600,  5520,  5580,  14574),
                new CandleStickData(new DateTime(2024,01,13,09,41,00),  5590,  5650,  5570,  5630,  14797),
                new CandleStickData(new DateTime(2024,01,14,09,42,00),  5640,  5700,  5620,  5680,  15020),
                new CandleStickData(new DateTime(2024,01,15,09,43,00),  5690,  5750,  5670,  5730,  15244),
                new CandleStickData(new DateTime(2024,01,16,09,44,00),  5740,  5800,  5720,  5780,  15467),
                new CandleStickData(new DateTime(2024,01,17,09,45,00),  5790,  5850,  5770,  5830,  15690),
                new CandleStickData(new DateTime(2024,01,18,09,46,00),  5840,  5900,  5820,  5880,  15913),
                new CandleStickData(new DateTime(2024,01,19,09,47,00),  5890,  5950,  5870,  5930,  16136),
                new CandleStickData(new DateTime(2024,01,20,09,48,00),  5940,  6000,  5920,  5980,  16360),
                new CandleStickData(new DateTime(2024,01,21,09,49,00),  5990,  6050,  5970,  6030,  16583),
                new CandleStickData(new DateTime(2024,01,22,09,50,00),  6040,  6100,  6020,  6080,  16806),
                new CandleStickData(new DateTime(2024,01,23,09,51,00),  6090,  6150,  6070,  6130,  17029),
                new CandleStickData(new DateTime(2024,01,24,09,52,00),  6140,  6200,  6120,  6180,  17253),
                new CandleStickData(new DateTime(2024,01,25,09,53,00),  6190,  6250,  6170,  6230,  17476),
                new CandleStickData(new DateTime(2024,01,26,09,54,00),  6240,  6300,  6220,  6280,  17699),
                new CandleStickData(new DateTime(2024,01,27,09,55,00),  6290,  6350,  6270,  6330,  17922),
                new CandleStickData(new DateTime(2024,01,28,09,56,00),  6340,  6400,  6320,  6380,  18146),
                new CandleStickData(new DateTime(2024,01,29,09,57,00),  6390,  6450,  6370,  6430,  18369),
                new CandleStickData(new DateTime(2024,01,30,09,58,00),  6440,  6500,  6420,  6480,  18592),
                new CandleStickData(new DateTime(2024,01,31,09,59,00),  6490,  6550,  6470,  6530,  18815),
                new CandleStickData(new DateTime(2024,02,01,10,00,00),  6540,  6600,  6520,  6580,  19039),
                new CandleStickData(new DateTime(2024,02,02,10,01,00),  6570,  6590,  6510,  6530,  19262),
                new CandleStickData(new DateTime(2024,02,03,10,02,00),  6520,  6560,  6460,  6480,  19485),
                new CandleStickData(new DateTime(2024,02,04,10,03,00),  6470,  6510,  6410,  6430,  19708),
                new CandleStickData(new DateTime(2024,02,05,10,04,00),  6420,  6460,  6360,  6380,  19932),
                new CandleStickData(new DateTime(2024,02,06,10,05,00),  6370,  6410,  6310,  6330,  20155),
                new CandleStickData(new DateTime(2024,02,07,10,06,00),  6320,  6360,  6260,  6280,  20378),
                new CandleStickData(new DateTime(2024,02,08,10,07,00),  6270,  6310,  6210,  6230,  20601),
                new CandleStickData(new DateTime(2024,02,09,10,08,00),  6220,  6260,  6160,  6180,  20825),
                new CandleStickData(new DateTime(2024,02,10,10,09,00),  6170,  6210,  6110,  6130,  21048),
                new CandleStickData(new DateTime(2024,02,11,10,10,00),  6120,  6160,  6060,  6080,  21271),
                new CandleStickData(new DateTime(2024,02,12,10,11,00),  6070,  6110,  6010,  6030,  21494),
                new CandleStickData(new DateTime(2024,02,13,10,12,00),  6020,  6060,  5960,  5980,  21718),
                new CandleStickData(new DateTime(2024,02,14,10,13,00),  5970,  6010,  5910,  5930,  21941),
                new CandleStickData(new DateTime(2024,02,15,10,14,00),  5920,  5960,  5860,  5880,  22164),
                new CandleStickData(new DateTime(2024,02,16,10,15,00),  5870,  5910,  5810,  5830,  22387),
                new CandleStickData(new DateTime(2024,02,17,10,16,00),  5820,  5860,  5760,  5780,  22611),
                new CandleStickData(new DateTime(2024,02,18,10,17,00),  5770,  5810,  5710,  5730,  22834),
                new CandleStickData(new DateTime(2024,02,19,10,18,00),  5720,  5760,  5660,  5680,  23057),
                new CandleStickData(new DateTime(2024,02,20,10,19,00),  5670,  5710,  5610,  5630,  23280),
                new CandleStickData(new DateTime(2024,02,21,10,20,00),  5620,  5660,  5560,  5580,  23504),
                new CandleStickData(new DateTime(2024,02,22,10,21,00),  5570,  5610,  5510,  5530,  23727),
                new CandleStickData(new DateTime(2024,02,23,10,22,00),  5520,  5560,  5460,  5480,  23950),
                new CandleStickData(new DateTime(2024,02,24,10,23,00),  5470,  5510,  5410,  5430,  24173),
                new CandleStickData(new DateTime(2024,02,25,10,24,00),  5420,  5460,  5360,  5380,  24397),
                new CandleStickData(new DateTime(2024,02,26,10,25,00),  5370,  5410,  5310,  5330,  24620),
                new CandleStickData(new DateTime(2024,02,27,10,26,00),  5320,  5360,  5260,  5280,  24843),
                new CandleStickData(new DateTime(2024,02,28,10,27,00),  5270,  5310,  5210,  5230,  25066),
                new CandleStickData(new DateTime(2024,02,29,10,28,00),  5220,  5260,  5160,  5180,  25290),
                new CandleStickData(new DateTime(2024,03,01,10,29,00),  5180,  5240,  5140,  5220,  25513),
                new CandleStickData(new DateTime(2024,03,02,10,30,00),  5230,  5290,  5190,  5270,  25736),
                new CandleStickData(new DateTime(2024,03,03,10,31,00),  5280,  5340,  5240,  5320,  25959),
                new CandleStickData(new DateTime(2024,03,04,10,32,00),  5330,  5390,  5290,  5370,  26183),
                new CandleStickData(new DateTime(2024,03,05,10,33,00),  5380,  5440,  5340,  5420,  26406),
                new CandleStickData(new DateTime(2024,03,06,10,34,00),  5430,  5490,  5390,  5470,  26629),
                new CandleStickData(new DateTime(2024,03,07,10,35,00),  5480,  5540,  5440,  5520,  26852),
                new CandleStickData(new DateTime(2024,03,08,10,36,00),  5530,  5590,  5490,  5570,  27076),
                new CandleStickData(new DateTime(2024,03,09,10,37,00),  5580,  5640,  5540,  5620,  27299),
                new CandleStickData(new DateTime(2024,03,10,10,38,00),  5630,  5690,  5590,  5670,  27522),
                new CandleStickData(new DateTime(2024,03,11,10,39,00),  5680,  5740,  5640,  5720,  27745),
                new CandleStickData(new DateTime(2024,03,12,10,40,00),  5730,  5790,  5690,  5770,  27969),
                new CandleStickData(new DateTime(2024,03,13,10,41,00),  5780,  5840,  5740,  5820,  28192),
                new CandleStickData(new DateTime(2024,03,14,10,42,00),  5830,  5890,  5790,  5870,  28415),
                new CandleStickData(new DateTime(2024,03,15,10,43,00),  5880,  5940,  5840,  5920,  28638),
                new CandleStickData(new DateTime(2024,03,16,10,44,00),  5930,  5990,  5890,  5970,  28862),
                new CandleStickData(new DateTime(2024,03,17,10,45,00),  5980,  6040,  5940,  6020,  29085),
                new CandleStickData(new DateTime(2024,03,18,10,46,00),  6030,  6090,  5990,  6070,  29308),
                new CandleStickData(new DateTime(2024,03,19,10,47,00),  6080,  6140,  6040,  6120,  29531),
                new CandleStickData(new DateTime(2024,03,20,10,48,00),  6130,  6190,  6090,  6170,  29755),
                new CandleStickData(new DateTime(2024,03,21,10,49,00),  6180,  6240,  6140,  6220,  29978),
                new CandleStickData(new DateTime(2024,03,22,10,50,00),  6230,  6290,  6190,  6270,  30201),
                new CandleStickData(new DateTime(2024,03,23,10,51,00),  6280,  6340,  6240,  6320,  30424),
                new CandleStickData(new DateTime(2024,03,24,10,52,00),  6330,  6390,  6290,  6370,  30648),
                new CandleStickData(new DateTime(2024,03,25,10,53,00),  6380,  6440,  6340,  6420,  30871),
                new CandleStickData(new DateTime(2024,03,26,10,54,00),  6430,  6490,  6390,  6470,  31094),
                new CandleStickData(new DateTime(2024,03,27,10,55,00),  6480,  6540,  6440,  6520,  31317),
            };

            //var data = new List<CandleStickData>
            //{
            //    new CandleStickData(new DateTime(2025,11,01,10,00,00),90000,90000,90000,90000,1),
            //    new CandleStickData(new DateTime(2025,11,02,10,00,00),80000,80000,80000,80000,1),
            //    new CandleStickData(new DateTime(2025,11,03,10,00,00),81000,81000,81000,81000,1),
            //    new CandleStickData(new DateTime(2025,11,04,10,00,00),82000,82000,82000,82000,1),
            //    new CandleStickData(new DateTime(2025,11,05,10,00,00),83000,83000,83000,83000,1),
            //    new CandleStickData(new DateTime(2025,11,06,10,00,00),84000,84000,84000,84000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,06,00),85000,85000,85000,85000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,07,00),86000,86000,86000,86000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,08,00),87000,87000,87000,87000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,09,00),88000,88000,88000,88000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,10,00),89000,89000,89000,89000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,11,00),90000,90000,90000,90000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,12,00),91000,91000,91000,91000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,13,00),92000,92000,92000,92000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,14,00),93000,93000,93000,93000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,15,00),94000,94000,94000,94000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,16,00),95000,95000,95000,95000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,17,00),94000,94000,94000,94000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,18,00),93000,93000,93000,93000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,19,00),92000,92000,92000,92000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,20,00),91000,91000,91000,91000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,21,00),90000,90000,90000,90000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,22,00),89000,89000,89000,89000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,23,00),88000,88000,88000,88000,1),
            //    //new CandleStickData(new DateTime(2025,11,27,10,24,00),87000,87000,87000,87000,1)
            //};

            // Добавляем серии японских свечей
            var candleStickSeries = new CandleStickSeries
            {
                ItemsSource = data,
                DataFieldX = "Date",
                DataFieldOpen = "Open",
                DataFieldHigh = "High",
                DataFieldLow = "Low",
                DataFieldClose = "Close"
            };

            plotModel.Series.Add(candleStickSeries);

            // Устанавливаем модель графика на наш элемент управления
            var plotView = new OxyPlot.WindowsForms.PlotView
            {
                Model = plotModel,
                Dock = DockStyle.Fill
            };

            this.Controls.Add(plotView);
        }   

        private void Form1_Load(object sender, EventArgs e)
        {

        }

    }

    // Класс для хранения данных свечей
    public class CandleStickData
    {
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }

        public CandleStickData(DateTime date, double open, double high, double low, double close, double volume)
        {
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }
    }

}