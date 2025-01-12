using NLua;
using Microsoft.UI.Xaml.Controls;
using PracManCore.Scripting.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace PracMinWin.ModuleUI;
public class Checkbox : StackPanel, ICheckbox {
    public IWindow Window { get; }

    LuaFunction _callback;

    private CheckBox _checkBox;

    public bool Checked {
        get => IsChecked();
        set => SetChecked(value);
    }

    public Checkbox(IWindow window, string text, LuaFunction callback) {
        Window = window;

        _checkBox = new CheckBox();
        _checkBox.Content = text;

        Children.Add(_checkBox);

        _callback = callback;

        _checkBox.Checked += Checkbox_Checked;
        _checkBox.Unchecked += Checkbox_Checked;
    }

    public void SetChecked(bool isChecked, bool callingCallback = false) {
       _checkBox.IsChecked = isChecked;
    }

    public bool IsChecked() {
        return _checkBox.IsChecked ?? false;
    }

    public void Checkbox_Checked(object sender, RoutedEventArgs e) {
        _callback.Call(IsChecked());
    }
}
