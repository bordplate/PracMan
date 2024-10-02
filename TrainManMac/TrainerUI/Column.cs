using NLua;
using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public class Column: Container {
    public Column(IWindow window) : base(window) {
        Window = window;
        
        TranslatesAutoresizingMaskIntoConstraints = false;
        Orientation = NSUserInterfaceLayoutOrientation.Vertical;
    }

    public override void ConstrainElement(NSView element) {
        AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-10-[element]-10-|", NSLayoutFormatOptions.AlignAllTop, null, NSDictionary.FromObjectAndKey(element, new NSString("element"))));
    }
}