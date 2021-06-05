using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrendByPivotPointsStrategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendByPivotPoints.Tests
{
    [TestClass()]
    public class LocalMoneyManagerTests
    {
        AccountFake account;
        LocalMoneyManager localMoneyManager;
        GlobalMoneyManagerReal globalMoneyManager;
        
        [TestInitialize]
        public void TestInitialize()
        {
            account = new AccountFake();            
            account.GObying = 4500;
            account.GOselling = 4000;
            account.Rate = 50;
            account.FreeBalance = 50000;
            account.Deposit = 1000000;            
            globalMoneyManager = new GlobalMoneyManagerReal(account, riskValuePrcnt: 5);            
            //globalMoneyManager.Money = 50000;
        }

        #region Testing Ruble

        #region Testing long position
        [TestMethod()]
        public void GetQntContractsTest_enterPriceGreaterStopPrice_positionLong_notEnoughGO_currencyRuble_returned11()
        {
            //arrange            
            var enterPrice = 70000;
            var stopPrice = 69000;
            var position = Position.Long;
            var currency = Currency.Ruble;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 11;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceGreaterStopPrice_positionLong_EnoughGO_currencyRuble_returned50()
        {
            //arrange            
            var enterPrice = 70000;
            var stopPrice = 60000;
            var position = Position.Long;
            var currency = Currency.Ruble;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 5;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionLong_notEnoughGO_currencyRuble_returned0()
        {
            //arrange            
            var enterPrice = 70000;
            var stopPrice = 71000;
            var position = Position.Long;
            var currency = Currency.Ruble;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 0;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionLong_EnoughGO_currencyRuble_returned0()
        {
            //arrange            
            var enterPrice = 70000;
            var stopPrice = 80000;
            var position = Position.Long;
            var currency = Currency.Ruble;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 0;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
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
            var position = Position.Short;
            var currency = Currency.Ruble;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 0;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceGreaterStopPrice_positionShort_EnoughGO_currencyRuble_returned0()
        {
            //arrange            
            var enterPrice = 70000;
            var stopPrice = 60000;
            var position = Position.Short;
            var currency = Currency.Ruble;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 0;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionLong_notEnoughGO_currencyRuble_returned12()
        {
            //arrange            
            var enterPrice = 70000;
            var stopPrice = 71000;
            var position = Position.Short;
            var currency = Currency.Ruble;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 12;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionShort_EnoughGO_currencyRuble_returned0()
        {
            //arrange            
            var enterPrice = 70000;
            var stopPrice = 80000;
            var position = Position.Short;
            var currency = Currency.Ruble;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 5;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
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
            var position = Position.Long;
            var currency = Currency.USD;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 11;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceGreaterStopPrice_positionLong_EnoughGO_currencyUSD_returned10()
        {
            //arrange            
            var enterPrice = 1100;
            var stopPrice = 1000;
            var position = Position.Long;
            var currency = Currency.USD;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 10;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionLong_notEnoughGO_currencyUSD_returned0()
        {
            //arrange            
            var enterPrice = 1100;
            var stopPrice = 1110;
            var position = Position.Long;
            var currency = Currency.USD;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 0;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionLong_EnoughGO_currencyUSD_returned0()
        {
            //arrange            
            var enterPrice = 1100;
            var stopPrice = 1200;
            var position = Position.Long;
            var currency = Currency.USD;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 0;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
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
            var position = Position.Short;
            var currency = Currency.USD;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 0;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceGreaterStopPrice_positionShort_EnoughGO_currencyUSD_returned0()
        {
            //arrange            
            var enterPrice = 1100;
            var stopPrice = 1000;
            var position = Position.Short;
            var currency = Currency.USD;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 0;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionLong_notEnoughGO_currencyUSD_returned12()
        {
            //arrange            
            var enterPrice = 1100;
            var stopPrice = 1110;
            var position = Position.Short;
            var currency = Currency.USD;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 12;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void GetQntContractsTest_enterPriceLessStopPrice_positionShort_EnoughGO_currencyUSD_returned0()
        {
            //arrange            
            var enterPrice = 1100;
            var stopPrice = 1200;
            var position = Position.Short;
            var currency = Currency.USD;
            localMoneyManager = new LocalMoneyManager(globalMoneyManager, account, currency);

            var expected = 10;

            //act
            var actual = localMoneyManager.GetQntContracts(enterPrice, stopPrice, position);
            //assert
            Assert.AreEqual(expected, actual);
        }
        #endregion
        #endregion
    }
}