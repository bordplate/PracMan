using NLua;
using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public class TextField: NSTextField, ITextField {
    private LuaFunction _callback;
    
    public TextField(LuaFunction callback) {
        DrawsBackground = true;
        Bordered = true;
        
        _callback = callback;
        
        Target = this;
        Action = new ObjCRuntime.Selector("textFieldDidChange:");

        RefusesFirstResponder = true;
    }
    
    [Export("textFieldDidChange:")]
    public void TextFieldDidChange(NSObject sender) {
        _callback.Call(this.StringValue);
    }
}