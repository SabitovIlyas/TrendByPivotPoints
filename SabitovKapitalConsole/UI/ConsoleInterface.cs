public class ConsoleInterface
{ 
    public static ConsoleInterface Create(Mode mode)
    {
        return new ConsoleInterface(mode); 
    }

    private ConsoleInterface(Mode mode) 
    {
        Run();
    }

    public void Run()
    {
        Console.WriteLine("");
        Console.ReadLine();
    }
}
