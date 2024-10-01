using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public class Spacer: NSView, ISpacer {
    public IWindow Window { get; }
    
    public Spacer(IWindow window) {
        Window = window;
        
        TranslatesAutoresizingMaskIntoConstraints = false;
        
        // Enforce a minimum size of 10x10
        AddConstraint(NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 10, 0));
    }
}