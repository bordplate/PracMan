using NLua;
using NLua.Exceptions;
using TrainMan.TrainerUI;
using TrainManCore.Scripting;
using TrainManCore.Scripting.Exceptions;
using TrainManCore.Scripting.UI;

namespace TrainMan;

public class ModuleWindowViewController: NSViewController, IWindow {
    public event Action<IWindow>? OnLoad;
    public string ClassName { get; }
    public NSWindow Window;

    LuaTable _luaContext;

    private bool _viewLoaded = false;
    private bool _isMainWindow = false;

    private NSStackView _stackView;

    private Module _module;

    private NSMenu? _menu;
    private List<Button> _buttons = [];
    
    public ModuleWindowViewController(bool isMainWindow, LuaTable luaContext, Module module) : base() {
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
        
        _stackView = new NSStackView {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Orientation = NSUserInterfaceLayoutOrientation.Horizontal,
            Distribution = NSStackViewDistribution.FillEqually,
            Spacing = 0,
        };
        
        Window = new NSWindow(
            CGRect.Empty, 
            NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable, 
            NSBackingStore.Buffered, 
            true) {
            Title = "",
        };
        
        Window.Center();
        
        Window.DidBecomeKey += (sender, e) => {
            if (_isMainWindow && _module.Delegate is ModuleDelegate trainerModule) {
                trainerModule.ActivateMenu();
            }
        };
    }

    public bool Load() {
        // Settings ContentViewController starts the view loading process and class ViewDidLoad
        Window.ContentViewController = this;
        
        try {
            (_luaContext["OnLoad"] as LuaFunction)?.Call([_luaContext]);
        } catch (LuaScriptException exception) {
            new NSAlert {
                AlertStyle = NSAlertStyle.Critical,
                InformativeText = exception.Message,
                MessageText = "Error",
            }.RunSheetModal(Window);

            return false;
        }
        
        OnLoad?.Invoke(this);

        return true;
    }
    
    public override void ViewDidLoad() {
        base.ViewDidLoad();
        
        View.SetFrameSize(new CGSize(100, 100));
        
        View.AddSubview(_stackView);
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[stackView]|", NSLayoutFormatOptions.None, null, new NSDictionary("stackView", _stackView)));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[stackView]|", NSLayoutFormatOptions.None, null, new NSDictionary("stackView", _stackView)));

        _viewLoaded = true;
    }
    
    public override void ViewDidDisappear() {
        base.ViewDidDisappear();

        if (_isMainWindow) {
            _module.Exit();
        }
    }

    public void Show() {
        if (!_viewLoaded && !Load()) {
            new NSAlert {
                AlertStyle = NSAlertStyle.Critical,
                InformativeText = $"Failed to load window for {ClassName}.",
                MessageText = "Error",
            }.RunSheetModal(Window);
            
            return;
        }
        
        Window.MakeKeyAndOrderFront(null);
    }

    public void Close() {
        Window.Close();
    }
    
    public void SetTitle(string title) {
        if (!_viewLoaded) {
            new NSAlert {
                AlertStyle = NSAlertStyle.Critical,
                InformativeText = "View must be loaded before calling `SetTitle`.",
                MessageText = "Error",
            }.RunSheetModal(Window);

            return;
        }
        
        Window.Title = title;
    }


    public IContainer? AddColumn() {
        if (!_viewLoaded) {
            new NSAlert {
                AlertStyle = NSAlertStyle.Critical,
                InformativeText = "View must be loaded before calling `AddColumn`.",
                MessageText = "Error",
            }.RunSheetModal(Window);

            return null;
        }
        
        var column = new Column(this);
        
        _stackView.AddArrangedSubview(column);
        
        _stackView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-10-[column]-10-|", NSLayoutFormatOptions.None, null, new NSDictionary("column", column)));
        column.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[column(>=250)]", NSLayoutFormatOptions.None, null, new NSDictionary("column", column)));

        return column;
    }
    
    public IButton? GetButton(string title) {
        foreach (var button in _buttons) {
            if (button.Title == title) {
                return button;
            }
        }

        return null;
    }
    
    public void RegisterButton(Button button) {
        _buttons.Add(button);
    }
}