using NLua;
using PracManCore.Scripting.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracMinWin.ModuleUI;
public class Button : Microsoft.UI.Xaml.Controls.Button, IButton {
    public IWindow Window { get; }
    public LuaFunction Callback { get; set; }

    public string Title { 
        get {
            return (string)Content;
        }
        set {
            Content = value;
        }
    }

    public Button(IWindow window, LuaFunction callback, string title) {
        Window = window;
        Title = title;
        Callback = callback;

        Click += Button_Click;
    }

    private void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e) {
        ((IButton)this).Activate();
    }
}
