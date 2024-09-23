using TrainManCore.Scripting;
using TrainManCore.Target;

namespace TrainMan;

[Register("AppDelegate")]
public class AppDelegate : NSApplicationDelegate {
    public AttachViewController AttachViewController = new();

    public NSMenu MainMenu;
    public NSMenuItem WindowsMenu;
    public NSMenuItem HelpMenu;
    
    private Dictionary<Target, Dictionary<string, object>> _activeTargets = new();
    
    public override void DidFinishLaunching(NSNotification notification) {
        CreateMenu();
        
        AttachViewController.Window.MakeKeyAndOrderFront(this);
    }
    
    public override bool ApplicationShouldHandleReopen(NSApplication sender, bool hasVisibleWindows)
    {
        if (!hasVisibleWindows)
        {
            // We activate the mod loader if there are active targets, otherwise we show the attach view controller
            if (_activeTargets.Count > 0) {
                foreach (var target in _activeTargets.Keys) {
                    ModLoaderFor(target).Window.MakeKeyAndOrderFront(this);
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
        MainMenu = new NSMenu();
        // *** Application Menu ***
        MainMenu.AddItem(CreateAppMenuItem());
        
        // *** File Menu ***
        var fileMenuItem = new NSMenuItem();

        var fileMenu = new NSMenu("File");
        fileMenuItem.Submenu = fileMenu;
        
        MainMenu.AddItem(fileMenuItem);

        // *** Edit Menu ***
        MainMenu.AddItem(CreateEditMenuItem());

        // *** Window Menu ***
        WindowsMenu = CreateWindowMenuItem();
        MainMenu.AddItem(WindowsMenu);

        // *** Help Menu ***
        HelpMenu = CreateHelpMenuItem();
        MainMenu.AddItem(HelpMenu);

        // Assign the main menu to the application
        NSApplication.SharedApplication.MainMenu = MainMenu;
        NSApplication.SharedApplication.HelpMenu = HelpMenu.Submenu;
        NSApplication.SharedApplication.WindowsMenu = WindowsMenu.Submenu;
    }
    
    public void ActivateMenu() {
        NSApplication.SharedApplication.MainMenu = MainMenu;
        NSApplication.SharedApplication.WindowsMenu = WindowsMenu.Submenu;
        NSApplication.SharedApplication.HelpMenu = HelpMenu.Submenu;
    }
    
    public void AddTarget(Target target) {
        if (!_activeTargets.ContainsKey(target)) {
            _activeTargets[target] = new();
        }
    }
    
    public void RemoveTarget(Target target) {
        if (_activeTargets.ContainsKey(target)) {
            _activeTargets.Remove(target);
        }
    }
    
    public void AddModuleForTarget(Module module, Target target) {
        if (!_activeTargets.ContainsKey(target)) {
            throw new Exception("Target not found.");
        }
        
        if (!_activeTargets[target].ContainsKey("Modules")) {
            _activeTargets[target]["Modules"] = new List<Module>();
        }
        
        var modules = _activeTargets[target]["Modules"] as List<Module>;
        modules.Add(module);
    }
    
    public void RemoveModuleForTarget(Module module, Target target) {
        if (!_activeTargets.ContainsKey(target)) {
            throw new Exception("Target not found.");
        }
        
        if (!_activeTargets[target].ContainsKey("Modules")) {
            throw new Exception("No modules found for target.");
        }
        
        var modules = _activeTargets[target]["Modules"] as List<Module>;
        modules.Remove(module);
    }
    
    public List<Module>? ModulesForTarget(Target target) {
        if (!_activeTargets.ContainsKey(target)) {
            return null;
        }
        
        if (!_activeTargets[target].ContainsKey("Modules")) {
            return new();
        }
        
        return _activeTargets[target]["Modules"] as List<Module>;
    }

    public ModLoaderViewController ModLoaderFor(Target target) {
        if (!_activeTargets.ContainsKey(target)) {
            throw new Exception("Target not found.");
        }
        
        if (_activeTargets[target].ContainsKey("ModLoader")) {
            // Reopen the mod loader
            var modLoader = _activeTargets[target]["ModLoader"] as ModLoaderViewController;
            return modLoader;
        } else {
            // Create a new mod loader
            var modLoader = new ModLoaderViewController(target);

            _activeTargets[target]["ModLoader"] = modLoader;
            
            return modLoader;
        }
    }
    
    public override void WillTerminate(NSNotification notification)
    {
        // Insert code here to tear down your application
    }
}