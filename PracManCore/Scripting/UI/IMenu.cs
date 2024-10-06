using NLua;

namespace PracManCore.Scripting.UI;

public interface IMenu: IControl {
    public string Title { get; set; }
    public IWindow Window { get; }
    public void AddSeparator();
    public IMenuItem AddItem(string title, Action callback);
    public IMenuItem AddCheckItem(string title, Action<bool> callback);
    public void OnOpen(Action callback);
}