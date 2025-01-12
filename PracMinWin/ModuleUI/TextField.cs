using NLua;
using Microsoft.UI.Xaml.Controls;
using PracManCore.Scripting.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracMinWin.ModuleUI;
public class TextField : TextBox, ITextField {
    public IWindow Window { get; }

    LuaFunction _callback;

    public TextField(IWindow window, LuaFunction callback) {
        Window = window;
        _callback = callback;
    }

    public string GetText() {
        return Text;
    }
}
