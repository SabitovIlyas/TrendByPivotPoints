public class ConsoleUI: UI
{ 
    public static ConsoleUI Create()
    {
        return new ConsoleUI(); 
    }

    private ConsoleUI() 
    {        
    }

    public void Run()
    {   
        while(true)
        {
            try
            {
                Show();
                var selectedMenuIndexStr = Console.ReadLine();
                if (selectedMenuIndexStr == "")
                    GoHome();
                else
                {
                    var selectedMenuIndex = int.Parse(selectedMenuIndexStr);
                    Select(selectedMenuIndex);
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Введено некорректное значение. Нажмите Enter и " +
                    "повторите снова.");
                Console.ReadLine();
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine("Такого пункта нет в меню. Нажмите Enter и " +
                    "повторите снова.");
                Console.ReadLine();
            }
        }        
    }

    public override void Show()
    {
        Console.Clear();
        base.Show();
        Console.WriteLine(Content);
        Console.WriteLine("Введите значение и нажмите Enter или просто нажмите Enter, " +
            "чтобы вернуться в родительское меню.");        
    }
}