namespace PracManCore.Scripting.UI;

public interface ICheckbox: IControl {
    public IWindow Window { get; }

    public bool Checked {
        get => IsChecked();
        set => SetChecked(value);
    }

    public void SetChecked(bool isChecked, bool callingCallback = false);
    public bool IsChecked();
}