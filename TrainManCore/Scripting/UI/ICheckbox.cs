namespace TrainManCore.Scripting.UI;

public interface ICheckbox {
    public IWindow Window { get; }
    public void SetChecked(bool isChecked);
    public bool IsChecked();
}