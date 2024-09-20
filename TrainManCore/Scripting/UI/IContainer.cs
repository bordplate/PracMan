namespace TrainManCore.Scripting.UI;

using NLua;

public interface IContainer {
    public IContainer AddRow(LuaFunction callback);
    public ILabel AddLabel(string text);
    public IButton AddButton(string title, LuaFunction callback);
    public ITextField AddTextField(LuaFunction callback);
    public IDropdown AddDropdown(LuaTable items, LuaFunction callback);
    public ICheckbox AddCheckbox(string text, LuaFunction callback);
}