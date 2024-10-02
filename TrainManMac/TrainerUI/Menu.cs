using NLua;
using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public class Menu: NSMenuItem, IMenu {
    public IWindow Window { get; }
    
    public Menu(IWindow? window, string title) {
        Window = window!;
        Title = title;
        
        Submenu = new NSMenu(title);
    }
    
    public void AddSeparator() {
        Submenu!.AddItem(SeparatorItem);
    }

    public IMenuItem AddItem(string title, Action callback) {
        var item = new MenuItem(Window!, title, callback);
        
        Submenu!.AddItem(item);
        
        return item;
    }

    public IMenuItem AddCheckItem(string title, Action<bool> callback) {
        var item = new MenuItem(Window!, title, callback);
        
        Submenu!.AddItem(item);
        
        return item;
    }
}