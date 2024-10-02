using TrainManCore.Scripting;
using TrainManCore.Target;

namespace TrainMan;

[Register("AppDelegate")]
public class AppDelegate : NSApplicationDelegate {
    public AttachViewController AttachViewController = new();

    public NSMenu MainMenu = new NSMenu();
    public NSMenuItem WindowsMenu;
    public NSMenuItem HelpMenu;
    
    private Dictionary<Target, Dictionary<string, object>> _activeTargets = new();

    public AppDelegate() {
        WindowsMenu = CreateWindowMenuItem();
        HelpMenu = CreateHelpMenuItem();
    }
    
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
        
        if (_activeTargets[target]["Modules"] is List<Module> modules) {
            modules.Add(module);
        }
    }
    
    public void RemoveModuleForTarget(Module module, Target target) {
        if (!_activeTargets.TryGetValue(target, out var activeTarget)) {
            throw new Exception("Target not found.");
        }
        
        if (!activeTarget.ContainsKey("Modules")) {
            throw new Exception("No modules found for target.");
        }
        
        if (_activeTargets[target]["Modules"] is List<Module> modules) {
            modules.Add(module);
        }
    }
    
    public List<Module>? ModulesForTarget(Target target) {
        if (!_activeTargets.TryGetValue(target, out var activeTarget)) {
            return null;
        }
        
        if (!activeTarget.ContainsKey("Modules")) {
            return new();
        }
        
        return _activeTargets[target]["Modules"] as List<Module>;
    }

    public List<Module> AllModulesForTarget(Target target) {
        // Get all modules for a target, enabled modules should be replaced with their enabled versions
        var allModules = Module.ModulesForTitle(target.GetGameTitleID(), target);
        var enabledModules = ModulesForTarget(target);
        
        if (enabledModules == null) {
            return allModules;
        }
        
        foreach (var module in enabledModules) {
            var index = allModules.FindIndex(m => m.ModulePath == module.ModulePath);
            allModules[index] = module;
        }
        
        return allModules;
    }

    public ModLoaderViewController ModLoaderFor(Target target) {
        if (_activeTargets[target].ContainsKey("ModLoader")) {
            // Reopen the mod loader
            if (_activeTargets[target]["ModLoader"] is ModLoaderViewController modLoader) {
                return modLoader;
            }
        }
        
        // Create a new mod loader
        var loader = new ModLoaderViewController(target);

        _activeTargets[target]["ModLoader"] = loader;
        
        return loader;
    }

    public void EnableModForTarget(Module module, Target target) {
        var trainerModule = new TrainerModule();
        module.TrainerDelegate = trainerModule;
                        
        AddModuleForTarget(module, target);

        module.OnExit += () => {
            RemoveModuleForTarget(module, target);

            if (ModulesForTarget(target)?.Count == 0 &&
                !ModLoaderFor(target).Window.IsVisible) {
                target.Stop();
            }
        };

        trainerModule.AddMenu("Trainer", (menu) => {
            menu.AddItem("Mod loader...", () => {
                ModLoaderFor(target).Window.MakeKeyAndOrderFront(null);
            });
                            
            if (module.Settings.Get<string>("General.inputs_controller", "") != "") {
                if (module.LoadInputs() is { } inputs) {
                    menu.AddSeparator();
                    
                    menu.AddItem("Input display", () => {
                        if (trainerModule.InputDisplayViewController == null) {
                            trainerModule.InputDisplayViewController = new InputDisplayViewController(inputs);
                        }
                                        
                        trainerModule.InputDisplayViewController.Window.MakeKeyAndOrderFront(null);
                    });
                
                    var buttonCombosCheckItem = menu.AddCheckItem("Enable button combos", enabled => {
                        if (enabled) {
                            inputs.EnableButtonCombos();
                        }
                        else {
                            inputs.DisableButtonCombos();
                        }
                    });

                    buttonCombosCheckItem.Checked = inputs.ButtonCombosListening;

                    menu.AddItem("Configure button combos...", () => {
                        var buttonCombosViewController = new ButtonCombosViewController(inputs);
                        buttonCombosViewController.Window.MakeKeyAndOrderFront(null);
                    });
                }
            }
        });
        
        trainerModule.ActivateMenu();
                        
        module.Load();
    }
    
    public override void WillTerminate(NSNotification notification)
    {
        // Insert code here to tear down your application
    }
}