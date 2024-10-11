using NLua;
using NLua.Exceptions;
using PracManCore.Scripting.UI;

namespace PracManMac.TrainerUI;

public class Checkbox: NSButton, ICheckbox {
    public IWindow Window { get; }
    
    LuaFunction _callback;
    
    public bool Checked {
        get => IsChecked();
        set => SetChecked(value);
    }
    
    public Checkbox(IWindow window, string text, LuaFunction callback) {
        Window = window;
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
        try {
            _callback.Call(IsChecked());
        } catch (LuaScriptException exception) {
            Console.Error.WriteLine(exception.Message);
            new NSAlert {
                AlertStyle = NSAlertStyle.Critical,
                InformativeText = exception.Message,
                MessageText = "Error",
            }.RunSheetModal(((ModuleWindowViewController)Window).Window);
        }
    }

    public void SetChecked(bool isChecked, bool callingCallback = false) {
        // These should be NSControlStateValue, but they don't work in MAUI for some reason
        State = isChecked ? NSCellStateValue.On : NSCellStateValue.Off;
        
        if (callingCallback) {
            try {
                _callback.Call(isChecked);
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

    public bool IsChecked() {
        return State == NSCellStateValue.On;
    }
}