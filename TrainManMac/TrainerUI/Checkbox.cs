using NLua;
using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public class Checkbox: NSButton, ICheckbox {
    LuaFunction _callback;
    
    public Checkbox(string text, LuaFunction callback) {
        Title = text;
        BezelStyle = NSBezelStyle.Rounded;
        TranslatesAutoresizingMaskIntoConstraints = false;
        Target = this;
        Action = new ObjCRuntime.Selector("checkboxClicked:");
        
        SetButtonType(NSButtonType.Switch);
        
        _callback = callback;
    }

    [Export("checkboxClicked:")]
    public void CheckboxClicked(NSObject sender) {
        _callback.Call(IsChecked());
    }

    public void SetChecked(bool isChecked) {
        // These should be NSControlStateValue, but they don't work in MAUI for some reason
        State = isChecked ? NSCellStateValue.On : NSCellStateValue.Off;
    }

    public bool IsChecked() {
        return State == NSCellStateValue.On;
    }
}