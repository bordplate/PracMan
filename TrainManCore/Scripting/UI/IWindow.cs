namespace TrainManCore.Scripting.UI;

using NLua;

public interface IWindow {
    public string ClassName { get; }
    public event Action? OnWindowLoaded;
    public void SetLuaContext(LuaTable luaContext);
    public void SetTitle(string title);
    public void Show();
    public void Close();
    public IContainer AddColumn();
    
    public IButton? GetButton(string title);
}