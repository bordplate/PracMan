using NLua;
using PracManCore.Scripting.UI;

namespace PracManMac.TrainerUI;

public class Menu: NSMenuItem, IMenu, INSMenuDelegate {
    public IWindow Window { get; }

    private event Action? onOpen;
    
    public Menu(IWindow? window, string title) {
        Window = window!;
        Title = title;
        
        Submenu = new NSMenu(title);
        Submenu.Delegate = this;
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
    
    public void OnOpen(Action callback) {
        onOpen += callback;
    }
    
    [Export("menuWillOpen:")]
    public void MenuWillOpen(NSMenu menu) {
        onOpen?.Invoke();
    }
}