using NLua;
using NLua.Exceptions;
using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public class TextField: NSTextField, ITextField {
    public IWindow Window { get; }
    private LuaFunction _callback;
    
    public TextField(IWindow window, LuaFunction callback) {
        Window = window;
        
        DrawsBackground = true;
        Bordered = true;
        
        _callback = callback;
        
        Target = this;
        Action = new ObjCRuntime.Selector("textFieldDidChange:");

        RefusesFirstResponder = true;
    }
    
    [Export("textFieldDidChange:")]
    public void TextFieldDidChange(NSObject sender) {
        try {
            _callback.Call(this.StringValue);
        } catch (LuaScriptException exception) {
            new NSAlert {
                AlertStyle = NSAlertStyle.Critical,
                InformativeText = exception.Message,
                MessageText = "Error",
            }.RunSheetModal(((TrainerViewController)Window).Window);
        }
    }
}