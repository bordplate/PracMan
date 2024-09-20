namespace TrainMan;

[Register("AppDelegate")]
public class AppDelegate : NSApplicationDelegate {
    private AttachViewController _attachViewController = new();
    
    public override void DidFinishLaunching(NSNotification notification) {
        _attachViewController.Window.MakeKeyAndOrderFront(this);
    }
    
    public override bool ApplicationShouldHandleReopen(NSApplication sender, bool hasVisibleWindows)
    {
        if (!hasVisibleWindows)
        {
            _attachViewController.Window.MakeKeyAndOrderFront(this);
        }
        
        return true;
    }

    public override void WillTerminate(NSNotification notification)
    {
        // Insert code here to tear down your application
    }
}