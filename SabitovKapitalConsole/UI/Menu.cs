using System;

public class Menu
{
    public string Name { get; protected set; } = string.Empty;
    public Menu HomeMenu { get; private set; }
    public string Content { get; private set; } = string.Empty;
    private Dictionary<int, Menu> linkedMenus = new Dictionary<int, Menu>();


    public static Menu Create(string name)
    {
        return new Menu(name);
    }

    private Menu(string name)
    {
        Name = name;
        Content = Name + "\r\n\r\n";
        HomeMenu = this;        
    }

    protected Menu()
    {   
    }

    public void Link(int selection, Menu menu)
    {
        linkedMenus.Add(selection, menu);
        Content += string.Format("{0}) {1}\r\n", selection, menu.Name);
        menu.HomeMenu = this;
    }

    public Menu GetMenu(int selection)
    {
        if (linkedMenus.TryGetValue(selection, out Menu menu))
            return menu;
        else
            throw new KeyNotFoundException();
    }
}