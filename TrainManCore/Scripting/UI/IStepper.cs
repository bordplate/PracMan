namespace TrainManCore.Scripting.UI;

public interface IStepper {
    public IWindow Window { get; }
    public void SetValue(int value);
}