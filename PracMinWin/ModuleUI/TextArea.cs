using Microsoft.UI.Xaml.Controls;
using PracManCore.Scripting.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracMinWin.ModuleUI;
public class TextArea : TextBox, ITextArea {
    public IWindow Window { get; }

    public int Rows { get; set; } = 1;

    public TextArea(IWindow window, int rows) {
        Window = window;
        Rows = rows;
    }

    public string GetText() {
        return Text;
    }

    public void SetMonospaced(bool monospaced) {
        
    }

    public void SetText(string text) {
        Text = text;
    }
}
