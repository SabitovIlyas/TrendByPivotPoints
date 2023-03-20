using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace LogPreparator.Tests
{
    [TestClass()]
    public class LogReaderTests
    {
        [TestInitialize()]
        public void Init()
        {
            
        }

        [TestMethod()]
        public void SplitLogOnLinesTest()
        {
            var expected = new List<string>() 
            {
                "00:48:03.75[179]DEBUG:Lab msg:Info:TradingSystemBollingerBands/Period Bollinger Band: 10; Standart " +
                "Deviation Coefficient: 2; Profit Percent/NGH3NGJ3:FUT/Short/: SetLimitOrdersForChangePosition: " +
                "Устанавливаем лимитный ордер для изменения позиции \r\n",
                "00:48:03.75[198]DEBUG:Lab msg:Info:TradingSystemBollingerBands/Period Bollinger Band: 20; " +
                "Standart Deviation Coefficient: 2; Profit Percent/NGH3NGJ3:FUT/Long/: GetPosition: Позиция найдена." +
                " Состояние позиции:\r\nEntryOrders: TSLab.ScriptExecution.Realtime.RealtimePosition+" +
                "<get_EntryOrders>d__21\r\nExitOrders: TSLab.ScriptExecution.Realtime.RealtimePosition+" +
                "<get_ExitOrders>d__23\r\nIsVirtual: False\r\nIsVirtualClosed: True\r\nBarsHeld: 6673\r\n" +
                "PositionState: HaveCloseSignal\r\nEntryOrderType: Limit\r\nExitOrderType: Market\r\n" +
                "EntrySlippage: 0\r\nExitSlippage: \r\nParentList: TSLab.ScriptExecution.Realtime." +
                "RealtimePositionList\r\nSecurity: NGH3NGJ3:FUT\r\nChangeInfos: System.Collections.Generic." +
                "List`1[TSLab.Script.IPositionInfo]\r\nIsAddedVirtual: False\r\nEntrySignalName: LE Вход №1\r\n" +
                "EntryNotes: \r\nExitSignalName: LE Вход №1\r\nExitNotes: \r\nChangeSignalName: LE Вход №1\r\n" +
                "ChangeNotes: \r\nIsLong: True\r\nIsShort: False\r\nIsActive: True\r\nEntryPrice: 0.253\r\n" +
                "AverageEntryPrice: 0.24220266666666668\r\nExitPrice: 0\r\nShares: 375\r\nSignedShares: 375\r\n" +
                "SharesChange: 0\r\nNormalizedShares: 375\r\nSharesOrigin: 1\r\nMaxShares: 375\r\nEntryBarNum: 2384" +
                "\r\nExitBarNum: 9057\r\nEntryBar: 02/27/2023 11:51:00/0.253/0.252/0.254/0.254/52\r\nExitBar: " +
                "03/10/2023 17:47:00/0.241/0.241/0.241/0.241/51\r\nEntryCommission: 0\r\nExitCommission: 0\r\n" +
                "ProfitPerTrade: \r\nFullEntryCommission: 0\r\nFullExitCommission: 0\r\nEntryExecution: Normal\r\n" +
                "ExitExecution: \r\n \r\n",
                "00:48:03.75[179]DEBUG:TradingSystemBollingerBands/Period Bollinger Band: 10; Standart Deviation " +
                "Coefficient: 2; Profit Percent/NGH3NGJ3:FUT/Short/: GetLotsForChangePositionBasedOnOpenedLots: " +
                "Получаем новый объём позиции, основанный на уже открытом объёме \r\n",
                "00:48:03.75[198]DEBUG:TradingSystemBollingerBands/Period Bollinger Band: 20; Standart Deviation " +
                "Coefficient: 2; Profit Percent/NGH3NGJ3:FUT/Long/: " +
                "UpdateFlagIsPriceCrossedEmaAfterOpenOrChangePosition: Обновляю флаг \" Пересечение цены EMA после " +
                "открытия или изменения позиции\". Текущее состояние флага: True \r\n",
                "00:48:03.75[179]INFO:TradingSystemBollingerBands/Period Bollinger Band: 10; Standart Deviation " +
                "Coefficient: 2; Profit Percent/NGH3NGJ3:FUT/Short/: GetLotsForChangePositionBasedOnOpenedLots: " +
                "Получаем новый объём позиции, основанный на уже открытом объёме"
            };
            var dataStorage = DataStorage.Create("LogReaderTest1.log");
            var log = dataStorage.ReadFile();
            var reader = new LogReader(log);

            reader.SplitLogOnLines();
            var actual = reader.Lines;

            if (expected.Count != actual.Count)
                Assert.Fail();

            for (var i = 0; i < expected.Count; i++)
                if (expected[i] != actual[i])
                    Assert.Fail();
        }

        [TestMethod()]
        public void UseSubstringFilterIncludedTest()
        {
            var expected = new List<string>()
            {                
                "00:48:03.75[198]DEBUG:Lab msg:Info:TradingSystemBollingerBands/Period Bollinger Band: 20; " +
                "Standart Deviation Coefficient: 2; Profit Percent/NGH3NGJ3:FUT/Long/: GetPosition: Позиция найдена." +
                " Состояние позиции:\r\nEntryOrders: TSLab.ScriptExecution.Realtime.RealtimePosition+" +
                "<get_EntryOrders>d__21\r\nExitOrders: TSLab.ScriptExecution.Realtime.RealtimePosition+" +
                "<get_ExitOrders>d__23\r\nIsVirtual: False\r\nIsVirtualClosed: True\r\nBarsHeld: 6673\r\n" +
                "PositionState: HaveCloseSignal\r\nEntryOrderType: Limit\r\nExitOrderType: Market\r\n" +
                "EntrySlippage: 0\r\nExitSlippage: \r\nParentList: TSLab.ScriptExecution.Realtime." +
                "RealtimePositionList\r\nSecurity: NGH3NGJ3:FUT\r\nChangeInfos: System.Collections.Generic." +
                "List`1[TSLab.Script.IPositionInfo]\r\nIsAddedVirtual: False\r\nEntrySignalName: LE Вход №1\r\n" +
                "EntryNotes: \r\nExitSignalName: LE Вход №1\r\nExitNotes: \r\nChangeSignalName: LE Вход №1\r\n" +
                "ChangeNotes: \r\nIsLong: True\r\nIsShort: False\r\nIsActive: True\r\nEntryPrice: 0.253\r\n" +
                "AverageEntryPrice: 0.24220266666666668\r\nExitPrice: 0\r\nShares: 375\r\nSignedShares: 375\r\n" +
                "SharesChange: 0\r\nNormalizedShares: 375\r\nSharesOrigin: 1\r\nMaxShares: 375\r\nEntryBarNum: 2384" +
                "\r\nExitBarNum: 9057\r\nEntryBar: 02/27/2023 11:51:00/0.253/0.252/0.254/0.254/52\r\nExitBar: " +
                "03/10/2023 17:47:00/0.241/0.241/0.241/0.241/51\r\nEntryCommission: 0\r\nExitCommission: 0\r\n" +
                "ProfitPerTrade: \r\nFullEntryCommission: 0\r\nFullExitCommission: 0\r\nEntryExecution: Normal\r\n" +
                "ExitExecution: \r\n \r\n",                
                "00:48:03.75[198]DEBUG:TradingSystemBollingerBands/Period Bollinger Band: 20; Standart Deviation " +
                "Coefficient: 2; Profit Percent/NGH3NGJ3:FUT/Long/: " +
                "UpdateFlagIsPriceCrossedEmaAfterOpenOrChangePosition: Обновляю флаг \" Пересечение цены EMA после " +
                "открытия или изменения позиции\". Текущее состояние флага: True \r\n",                
            };
            var dataStorage = DataStorage.Create("LogReaderTest1.log");
            var filter = new string[1]{"TradingSystemBollingerBands/Period Bollinger Band: 20; Standart Deviation Coefficient: 2; " +
                "Profit Percent/NGH3NGJ3:FUT/Long/" };
            var log = dataStorage.ReadFile();
            var reader = new LogReader(log);

            reader.SplitLogOnLines();
            reader.UseSubstringFilterIncluded(filter);
            var actual = reader.Lines;

            if (expected.Count != actual.Count)
                Assert.Fail();

            for (var i = 0; i < expected.Count; i++)
                if (expected[i] != actual[i])
                    Assert.Fail();
        }


        [TestMethod()]        
        public void UseSubstringFilterExcludedTest()
        {
            var expected = new List<string>()
            {                
                "00:48:03.75[198]DEBUG:TradingSystemBollingerBands/Period Bollinger Band: 20; Standart Deviation " +
                "Coefficient: 2; Profit Percent/NGH3NGJ3:FUT/Long/: " +
                "UpdateFlagIsPriceCrossedEmaAfterOpenOrChangePosition: Обновляю флаг \" Пересечение цены EMA после " +
                "открытия или изменения позиции\". Текущее состояние флага: True \r\n",
            };
            var dataStorage = DataStorage.Create("LogReaderTest1.log");
            var filter1 = new string[1]{ "TradingSystemBollingerBands/Period Bollinger Band: 20; Standart Deviation Coefficient: 2; " +
                "Profit Percent/NGH3NGJ3:FUT/Long/" };
            var filter2 = new string[1] { "Lab msg" };
            var log = dataStorage.ReadFile();
            var reader = new LogReader(log);

            reader.SplitLogOnLines();
            reader.UseSubstringFilterIncluded(filter1);
            reader.UseSubstringFilterExcluded(filter2);
            var actual = reader.Lines;

            if (expected.Count != actual.Count)
                Assert.Fail();

            for (var i = 0; i < expected.Count; i++)
                if (expected[i] != actual[i])
                    Assert.Fail();
        }
    }
}