using NLua;

namespace PracManCore.Scripting.UI;

public interface IDropdown: IControl {
    public IWindow Window { get; }
    public void SetItems(LuaTable items);
    
    public void SetSelectedIndex(int index);
}