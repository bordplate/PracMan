using NLua;
using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public class Button: NSButton, IButton {
    private LuaFunction _callback;
    
    public Button(string title, LuaFunction callback) {
        _callback = callback;
        
        Title = title;
        BezelStyle = NSBezelStyle.Rounded;
        TranslatesAutoresizingMaskIntoConstraints = false;
        Target = this;
        Action = new ObjCRuntime.Selector("buttonClicked:");
    }
    
    [Export("buttonClicked:")]
    public void ButtonClicked(NSObject sender) {
        _callback.Call();
    }
}