namespace PracManCore.Scripting.UI;

using NLua;

public interface IContainer: IControl {
    public IWindow Window { get; set; }
    public IContainer AddRow(LuaFunction callback);
    public IContainer AddColumn(LuaFunction callback);
    public ISpacer AddSpacer();
    public ILabel AddLabel(string text);
    public IButton AddButton(string title, LuaFunction callback);
    public ITextField AddTextField(LuaFunction callback);
    public ITextArea AddTextArea(int rows);
    public IStepper AddStepper(int minValue, int maxValue, int step, LuaFunction callback);
    public IDropdown AddDropdown(LuaTable items, LuaFunction callback);
    public ICheckbox AddCheckbox(string text, LuaFunction callback);
}