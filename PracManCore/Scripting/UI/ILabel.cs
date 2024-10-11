namespace PracManCore.Scripting.UI;

public interface ILabel: IControl {
    public IWindow Window { get; }
    public void SetText(string text);
}