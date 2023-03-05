public class AccountSerializator : Serializator
{
    private static int lastId;
    private Account account;
    private string serializedAccount;

    public static AccountSerializator Create(Account account)
    {
        return new AccountSerializator(account);
    }

    public static AccountSerializator Create(string serializedAccount)
    {
        return new AccountSerializator(serializedAccount);
    }

    private AccountSerializator(Account account) 
    {
        this.account = account;
    }

    private AccountSerializator(string serializedAccount)
    {
        this.serializedAccount = serializedAccount;
    }

    private static int GetNewId()
    {
        return lastId++;
    }

    public Object Deserialize()
    {
        var info = serializedAccount.Split(';');
        var id = int.Parse(info[1].Split(':')[1]);
        var name = info[2].Split(':')[1];
        return Account.Create(name, null, id);
    }

    public string Serialize()
    {
        return string.Format("Account;Id:{0};Name:{1}", GetNewId(), account.Name);        
    }
}