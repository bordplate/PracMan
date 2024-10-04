using NLua;
using TrainMan.TrainerUI;
using TrainManCore.Scripting;
using TrainManCore.Scripting.UI;

namespace TrainMan;

public class TrainerModule: ITrainer {
    public List<IWindow> Windows { get; set; } = [];
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

    public IWindow CreateWindow(Module module, LuaTable luaObject, bool isMainWindow = false) {
        if (_mainWindow == null && !isMainWindow) {
            throw new Exception("Main window must be created first.");
        }

        if (isMainWindow && _mainWindow != null) {
            throw new Exception("Main window already created.");
        }
        
        var window = new TrainerViewController(isMainWindow, luaObject, module);
        
        if (isMainWindow) {
            _mainWindow = window;
        }
        
        Windows.Add(window);

        return window;
    }
    
    public IMenu AddMenu(string title, Action<IMenu>? callback) {
        var newMenu = new Menu(_mainWindow, title);

        foreach (var menu in _menus) {
            // If the menu already exists, add a separator and return the existing menu
            if (menu.Title == title) {
                menu.AddSeparator();

                if (callback != null) {
                    TryInvoke(_mainWindow, callback, menu);
                }

                return menu;
            }
        }
        
        _menus.Add(newMenu);
        MainMenu.InsertItem(newMenu, _menus.Count);

        if (callback != null) {
            TryInvoke(_mainWindow, callback, newMenu);
        }

        return newMenu;
    }

    public void ActivateMenu() {
        NSApplication.SharedApplication.MainMenu = MainMenu;
        NSApplication.SharedApplication.WindowsMenu = WindowsMenu.Submenu!;
        NSApplication.SharedApplication.HelpMenu = HelpMenu.Submenu;
    }
    
    public void CloseAllWindows() {
        foreach (var window in Windows) {
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

    public static void TryInvoke(IWindow? window, LuaFunction action, params object[] args) {
        try {
            action.Call(args);
        } catch (Exception e) {
            var alert = new NSAlert {
                AlertStyle = NSAlertStyle.Critical,
                InformativeText = e.Message,
                MessageText = "An error occurred"
            };
            
            if (window is TrainerViewController trainerViewController) {
                alert.RunSheetModal(trainerViewController.Window);
            } else {
                alert.RunModal();
            }
        }
    }

    public static void TryInvoke(IWindow? window, Action action, params object[] args) {
        try {
            action();
        } catch (Exception e) {
            var alert = new NSAlert {
                AlertStyle = NSAlertStyle.Critical,
                InformativeText = e.Message,
                MessageText = "An error occurred"
            };
            
            if (window is TrainerViewController trainerViewController) {
                alert.RunSheetModal(trainerViewController.Window);
            } else {
                alert.RunModal();
            }
        }
    }

    public static void TryInvoke<T>(IWindow? window, Action<T> action, params object[] args) {
        try {
            action((T)args[0]);
        } catch (Exception e) {
            var alert = new NSAlert {
                AlertStyle = NSAlertStyle.Critical,
                InformativeText = e.Message,
                MessageText = "An error occurred"
            };
            
            if (window is TrainerViewController trainerViewController) {
                alert.RunSheetModal(trainerViewController.Window);
            } else {
                alert.RunModal();
            }
        }
    }
}
