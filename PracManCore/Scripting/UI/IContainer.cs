namespace PracManCore.Scripting.UI;

using NLua;

public interface IContainer: IControl {
    public IWindow Window { get; set; }
    public IContainer AddRow(LuaFunction? callback = null);
    public IContainer AddColumn(LuaFunction? callback = null);
    public ISpacer AddSpacer();
    public ILabel AddLabel(string text);
    public IButton AddButton(string title, LuaFunction callback);
    public ITextField AddTextField(LuaFunction callback);
    public ITextArea AddTextArea(int rows);
    public IStepper AddStepper(long minValue, long maxValue, int step, LuaFunction callback);
    public IDropdown AddDropdown(LuaTable items, LuaFunction callback);
    public ICheckbox AddCheckbox(string text, LuaFunction callback);
}