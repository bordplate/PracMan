using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PracManCore.Scripting.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracMinWin.ModuleUI;
public class Label : StackPanel, ILabel {
    public IWindow Window { get; }

    private TextBlock _label;

    public Label(IWindow window) {
        Window = window;

        _label = new TextBlock();

        Children.Add(_label);
    }

    public void SetText(string text) {
        _label.Text = text;
    }
}
