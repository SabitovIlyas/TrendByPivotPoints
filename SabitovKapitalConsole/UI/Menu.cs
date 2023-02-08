public class Menu
{
    public string Name { get; protected set; }
    private Dictionary<int, Menu> linkedMenus = new Dictionary<int, Menu>();    

    public void Link(int selection, Menu menu)
    {
        linkedMenus.Add(selection, menu);
    }

    public Menu GetMenu(int selection)
    {
        linkedMenus.TryGetValue(selection, out Menu menu);
        return menu;
    }
}