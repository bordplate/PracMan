using NLua;

namespace TrainManCore.Scripting.UI;

public interface IMenu {
    public string Title { get; set; }
    
    public void AddSeparator();
    public IMenuItem AddItem(string title, Action callback);
    public IMenuItem AddCheckItem(string title, LuaFunction callback);
}