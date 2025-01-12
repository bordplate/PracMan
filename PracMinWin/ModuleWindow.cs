using Microsoft.UI.Xaml;
using NLua;
using PracManCore.Scripting.Exceptions;
using PracManCore.Scripting;
using PracManCore.Scripting.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Module = PracManCore.Scripting.Module;
using Windows.Devices.SmartCards;
using Microsoft.UI.Xaml.Controls;
using NLua.Exceptions;
using System.ComponentModel;
using IContainer = PracManCore.Scripting.UI.IContainer;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using WinUIEx;
using Windows.UI.Composition;
using Windows.UI;
using System.Numerics;
using Microsoft.UI.Windowing;

namespace PracMinWin;

public class BlurredBackdrop : CompositionBrushBackdrop {
    protected override Windows.UI.Composition.CompositionBrush CreateBrush(Windows.UI.Composition.Compositor compositor)
        => compositor.CreateHostBackdropBrush();
}

public class ModuleWindow : Window, IWindow {
    public string ClassName { get; }
    public event Action<IWindow>? OnLoad;

    LuaTable _luaContext;
    private Module _module;
    private bool _isMainWindow;

    private IContainer _content;

    public ModuleWindow(bool isMainWindow, LuaTable luaContext, Module module) {
        _luaContext = luaContext;

        if (luaContext["class"] is not LuaTable classTable) {
            throw new ScriptException("Invalid class table.");
        }

        if (classTable["name"] is not string className) {
            throw new ScriptException("No class name.");
        }

        if (classTable["OnLoad"] is not LuaFunction) {
            throw new ScriptException($"Class `{className}` must have `OnLoad` function.");
        }

        luaContext["native_window"] = this;

        ClassName = className;

        _module = module;
        _isMainWindow = isMainWindow;

        _content = new ModuleUI.Container(this) {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Width = double.NaN,
            Height = double.NaN,
        };

        SystemBackdrop = new DesktopAcrylicBackdrop();

        Content = (UIElement)_content;

        App.ConfigureTitleBar(this);
    }

    public IContainer? AddColumn() {
        return _content.AddColumn(null);
    }

    public IButton? GetButton(string title) {
        throw new NotImplementedException();
    }

    public bool Load() {
        try {
            (_luaContext["OnLoad"] as LuaFunction)?.Call([_luaContext]);
        } catch (LuaScriptException exception) {
            ShowAlert("Error", exception.Message);
            return false;
        }

        OnLoad?.Invoke(this);

        return true;
    }

    public void SetTitle(string title) {
        Title = title;
    }

    public void Show() {
        if (!Load()) {
            ShowAlert("Error", $"Failed to load window for `{ClassName}`.");
            return;
        }

        Activate();
    }

    private void ShowAlert(string title, string message) {
        var dlg = new ContentDialog {
            Title = title,
            Content = message,
            PrimaryButtonText = "OK",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.Content.XamlRoot
        };
        _ = dlg.ShowAsync();
    }
}
