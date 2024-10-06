using NLua;
using NLua.Exceptions;
using PracManCore.Scripting.UI;

namespace PracManMac.TrainerUI;

public class Button: NSButton, IButton {
    public IWindow Window { get; }
    public LuaFunction Callback { get; set; }
    
    public Button(IWindow window, string title, LuaFunction callback) {
        Window = window;
        Callback = callback;
        
        Title = title;
        BezelStyle = NSBezelStyle.Rounded;
        TranslatesAutoresizingMaskIntoConstraints = false;
        Target = this;
        Action = new ObjCRuntime.Selector("buttonClicked:");
    }
    
    [Export("buttonClicked:")]
    public void ButtonClicked(NSObject sender) {
        try {
            ((IButton)this).Activate();
        } catch (LuaScriptException exception) {
            Console.Error.WriteLine(exception.Message);
            new NSAlert {
                AlertStyle = NSAlertStyle.Critical,
                InformativeText = exception.Message,
                MessageText = "Error",
            }.RunSheetModal(((ModuleWindowViewController)Window).Window);
        }
    }
}