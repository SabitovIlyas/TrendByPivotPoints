using Microsoft.VisualStudio.TestTools.UnitTesting;
using SabitovCapitalConsole;

namespace SabitovCapitalConsole.Tests
{
    [TestClass()]
    public class ConsoleInterfaceTests
    {
        [TestMethod()]
        public void RunTest()
        {
            Menu mainMenu = MainMenu.Create(name: nameof(MainMenu));
            Menu balanceHistoryMenu = BalanceHistoryMenu.Create(name: nameof(BalanceHistoryMenu));
            mainMenu.Link(selection: 0, menu: balanceHistoryMenu);
            UI testInterface = TestInterface.Create();
            testInterface.AddMenu(mainMenu);
            testInterface.AddMenu(balanceHistoryMenu);
            testInterface.Show();
            
            var actual = testInterface.GetCurrentMenuName();
            Assert.AreEqual(expected: nameof(MainMenu), actual);
            testInterface.Select(selectedMenuIndex: 0);

            actual = testInterface.GetCurrentMenuName();
            Assert.AreEqual(expected: nameof(BalanceHistoryMenu), actual);
        }
    }
}