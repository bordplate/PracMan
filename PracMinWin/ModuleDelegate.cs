using NLua;
using PracManCore.Scripting;
using PracManCore.Scripting.UI;
using PracMinWin.ModuleUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRT.PracMinWinVtableClasses;

namespace PracMinWin;

public class ModuleDelegate : IModule {
    public List<IWindow> Windows { get; set; } = [];

    private IWindow? _mainWindow = null;

    public IMenu AddMenu(string title, Action<IMenu> callback) {
        var newMenu = new Menu(_mainWindow, title);

        // TODO: Implement callback and stuff

        return newMenu;
    }

    public void CloseAllWindows() {
        throw new NotImplementedException("IModule.CloseAllWindows");
    }

    public IWindow CreateWindow(Module module, LuaTable luaObject, bool isMainWindow = false) {
        if (isMainWindow && _mainWindow != null) {
            throw new Exception("Main window already created.");
        }

        var window = new ModuleWindow(isMainWindow, luaObject, module);

        if (isMainWindow) {
            _mainWindow = window;
        }

        Windows.Add(window);

        return window;
    }
}
