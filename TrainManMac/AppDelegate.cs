using TrainManCore.Scripting;
using TrainManCore.Scripting.Exceptions;
using TrainManCore.Target;

namespace TrainMan;

[Register("AppDelegate")]
public class AppDelegate : NSApplicationDelegate, IApplication {
    public AttachViewController AttachViewController = new();

    public NSMenu MainMenu = new();
    public NSMenuItem WindowsMenu;
    public NSMenuItem HelpMenu;
    
    private Dictionary<Target, ModLoaderViewController> _modLoaders = new();

    public AppDelegate() {
        WindowsMenu = CreateWindowMenuItem();
        HelpMenu = CreateHelpMenuItem();
        
        Application.Delegate = this;
    }
    
    public override void DidFinishLaunching(NSNotification notification) {
        CreateMenu();
        
        AttachViewController.Window.MakeKeyAndOrderFront(this);
    }
    
    public override bool ApplicationShouldHandleReopen(NSApplication sender, bool hasVisibleWindows)
    {
        if (!hasVisibleWindows) {
            // We activate the mod loader if there are active targets, otherwise we show the attach view controller
            if (Application.ActiveTargets.Count > 0) {
                foreach (var target in Application.ActiveTargets) {
                    OpenModLoader(target);
                }
            } else {
                AttachViewController.Window.MakeKeyAndOrderFront(this);
            }
        }
        
        return true;
    }

    public NSMenuItem CreateAppMenuItem() {
        var appMenuItem = new NSMenuItem();

        var appMenu = new NSMenu();
        appMenuItem.Submenu = appMenu;
        
        // "About" menu item
        var aboutTitle = $"About {NSProcessInfo.ProcessInfo.ProcessName}";
        var aboutMenuItem = new NSMenuItem(aboutTitle, new ObjCRuntime.Selector("orderFrontStandardAboutPanel:"), "");
        appMenu.AddItem(aboutMenuItem);

        // Separator
        appMenu.AddItem(NSMenuItem.SeparatorItem);

        // "Quit" menu item
        var quitTitle = $"Quit {NSProcessInfo.ProcessInfo.ProcessName}";
        var quitMenuItem = new NSMenuItem(quitTitle, new ObjCRuntime.Selector("terminate:"), "q");
        appMenu.AddItem(quitMenuItem);

        return appMenuItem;
    }

    public NSMenuItem CreateEditMenuItem() {
        var editMenuItem = new NSMenuItem();

        var editMenu = new NSMenu("Edit");
        editMenuItem.Submenu = editMenu;

        editMenu.AddItem(new NSMenuItem("Undo", new ObjCRuntime.Selector("undo:"), "z"));
        editMenu.AddItem(new NSMenuItem("Redo", new ObjCRuntime.Selector("redo:"), "Z"));
        editMenu.AddItem(NSMenuItem.SeparatorItem);
        editMenu.AddItem(new NSMenuItem("Cut", new ObjCRuntime.Selector("cut:"), "x"));
        editMenu.AddItem(new NSMenuItem("Copy", new ObjCRuntime.Selector("copy:"), "c"));
        editMenu.AddItem(new NSMenuItem("Paste", new ObjCRuntime.Selector("paste:"), "v"));
        editMenu.AddItem(new NSMenuItem("Select All", new ObjCRuntime.Selector("selectAll:"), "a"));

        return editMenuItem;
    }
    
    public NSMenuItem CreateWindowMenuItem() {
        var windowMenuItem = new NSMenuItem();

        var windowMenu = new NSMenu("Window");
        windowMenuItem.Submenu = windowMenu;

        windowMenu.AddItem(new NSMenuItem("Minimize", new ObjCRuntime.Selector("performMiniaturize:"), "m"));
        windowMenu.AddItem(new NSMenuItem("Zoom", new ObjCRuntime.Selector("performZoom:"), ""));
        windowMenu.AddItem(new NSMenuItem("Close", new ObjCRuntime.Selector("performClose:"), "w"));
        windowMenu.AddItem(NSMenuItem.SeparatorItem);
        windowMenu.AddItem(new NSMenuItem("Bring All to Front", new ObjCRuntime.Selector("arrangeInFront:"), ""));

        return windowMenuItem;
    }
    
