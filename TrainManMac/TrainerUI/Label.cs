using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public class Label: NSTextField, ILabel {
    public Label(string text) {
        StringValue = text;
        Editable = false;
        DrawsBackground = false;
        Bordered = false;
    }
}