public class UI
{
    public string Content { get; private set; } = string.Empty;
    public Menu CurrentMenu { get; private set; }

    private List<Menu> menus = new List<Menu>();

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
        try
        {
            CurrentMenu = CurrentMenu.GetMenu(selectedMenuIndex);
        }
        catch (KeyNotFoundException exception)
        {
            throw exception;
        }        
    }

    public void GoHome()
    {
        CurrentMenu = CurrentMenu.HomeMenu;
    }    

    public virtual void Show()
    {
        Content = CurrentMenu.Content;
    }
}