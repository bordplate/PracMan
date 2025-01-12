using Microsoft.UI.Xaml.Controls;
using NLua;
using PracManCore.Scripting.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracMinWin.ModuleUI;
public class Dropdown : ComboBox, IDropdown {
    public IWindow Window { get; }

    LuaFunction _luaFunction;
    public Dropdown(IWindow window, LuaTable options, LuaFunction luaFunction) {
        Window = window;
        _luaFunction = luaFunction;

        HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Stretch;
        Width = double.NaN;

        SetItems(options);

        SelectionChanged += (sender, args) => {
            SetSelectedIndex(SelectedIndex, true);
        };
    }

    public void SetItems(LuaTable items) {
        var itemsDict = GetItemsFromLuaTable(items);

        foreach (var item in itemsDict) {
            Items.Add(item.Value);
        }

        SelectedIndex = 0;
    }

    public void SetSelectedIndex(int index, bool callingCallback = false) {
        SelectedIndex = index;

        if (callingCallback) {
            _luaFunction.Call(index);
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
                var itemValue = options[key];
                if (itemValue != null) {
                    items.Add((int)(long)key, itemValue.ToString()!);
                }
            }
        }

        return items;
    }

}
