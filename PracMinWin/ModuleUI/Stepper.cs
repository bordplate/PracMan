using NLua;
using Microsoft.UI.Xaml.Controls;
using PracManCore.Scripting.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracMinWin.ModuleUI;
public class Stepper : TextBox, IStepper {
    public IWindow Window { get; }

    LuaFunction _luaCallback;

    public Stepper(IWindow window, long minValue, long maxValue, int step, LuaFunction callback) {
        Window = window;
        _luaCallback = callback;
    }

    public int GetValue() {
        return 0;
    }

    public void SetValue(int value) {
        
    }
}
