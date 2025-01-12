using PracManCore.Scripting.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracMinWin.ModuleUI;
public class Menu : IMenu {
    public IWindow Window { get; }
    public string Title { get; set; }

    public Menu(IWindow? window, string title) {
        Window = window!;
        Title = title;
    }

    public IMenuItem AddCheckItem(string title, Action<bool> callback) {
        throw new NotImplementedException();
    }

    public IMenuItem AddItem(string title, Action callback) {
        throw new NotImplementedException();
    }

    public void AddSeparator() {
        throw new NotImplementedException();
    }

    public void OnOpen(Action callback) {
        throw new NotImplementedException();
    }
}
