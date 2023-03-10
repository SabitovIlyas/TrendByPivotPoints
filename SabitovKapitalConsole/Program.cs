using SabitovCapitalConsole.Entities;

var balance = Balance.Create();
var portfolio = Portfolio.Create(balance);
var account = Account.Create("Пятанов Иван Вадимович", portfolio);

var dateTime = new DateTime(2023, 01, 25);
account.CreateTransaction(Operation.Deposit, 50000, dateTime);
balance.Update(dateTime, 1344578.62m);

var mainMenu = Menu.Create(name: "Основное меню");
var balanceHistoryMenu = Menu.Create(name: "История изменения баланса");
var addBalanceStampMenu = Menu.Create(name: "Добавить новый баланс");
var participantsMenu = Menu.Create(name: "Участники");

mainMenu.Link(selection: 0, menu: balanceHistoryMenu);
mainMenu.Link(selection: 1, menu: addBalanceStampMenu);
mainMenu.Link(selection: 2, menu: participantsMenu);

var consoleInterface = ConsoleInterface.Create();
consoleInterface.AddMenu(mainMenu);
consoleInterface.AddMenu(balanceHistoryMenu);
consoleInterface.AddMenu(addBalanceStampMenu);
consoleInterface.AddMenu(participantsMenu);

consoleInterface.Run();