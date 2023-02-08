public class BalanceHistoryMenu : Menu
{
    public static BalanceHistoryMenu Create(string name)
    {
        return new BalanceHistoryMenu(name);
    }

    private BalanceHistoryMenu(string name)
    {
        Name = name;
    }
}