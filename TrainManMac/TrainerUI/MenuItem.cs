using NLua;
using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public class MenuItem: NSMenuItem, IMenuItem {
    private Action _callback;
    
    public MenuItem(string title, Action callback) {
        _callback = callback;
        
        Title = title;
        Action = new ObjCRuntime.Selector("menuAction:");
        Target = this;
    }
    
    [Export("menuAction:")]
    private void MenuAction(NSObject sender) {
        _callback();
    }
}