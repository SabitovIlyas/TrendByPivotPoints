using Microsoft.VisualStudio.TestTools.UnitTesting;
using SabitovCapitalConsole;

namespace SabitovCapitalConsole.Tests
{
    [TestClass()]
    public class ConsoleInterfaceTests
    {
        UI testInterface;

        [TestInitialize()]
        public void Init()
        {
            Menu mainMenu = Menu.Create(name: nameof(AccountsMenu));
            Menu balanceHistoryMenu = Menu.Create(name: nameof(BalanceHistoryMenu));
            mainMenu.Link(selection: 0, menu: balanceHistoryMenu);
            testInterface = TestInterface.Create();
            testInterface.AddMenu(mainMenu);
            testInterface.AddMenu(balanceHistoryMenu);
        }

        [TestMethod()]
        public void WalkingThrowTheMenusTest()
        {   
            var actual = testInterface.GetCurrentMenuName();
            Assert.AreEqual(expected: nameof(AccountsMenu), actual);
            
            testInterface.Select(selectedMenuIndex: 0);
            actual = testInterface.GetCurrentMenuName();
            Assert.AreEqual(expected: nameof(BalanceHistoryMenu), actual);

            testInterface.GoHome();
            actual = testInterface.GetCurrentMenuName();
            Assert.AreEqual(expected: nameof(AccountsMenu), actual);
        }

        [TestMethod()]
        public void ShowContentTest()
        {
            var expected = string.Format("{0}\r\n\r\n0) BalanceHistoryMenu\r\n", 
                nameof(AccountsMenu));
            testInterface.Show();
            var actual = testInterface.Content;
            Assert.AreEqual(expected, actual);
        }
    }
}