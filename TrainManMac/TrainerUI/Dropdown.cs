using NLua;
using TrainManCore.Scripting.UI;

namespace TrainMan.TrainerUI;

public class Dropdown: NSPopUpButton, IDropdown {
    private LuaFunction _callback;
    
    Dictionary<int, string> _items = new ();
    
    public Dropdown(LuaTable items, LuaFunction callback) {
        _callback = callback;
        _items = GetItemsFromLuaTable(items);
        
        PullsDown = true;
        
        SetItems(items);
        
        SetTitle(_items.Values.First());
        
        Target = this;
        Action = new ObjCRuntime.Selector("dropdownDidChange:");
        
        _callback.Call(1, _items.Values.First());
    }

    public Dictionary<int, string> GetItemsFromLuaTable(LuaTable options) {
        var items = new Dictionary<int, string>();
        foreach (var key in options.Keys) {
            var item = options[key];

            if (item is LuaTable table) {
                var index = table[1] as long?;
                var value = table[2] as string;
                
                items.Add((int)index, value);
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
        
        SetTitle(_items.Values.First());
    }
    
    public void SetSelectedIndex(int index) {
        if (index < 0 || index >= _items.Count || !_items.ContainsKey(index)) {
            SetTitle("");
            return;
        }
        
        SetTitle(_items[index]);
        
        _callback.Call(index, _items[index]);
    }
    
    [Export("dropdownDidChange:")]
    public void DropdownDidChange(NSObject sender) {
        var keys = _items.Keys.ToArray();
        var values = _items.Values.ToArray();

        _callback.Call(keys[IndexOfSelectedItem-1], values[IndexOfSelectedItem-1]);
        
        SetTitle(values[IndexOfSelectedItem-1]);
    }
}