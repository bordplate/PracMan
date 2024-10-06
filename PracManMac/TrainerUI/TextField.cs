using NLua;
using NLua.Exceptions;
using PracManCore.Scripting.UI;

namespace PracManMac.TrainerUI;

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

    public string GetText() {
        return StringValue;
    }
    
    [Export("textFieldDidChange:")]
    public void TextFieldDidChange(NSObject sender) {
        try {
            _callback.Call(this.StringValue);
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