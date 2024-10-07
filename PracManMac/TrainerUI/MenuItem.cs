using NLua;
using NLua.Exceptions;
using PracManCore.Scripting.UI;

namespace PracManMac.TrainerUI;

public class MenuItem: NSMenuItem, IMenuItem {
    public IWindow Window { get; }
    
    private Action? _callback;
    private Action<bool>? _checkCallback;
    public bool IsCheckable { get; set; }

    public bool Checked {
        get { return State == NSCellStateValue.On; }
        set {
            State = value ? NSCellStateValue.On : NSCellStateValue.Off;
        }
    }

    public MenuItem(IWindow window, string title, Action callback) {
        Window = window;
        
        _callback = callback;
        IsCheckable = false;
        
        Title = title;
        Action = new ObjCRuntime.Selector("menuAction:");
        Target = this;
    }
    
    public MenuItem(IWindow window, string title, Action<bool> callback) {
        Window = window;
        _checkCallback = callback;
        IsCheckable = true;
        
        Title = title;
        Action = new ObjCRuntime.Selector("menuAction:");
        Target = this;
    }
    
    [Export("menuAction:")]
    private void MenuAction(NSObject sender) {
        if (IsCheckable) {
            State = State == NSCellStateValue.On ? NSCellStateValue.Off : NSCellStateValue.On;

            try {
                _checkCallback?.Invoke(State == NSCellStateValue.On);
            } catch (LuaScriptException exception) {
                Console.Error.WriteLine(exception.Message);
                new NSAlert {
                    AlertStyle = NSAlertStyle.Critical,
                    InformativeText = exception.Message,
                    MessageText = "Error",
                }.RunModal();
            }

            return;
        }

        try {
            _callback?.Invoke();
        } catch (LuaScriptException exception) {
            Console.Error.WriteLine(exception.Message);
            new NSAlert {
                AlertStyle = NSAlertStyle.Critical,
                InformativeText = exception.Message,
                MessageText = "Error",
            }.RunSheetModal(((ModuleWindowViewController)Window).Window);
        }
    }
}