    public NSMenuItem CreateHelpMenuItem() {
        var helpMenuItem = new NSMenuItem();

        var helpMenu = new NSMenu("Help");
        helpMenuItem.Submenu = helpMenu;

        helpMenu.AddItem(new NSMenuItem("Help", new ObjCRuntime.Selector("showHelp:"), "?"));
        helpMenu.AddItem(new NSMenuItem("About", new ObjCRuntime.Selector("showAbout:"), ""));
        
        return helpMenuItem;
    }

    void CreateMenu() {
        MainMenu.AddItem(CreateAppMenuItem());
        MainMenu.AddItem(CreateEditMenuItem());
        
        MainMenu.AddItem(WindowsMenu);
        MainMenu.AddItem(HelpMenu);

        // Assign the main menu to the application
        NSApplication.SharedApplication.MainMenu = MainMenu;
        NSApplication.SharedApplication.HelpMenu = HelpMenu.Submenu;
        NSApplication.SharedApplication.WindowsMenu = WindowsMenu.Submenu!;
    }
    
    public void ActivateMenu() {
        NSApplication.SharedApplication.MainMenu = MainMenu;
        NSApplication.SharedApplication.WindowsMenu = WindowsMenu.Submenu!;
        NSApplication.SharedApplication.HelpMenu = HelpMenu.Submenu;
    }

    public void OpenModLoader(Target target) {
        if (!_modLoaders.ContainsKey(target)) {
            _modLoaders[target] = new ModLoaderViewController(target);
        }

        _modLoaders[target].Window.MakeKeyAndOrderFront(null);
    }

    public void OnModuleLoad(Module module, Target target) {
        var moduleDelegate = new ModuleDelegate();
        module.Delegate = moduleDelegate;
        
        module.OnExit += () => {
            _modLoaders.TryGetValue(target, out var modLoaderWindow);
            
            if (target.Modules.Count == 0 &&
                (modLoaderWindow == null || !modLoaderWindow.Window.IsVisible)) {
                target.Stop();
            }
        };

        moduleDelegate.AddMenu("Trainer", (menu) => {
            menu.AddItem("Mod loader...", () => {
                OpenModLoader(target);
            });
                            
            if (module.Settings.Get<string>("General.inputs_controller", "") != "") {
                if (module.LoadInputs() is { } inputs) {
                    menu.AddSeparator();
                    
                    menu.AddItem("Input display", () => {
                        if (moduleDelegate.InputDisplayViewController == null) {
                            moduleDelegate.InputDisplayViewController = new InputDisplayViewController(inputs);
                        }
                                        
                        moduleDelegate.InputDisplayViewController.Window.MakeKeyAndOrderFront(null);
                    });
                
                    var buttonCombosCheckItem = menu.AddCheckItem("Enable button combos", enabled => {
                        if (enabled) {
                            inputs.EnableButtonCombos();
                        }
                        else {
                            inputs.DisableButtonCombos();
                        }
                    });
                    
                    menu.OnOpen(() => {
                        buttonCombosCheckItem.Checked = inputs.ButtonCombosListening;
                    });

                    buttonCombosCheckItem.Checked = inputs.ButtonCombosListening;

                    menu.AddItem("Configure button combos...", () => {
                        var buttonCombosViewController = new ButtonCombosViewController(inputs);
                        buttonCombosViewController.Window.MakeKeyAndOrderFront(null);
                    });
                }
            }
        });
        
        moduleDelegate.ActivateMenu();
    }
    
    public override void WillTerminate(NSNotification notification)
    {
        // Insert code here to tear down your application
    }
    
    public void RunOnMainThread(Action action) {
        NSApplication.SharedApplication.InvokeOnMainThread(action);
    }
}