namespace TrainManCore.Scripting.UI;

using NLua;

public interface IWindow {
    public void SetLuaContext(LuaTable luaContext);

    public void SetTitle(string title);
    
    public void Show();
    public void Close();
    
    public IContainer AddColumn();
}