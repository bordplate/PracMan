using NLua;
using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public class Menu: NSMenuItem, IMenu {
    public Menu(string title) {
        Title = title;
        
        Submenu = new NSMenu(title);
    }
    
    public void AddSeparator() {
        Submenu.AddItem(SeparatorItem);
    }

    public IMenuItem AddItem(string title, Action callback) {
        var item = new MenuItem(title, callback);
        
        Submenu.AddItem(item);
        
        return item;
    }

    public IMenuItem AddCheckItem(string title, LuaFunction callback) {
        throw new NotImplementedException();
    }
}