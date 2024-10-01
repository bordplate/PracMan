using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public class Label: NSTextField, ILabel {
    public IWindow Window { get; }
    
    public Label(IWindow window, string text) {
        Window = window;
        
        StringValue = text;
        Editable = false;
        DrawsBackground = false;
        Bordered = false;
    }
}