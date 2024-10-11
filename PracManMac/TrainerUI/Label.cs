using PracManCore.Scripting.UI;

namespace PracManMac.TrainerUI;

public class Label: NSTextField, ILabel {
    public IWindow Window { get; }
    
    public Label(IWindow window, string text) {
        Window = window;
        
        StringValue = text;
        Editable = false;
        DrawsBackground = false;
        Bordered = false;
    }

    public void SetText(string text) {
        StringValue = text;
    }
}