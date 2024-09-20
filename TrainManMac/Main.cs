using TrainMan;

static void CreateMenu() {
    var mainMenu = new NSMenu();
    // *** Application Menu ***
    var appMenuItem = new NSMenuItem();
    mainMenu.AddItem(appMenuItem);

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

    // *** File Menu ***
    var fileMenuItem = new NSMenuItem();
    mainMenu.AddItem(fileMenuItem);

    var fileMenu = new NSMenu("File");
    fileMenuItem.Submenu = fileMenu;

    // *** Edit Menu ***
    var editMenuItem = new NSMenuItem();
    mainMenu.AddItem(editMenuItem);

    var editMenu = new NSMenu("Edit");
    editMenuItem.Submenu = editMenu;

    editMenu.AddItem(new NSMenuItem("Undo", new ObjCRuntime.Selector("undo:"), "z"));
    editMenu.AddItem(new NSMenuItem("Redo", new ObjCRuntime.Selector("redo:"), "Z"));
    editMenu.AddItem(NSMenuItem.SeparatorItem);
    editMenu.AddItem(new NSMenuItem("Cut", new ObjCRuntime.Selector("cut:"), "x"));
    editMenu.AddItem(new NSMenuItem("Copy", new ObjCRuntime.Selector("copy:"), "c"));
    editMenu.AddItem(new NSMenuItem("Paste", new ObjCRuntime.Selector("paste:"), "v"));
    editMenu.AddItem(new NSMenuItem("Select All", new ObjCRuntime.Selector("selectAll:"), "a"));

    // *** Window Menu ***
    var windowMenuItem = new NSMenuItem();
    mainMenu.AddItem(windowMenuItem);

    var windowMenu = new NSMenu("Window");
    windowMenuItem.Submenu = windowMenu;

    // Set the window menu in the NSApplication
    NSApplication.SharedApplication.WindowsMenu = windowMenu;

    // Standard Window menu items
    windowMenu.AddItem(new NSMenuItem("Minimize", new ObjCRuntime.Selector("performMiniaturize:"), "m"));
    windowMenu.AddItem(new NSMenuItem("Zoom", new ObjCRuntime.Selector("performZoom:"), ""));
    windowMenu.AddItem(NSMenuItem.SeparatorItem);
    windowMenu.AddItem(new NSMenuItem("Bring All to Front", new ObjCRuntime.Selector("arrangeInFront:"), ""));

    // *** Help Menu ***
    var helpMenuItem = new NSMenuItem();
    mainMenu.AddItem(helpMenuItem);

    var helpMenu = new NSMenu("Help");
    helpMenuItem.Submenu = helpMenu;

    // Set the help menu in the NSApplication
    NSApplication.SharedApplication.HelpMenu = helpMenu;

    // Assign the main menu to the application
    NSApplication.SharedApplication.MainMenu = mainMenu;
}

// This is the main entry point of the application.
NSApplication.Init();

NSApplication.SharedApplication.Delegate = new AppDelegate();

CreateMenu();

NSApplication.Main(args);