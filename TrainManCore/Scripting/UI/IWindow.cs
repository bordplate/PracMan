namespace TrainManCore.Scripting.UI;

using NLua;

public interface IWindow {
    public event Action<IWindow>? OnLoad;
    public string ClassName { get; }
    public bool Load();
    public void SetTitle(string title);
    public void Show();
    public void Close();
    public IContainer? AddColumn();
    public IButton? GetButton(string title);
}