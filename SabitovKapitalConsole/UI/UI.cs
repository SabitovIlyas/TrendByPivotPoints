public class UI
{
    List<Menu> menus= new List<Menu>();
    public Menu CurrentMenu { get; private set; }

    public void AddMenu(Menu menu)
    {
        if (menus.Count == 0)
            CurrentMenu = menu;
        menus.Add(menu);
    }

    public string GetCurrentMenuName()
    {
        return CurrentMenu.Name;
    }

    public void Select(int selectedMenuIndex)
    {
        CurrentMenu = CurrentMenu.GetMenu(selectedMenuIndex);
        Show();
    }

    public void Show()
    {        
    }
}