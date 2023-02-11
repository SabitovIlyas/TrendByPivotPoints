Balance balance = Balance.Create();
Account account = Account.Create("Пятанов Иван Вадимович", balance);

var dateTime = new DateTime(2023, 01, 25);
account.CreateTransaction(Operation.Deposit, 50000, dateTime);
balance.Update(dateTime, 1344578.62m);

Menu mainMenu = Menu.Create(name: "Основное меню");
Menu balanceHistoryMenu = Menu.Create(name: "История изменения баланса");
Menu addBalanceStampMenu = Menu.Create(name: "Добавить новый баланс");
Menu participantsMenu = Menu.Create(name: "Участники");

mainMenu.Link(selection: 0, menu: balanceHistoryMenu);
mainMenu.Link(selection: 1, menu: addBalanceStampMenu);
mainMenu.Link(selection: 2, menu: participantsMenu);

var consoleInterface = ConsoleInterface.Create();
consoleInterface.AddMenu(mainMenu);
consoleInterface.AddMenu(balanceHistoryMenu);
consoleInterface.AddMenu(addBalanceStampMenu);
consoleInterface.AddMenu(participantsMenu);

consoleInterface.Run();