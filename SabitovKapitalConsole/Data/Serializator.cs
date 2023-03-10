namespace SabitovCapitalConsole.Data
{
    public interface Serializator
    {
        object Deserialize();
        string Serialize();
    }
}