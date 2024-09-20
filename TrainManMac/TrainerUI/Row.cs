namespace TrainMan.TrainerUI;

public class Row: Container {
    public Row() : base() {
        TranslatesAutoresizingMaskIntoConstraints = false;
        Orientation = NSUserInterfaceLayoutOrientation.Horizontal;
        Distribution = NSStackViewDistribution.FillEqually;
    }

    public override void ConstrainElement(NSView element) {
        
    }
}