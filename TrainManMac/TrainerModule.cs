using TrainManCore.Scripting.UI;

namespace TrainMan;

public class TrainerModule: ITrainer {
    public IWindow CreateWindow() {
        return new TrainerViewController();
    }

    public void Alert(string text) {
        var alert = new NSAlert {
            AlertStyle = NSAlertStyle.Informational,
            InformativeText = text
        };
        
        alert.RunModal();
    }
}