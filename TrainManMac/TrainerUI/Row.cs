using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public class Row: Container {
    public Row(IWindow window) : base(window) {
        Window = window;
        
        TranslatesAutoresizingMaskIntoConstraints = false;
        Orientation = NSUserInterfaceLayoutOrientation.Horizontal;
        Distribution = NSStackViewDistribution.FillEqually;
    }

    public override void ConstrainElement(NSView element) {
        
    }
}