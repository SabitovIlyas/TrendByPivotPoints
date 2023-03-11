using SabitovCapitalConsole.Entities;

public class AccountsMenu : Menu
{
    Portfolio portfolio;
    public static AccountsMenu Create(string name, Portfolio portfolio)
    {
        return new AccountsMenu(name, portfolio);
    }

    private AccountsMenu(string name, Portfolio portfolio):base(name)
    {
        this.portfolio = portfolio;

        foreach (var account in portfolio.Accounts)
            Link(account.Id, AccountBalanceMenu.Create(account.Name, account));
        //Content += portfolio.ToString()+"\r\n";
    }
}

public class AccountBalanceMenu : Menu
{
    Account account;
    public static AccountBalanceMenu Create(string name, Account account)
    {
        return new AccountBalanceMenu(name, account);
    }

    private AccountBalanceMenu(string name, Account account) : base(name)
    {
        this.account = account;

        var deposit = Math.Round(account.GetDeposit(), 2);
        var profit = Math.Round(account.GetProfit(), 2);
        var balance = deposit + profit;
        Content = string.Format("{0}\r\n\r\nТекущий баланс составляет: " +
            "{1}\r\nТекущий депозит: {2}\r\nТекущая прибыль: {3}\r\n", 
            account.Name, balance, deposit, profit);
    }
}