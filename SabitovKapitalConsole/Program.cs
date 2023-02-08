Console.WriteLine("Hello, World!");

Balance balance = Balance.Create();
Account account = Account.Create("Пятанов Иван Вадимович", balance);

var dateTime = new DateTime(2023, 01, 25);
account.CreateTransaction(Operation.Deposit, 50000, dateTime);
balance.Update(dateTime, 1344578.62m);
