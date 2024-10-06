using NLua;

namespace PracManCore.Scripting.UI;

public interface IButtonListener {
    public void OnButtonPressed(IButton button);
}

public interface IButton: IControl {
    public IWindow Window { get; }
    public string Title { get; set; }
    public LuaFunction Callback { get; set; }

    public void Activate() {
        if (Inputs.ButtonListener != null) {
            Inputs.ButtonListener.OnButtonPressed(this);
            
            return;
        }
        
        Callback.Call();
    }
}