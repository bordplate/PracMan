using Microsoft.UI.Xaml.Controls;
using PracManCore.Scripting.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracMinWin.ModuleUI;
public class Spacer: StackPanel, ISpacer {
    public IWindow Window { get; }

    public Spacer(IWindow window) {
        Window = window;

        Margin = new Microsoft.UI.Xaml.Thickness(5);
    }
}
