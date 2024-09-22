using NLua;
using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public abstract class Container: NSStackView, IContainer {
    public abstract void ConstrainElement(NSView element);
    
    public ILabel AddLabel(string text) {
        var label = new Label(text);
        AddArrangedSubview(label);
        
        ConstrainElement(label);
        
        return label;
    }
    
    public ITextField AddTextField(LuaFunction callback) {
        var textField = new TextField(callback);
        AddArrangedSubview(textField);
        
        ConstrainElement(textField);
        
        return textField;
    }

    public IButton AddButton(string title, LuaFunction callback) {
        var button = new Button(title, callback);
        AddArrangedSubview(button);
        
        ConstrainElement(button);
        
        return button;
    }
    
    public IDropdown AddDropdown(LuaTable options, LuaFunction callback) {
        // Convert LuaTable to List<string>
        var dropdown = new Dropdown(options, callback);
        AddArrangedSubview(dropdown);
        
        ConstrainElement(dropdown);
        
        return dropdown;
    }
    
    public ICheckbox AddCheckbox(string text, LuaFunction callback) {
        var checkbox = new Checkbox(text, callback);
        AddArrangedSubview(checkbox);
        
        ConstrainElement(checkbox);
        
        return checkbox;
    }

    public IContainer AddRow(LuaFunction callback) {
        var row = new Row();
        AddArrangedSubview(row);
        
        ConstrainElement(row);
        
        callback.Call(row);
        
        return row;
    }
    
    public IContainer AddColumn(LuaFunction callback) {
        var column = new Column();
        AddArrangedSubview(column);
        
        ConstrainElement(column);
        
        // Make column fill from top to bottom
        AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-10-[column]-10-|", NSLayoutFormatOptions.None, null, new NSDictionary("column", column)));
        
        callback.Call(column);
        
        return column;
    }
    
    public ISpacer AddSpacer() {
        var spacer = new Spacer();
        AddArrangedSubview(spacer);
        
        ConstrainElement(spacer);
        
        return spacer;
    }
    
    public IStepper AddStepper(int minValue, int maxValue, int step, LuaFunction callback) {
        var stepper = new Stepper(minValue, maxValue, step, callback);
        AddArrangedSubview(stepper);
        
        ConstrainElement(stepper);
        
        return stepper;
    }
}