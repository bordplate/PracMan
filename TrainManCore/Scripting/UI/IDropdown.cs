using NLua;

namespace TrainManCore.Scripting.UI;

public interface IDropdown {
    public void SetItems(LuaTable items);
    
    public void SetSelectedIndex(int index);
}