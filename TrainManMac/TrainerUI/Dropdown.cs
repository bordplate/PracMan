using NLua;
using NLua.Exceptions;
using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public class Dropdown: NSPopUpButton, IDropdown {
    public IWindow Window { get; }
    private LuaFunction _callback;
    
    Dictionary<int, string> _items = new ();
    
    public Dropdown(IWindow window, LuaTable items, LuaFunction callback) {
        Window = window;
        
        _callback = callback;
        _items = GetItemsFromLuaTable(items);
        
        PullsDown = true;
        
        SetItems(items);
        
        SetTitle(_items.Values.First());
        
        Target = this;
        Action = new ObjCRuntime.Selector("dropdownDidChange:");

        try {
            _callback.Call(1, _items.Values.First());
        } catch (LuaScriptException exception) {
            new NSAlert {
                AlertStyle = NSAlertStyle.Critical,
                InformativeText = exception.Message,
                MessageText = "Error",
            }.RunSheetModal(((ModuleWindowViewController)Window).Window);
        }
    }

    public Dictionary<int, string> GetItemsFromLuaTable(LuaTable options) {
        var items = new Dictionary<int, string>();
        foreach (var key in options.Keys) {
            var item = options[key];

            if (item is LuaTable table) {
                if (table[1] is long index && table[2] is string value) {
                    items.Add((int)index, value);                    
                }
            } else {
                items.Add((int)(long)key, (string)options[key]);
            }
        }

        return items;
    }

    public void SetItems(LuaTable items) {
        _items = GetItemsFromLuaTable(items);
        
        RemoveAllItems();
        
        AddItem("");
        
        foreach (var item in _items) {
            AddItem(item.Value);
        }
        
        SetTitle(_items.Values.FirstOrDefault() ?? "");
    }
    
    public void SetSelectedIndex(int index) {
        if (index < 0 || index >= _items.Count || !_items.ContainsKey(index)) {
            SetTitle("");
            return;
        }
        
        SetTitle(_items[index]);

        try {
            _callback.Call(index, _items[index]);
        } catch (LuaScriptException exception) {
            new NSAlert {
                AlertStyle = NSAlertStyle.Critical,
                InformativeText = exception.Message,
                MessageText = "Error",
            }.RunSheetModal(((ModuleWindowViewController)Window).Window);
        }
    }
    
    [Export("dropdownDidChange:")]
    public void DropdownDidChange(NSObject sender) {
        var keys = _items.Keys.ToArray();
        var values = _items.Values.ToArray();

        try {
            _callback.Call(keys[IndexOfSelectedItem - 1], values[IndexOfSelectedItem - 1]);
        } catch (LuaScriptException exception) {
            new NSAlert {
                AlertStyle = NSAlertStyle.Critical,
                InformativeText = exception.Message,
                MessageText = "Error",
            }.RunSheetModal(((ModuleWindowViewController)Window).Window);
        }

        SetTitle(values[IndexOfSelectedItem-1]);
    }
}