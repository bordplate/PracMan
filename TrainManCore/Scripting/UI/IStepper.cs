namespace TrainManCore.Scripting.UI;

public interface IStepper: IControl {
    public IWindow Window { get; }
    public void SetValue(int value);
}