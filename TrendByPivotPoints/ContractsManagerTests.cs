using Microsoft.VisualStudio.TestTools.UnitTesting;
using TradingSystems;

namespace TrendByPivotPoints.Tests
{
    [TestClass()]
    public class ContractsManagerTests
    {
        AccountFake account;
        ContractsManager contractsManager;
        RiskManagerReal riskManager;
        Logger logger = new NullLogger();
        Currency baseCurrency = Currency.Ruble;
        CurrencyConverter currencyConverter;

        [TestInitialize]
        public void TestInitialize()
        {
            var currencyBalance = 50000;
            var estimatedBalance = 1000000;
            var security = new SecurityLab(currency: Currency.Ruble, shares: 1);
            account = new AccountFake(initDeposit: estimatedBalance, baseCurrency: Currency.Ruble, logger: logger);

            currencyConverter = new CurrencyConverter(baseCurrency);
            account.GObying = 4500;
            account.GOselling = 4000;
            currencyConverter.AddCurrencyRate(Currency.USD, rate: 50);
           
            account.FreeBalance = currencyBalance;            
            riskManager = new RiskManagerReal(account, logger, riskValuePrcnt: 5);
        }

        #region Testing Ruble

        #region Testing long position
        [TestMethod()]
        public void GetQntContractsTest_enterPriceGreaterStopPrice_positionLong_notEnoughGO_currencyRuble_returned11()
        {
            //arrange            
            var enterPrice = 70000;
            var stopPrice = 69000;
            var position = PositionSide.Long;
            var currency = Currency.Ruble;

            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);
            
            var expected = 11;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceGreaterStopPrice_positionLong_EnoughGO_currencyRuble_returned50()
        {
            //arrange            
            var enterPrice = 70000;
            var stopPrice = 60000;
            var position = PositionSide.Long;
            var currency = Currency.Ruble;
            
            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);
            
            var expected = 5;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionLong_notEnoughGO_currencyRuble_returned0()
        {
            //arrange            
            var enterPrice = 70000;
            var stopPrice = 71000;
            var position = PositionSide.Long;
            var currency = Currency.Ruble;

            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);            
            
            var expected = 0;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionLong_EnoughGO_currencyRuble_returned0()
        {
            //arrange            
            var enterPrice = 70000;
            var stopPrice = 80000;
            var position = PositionSide.Long;
            var currency = Currency.Ruble;

            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);
            
            var expected = 0;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region Testing short position

        [TestMethod()]
        public void GetQntContractsTest_enterPriceGreaterStopPrice_positionShort_notEnoughGO_currencyRuble_returned0()
        {
            //arrange            
            var enterPrice = 70000;
            var stopPrice = 69000;
            var position = PositionSide.Short;
            var currency = Currency.Ruble;

            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);
            
            var expected = 0;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceGreaterStopPrice_positionShort_EnoughGO_currencyRuble_returned0()
        {
            //arrange            
            var enterPrice = 70000;
            var stopPrice = 60000;
            var position = PositionSide.Short;
            var currency = Currency.Ruble;

            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);
            
            var expected = 0;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionLong_notEnoughGO_currencyRuble_returned12()
        {
            //arrange            
            var enterPrice = 70000;
            var stopPrice = 71000;
            var position = PositionSide.Short;
            var currency = Currency.Ruble;

            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);

            var expected = 12;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionShort_EnoughGO_currencyRuble_returned0()
        {
            //arrange            
            var enterPrice = 70000;
            var stopPrice = 80000;
            var position = PositionSide.Short;
            var currency = Currency.Ruble;

            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);

            var expected = 5;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }
        #endregion

        #endregion


        #region Testing USD

        #region Testing long position
        [TestMethod()]
        public void GetQntContractsTest_enterPriceGreaterStopPrice_positionLong_notEnoughGO_currencyUSD_returned11()
        {
            //arrange            
            var enterPrice = 1100;
            var stopPrice = 1090;
            var position = PositionSide.Long;
            var currency = Currency.USD;

            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);

            var expected = 11;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceGreaterStopPrice_positionLong_EnoughGO_currencyUSD_returned10()
        {
            //arrange            
            var enterPrice = 1100;
            var stopPrice = 1000;
            var position = PositionSide.Long;
            var currency = Currency.USD;

            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);

            var expected = 10;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionLong_notEnoughGO_currencyUSD_returned0()
        {
            //arrange            
            var enterPrice = 1100;
            var stopPrice = 1110;
            var position = PositionSide.Long;
            var currency = Currency.USD;

            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);

            var expected = 0;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionLong_EnoughGO_currencyUSD_returned0()
        {
            //arrange            
            var enterPrice = 1100;
            var stopPrice = 1200;
            var position = PositionSide.Long;
            var currency = Currency.USD;

            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);

            var expected = 0;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region Testing short position

        [TestMethod()]
        public void GetQntContractsTest_enterPriceGreaterStopPrice_positionShort_notEnoughGO_currencyUSD_returned0()
        {
            //arrange            
            var enterPrice = 1100;
            var stopPrice = 1090;
            var position = PositionSide.Short;
            var currency = Currency.USD;

            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);

            var expected = 0;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceGreaterStopPrice_positionShort_EnoughGO_currencyUSD_returned0()
        {
            //arrange            
            var enterPrice = 1100;
            var stopPrice = 1000;
            var position = PositionSide.Short;
            var currency = Currency.USD;

            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);

            var expected = 0;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionLong_notEnoughGO_currencyUSD_returned12()
        {
            //arrange            
            var enterPrice = 1100;
            var stopPrice = 1110;
            var position = PositionSide.Short;
            var currency = Currency.USD;

            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);

            var expected = 12;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionShort_EnoughGO_currencyUSD_returned0()
        {
            //arrange            
            var enterPrice = 1100;
            var stopPrice = 1200;
            var position = PositionSide.Short;
            var currency = Currency.USD;

            var security = new SecurityLab(currency, shares: 1, GObuying: 4500, GOselling: 4000);
            contractsManager = new ContractsManager(riskManager, account, currency, currencyConverter, logger);

            var expected = 10;

            //act
            var actual = contractsManager.GetQntContracts(security, enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }
        #endregion
        #endregion
    }
}