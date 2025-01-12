using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using NLua;
using PracManCore.Scripting.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracMinWin.ModuleUI;
public class Container : Grid, IContainer {
    public IWindow Window { get; set; }

    public Container(IWindow window) {
        Window = window;
        // Default spacing for rows and columns
        RowSpacing = 5;
        ColumnSpacing = 5;
    }

    public IButton AddButton(string title, LuaFunction callback) {
        var button = new Button(Window, callback, title) {
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };

        AddChildToGrid(button);

        return button;
    }

    public ICheckbox AddCheckbox(string text, LuaFunction callback) {
        var checkbox = new Checkbox(Window, text, callback);

        AddChildToGrid(checkbox);

        return checkbox;
    }

    public IContainer AddColumn(LuaFunction? callback = null) {
        var columnContainer = new Container(Window) {
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        AddChildToGrid(columnContainer);

        callback?.Call([columnContainer]);

        return columnContainer;
    }

    public IDropdown AddDropdown(LuaTable items, LuaFunction callback) {
        var dropdown = new Dropdown(Window, items, callback);

        AddChildToGrid(dropdown);

        return dropdown;
    }

    public ILabel AddLabel(string text) {
        var label = new Label(Window);
        label.SetText(text);

        AddChildToGrid(label);

        return label;
    }

    public IContainer AddRow(LuaFunction? callback = null) {
        var rowContainer = new Container(Window) {
            Orientation = Orientation.Horizontal,
        };

        AddChildToGrid(rowContainer);

        callback?.Call([rowContainer]);

        return rowContainer;
    }

    public ISpacer AddSpacer() {
        var spacer = new Spacer(Window);

        AddChildToGrid(spacer);

        return spacer;
    }

    public IStepper AddStepper(long minValue, long maxValue, int step, LuaFunction callback) {
        var stepper = new Stepper(Window, minValue, maxValue, step, callback);

        AddChildToGrid(stepper);

        return stepper;
    }

    public ITextArea AddTextArea(int rows) {
        var textArea = new TextArea(Window, rows);

        AddChildToGrid(textArea);

        return textArea;
    }

    public ITextField AddTextField(LuaFunction callback) {
        var textField = new TextField(Window, callback);

        AddChildToGrid(textField);

        return textField;
    }

    // Helper method to add child elements to the Grid
    private void AddChildToGrid(FrameworkElement element) {
        if (Orientation == Orientation.Vertical) {
            // Add a new row for vertical orientation with Auto height
            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Place the element in the new row
            Grid.SetRow(element, RowDefinitions.Count - 1);
            Grid.SetColumn(element, 0);
        } else if (Orientation == Orientation.Horizontal) {
            // Add a new column with star sizing for even distribution
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Place the element in the new column
            Grid.SetColumn(element, ColumnDefinitions.Count - 1);
            Grid.SetRow(element, 0);
        }

        // Add the element to the grid
        Children.Add(element);
    }

    public Orientation Orientation { get; set; } = Orientation.Vertical;
}