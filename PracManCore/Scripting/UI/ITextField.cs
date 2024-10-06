namespace PracManCore.Scripting.UI;

public interface ITextField: IControl {
    public IWindow Window { get; }
    public string GetText();
}