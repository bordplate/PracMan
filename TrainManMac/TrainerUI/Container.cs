using NLua;
using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public abstract class Container(IWindow window) : NSStackView, IContainer {
    public IWindow Window { get; set; } = window;
    public abstract void ConstrainElement(NSView element);
    
    public ILabel AddLabel(string text) {
        var label = new Label(Window, text);
        AddArrangedSubview(label);
        
        ConstrainElement(label);
        
        return label;
    }
    
    public ITextField AddTextField(LuaFunction callback) {
        var textField = new TextField(Window, callback);
        AddArrangedSubview(textField);
        
        ConstrainElement(textField);
        
        return textField;
    }

    public IButton AddButton(string title, LuaFunction callback) {
        var button = new Button(Window, title, callback);
        AddArrangedSubview(button);

        ConstrainElement(button);
        
        ((TrainerViewController)Window).RegisterButton(button);
        
        return button;
    }
    
    public IDropdown AddDropdown(LuaTable options, LuaFunction callback) {
        var dropdown = new Dropdown(Window, options, callback);
        AddArrangedSubview(dropdown);
        
        ConstrainElement(dropdown);
        
        return dropdown;
    }
    
    public ICheckbox AddCheckbox(string text, LuaFunction callback) {
        var checkbox = new Checkbox(Window, text, callback);
        AddArrangedSubview(checkbox);
        
        ConstrainElement(checkbox);
        
        return checkbox;
    }

    public IContainer AddRow(LuaFunction callback) {
        var row = new Row(Window);
        AddArrangedSubview(row);
        
        ConstrainElement(row);
        
        TrainerModule.TryInvoke(window, callback, row);
        
        return row;
    }
    
    public IContainer AddColumn(LuaFunction callback) {
        var column = new Column(Window);
        AddArrangedSubview(column);
        
        ConstrainElement(column);
        
        // Make column fill from top to bottom
        AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-10-[column]-10-|", NSLayoutFormatOptions.None, 
            null, new NSDictionary("column", column)));
        
        TrainerModule.TryInvoke(window, callback, column);
        
        return column;
    }
    
    public ISpacer AddSpacer() {
        var spacer = new Spacer(Window);
        AddArrangedSubview(spacer);
        
        ConstrainElement(spacer);
        
        return spacer;
    }
    
    public IStepper AddStepper(int minValue, int maxValue, int step, LuaFunction callback) {
        var stepper = new Stepper(Window, minValue, maxValue, step, callback);
        AddArrangedSubview(stepper);
        
        ConstrainElement(stepper);
        
        return stepper;
    }
}