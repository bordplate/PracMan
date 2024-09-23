using NLua;
using TrainMan.TrainerUI;
using TrainManCore.Scripting;
using TrainManCore.Scripting.UI;

namespace TrainMan;

public class TrainerModule: ITrainer {
    List<IWindow> _windows = [];
    private IWindow? _mainWindow;
    
    private List<IMenu> _menus = [];
    
    public InputDisplayViewController? InputDisplayViewController;
    
    public NSMenu MainMenu;
    public NSMenuItem WindowsMenu;
    public NSMenuItem HelpMenu;
    
    public TrainerModule() {
        MainMenu = new NSMenu();

        var appDelegate = (AppDelegate)NSApplication.SharedApplication.Delegate;

        MainMenu = new NSMenu();

        MainMenu.AddItem(appDelegate.CreateAppMenuItem());
        MainMenu.AddItem(appDelegate.CreateEditMenuItem());
        
        WindowsMenu = appDelegate.CreateWindowMenuItem();
        MainMenu.AddItem(WindowsMenu);
        
        HelpMenu = appDelegate.CreateHelpMenuItem();
        MainMenu.AddItem(HelpMenu);

        var trainerMenu = AddMenu("Trainer", null);

        trainerMenu.AddItem("Attach to new...", () => {
            appDelegate.AttachViewController.Window.MakeKeyAndOrderFront(null);
        });
    }
    
    public IWindow CreateWindow(Module module, bool isMainWindow = false) {
        if (_mainWindow == null && !isMainWindow) {
            throw new Exception("Main window must be created first.");
        }

        if (isMainWindow && _mainWindow != null) {
            throw new Exception("Main window already created.");
        }
        
        var window = new TrainerViewController(isMainWindow, module);
        
        if (isMainWindow) {
            _mainWindow = window;
        }
        
        _windows.Add(window);

        return window;
    }
    
    public IMenu AddMenu(string title, Action<IMenu>? callback) {
        var newMenu = new Menu(title);

        foreach (var menu in _menus) {
            // If the menu already exists, add a separator and return the existing menu
            if (menu.Title == title) {
                menu.AddSeparator();
                
                callback?.Invoke(menu);
                
                return menu;
            }
        }
        
        _menus.Add(newMenu);
        MainMenu.InsertItem(newMenu, _menus.Count);

        if (callback != null) {
            callback(newMenu);
        }

        return newMenu;
    }

    public void ActivateMenu() {
        NSApplication.SharedApplication.MainMenu = MainMenu;
        NSApplication.SharedApplication.WindowsMenu = WindowsMenu.Submenu;
        NSApplication.SharedApplication.HelpMenu = HelpMenu.Submenu;
    }
    
    public void CloseAllWindows() {
        foreach (var window in _windows) {
            window.Close();
        }
    }

    public void Alert(string text) {
        var alert = new NSAlert {
            AlertStyle = NSAlertStyle.Informational,
            InformativeText = text
        };
        
        alert.RunModal();
    }
    
    public void RunOnMainThread(Action action) {
        NSApplication.SharedApplication.InvokeOnMainThread(() => {
            action();
        });
    }
}
