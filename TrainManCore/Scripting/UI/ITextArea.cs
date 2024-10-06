namespace TrainManCore.Scripting.UI;

public interface ITextArea: IControl {
    public IWindow Window { get; }
    public int Rows { get; }
    public string GetText();
    public void SetText(string text);
    public void SetMonospaced(bool monospaced);
}