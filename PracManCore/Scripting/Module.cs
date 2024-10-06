using System.Globalization;
using Nett;
using Nett.Parser;
using NLua;
using NLua.Exceptions;
using PracManCore.Scripting.Exceptions;
using PracManCore.Scripting.UI;
using PracManCore.Target;

namespace PracManCore.Scripting;

public class Module(string title, string path) {
    public event Action? OnExit;
    
    public readonly string Title = title;
    public readonly string Path = path;
    public readonly string Identifier = System.IO.Path.GetFileName(path);
    public bool IsLoaded;
    public readonly Settings Settings = new(System.IO.Path.Combine(path, "config.toml"));

    private Target.Target? _target;
    private Inputs? _inputs;
    
    private Settings? _userSettings;

    private readonly Lua _state = new();

    public IModule? Delegate;

    public void Load(Target.Target target) {
        _target = target;

        _userSettings = GetUserSettings();
        if (_userSettings == null) {
            Exit();
            return;
        }
        
        Application.Delegate?.OnModuleLoad(this, _target);
        
        var patches = Settings.GetTableForSection("Patches");
        if (patches != null) {
            foreach (var addressString in patches.Keys) {
                var address = Convert.ToUInt32(addressString, 16);
                
                if (patches[addressString].TomlType == TomlObjectType.Int) {
                    var value = patches.Get<int>(addressString);
                    
                    _target.WriteMemory(address, (uint)value);
                }

                if (patches[addressString].TomlType == TomlObjectType.String) {
                    // This is a binary file that we need to load and write to memory
                    var filename = patches.Get<string>(addressString);
                    var bytes = File.ReadAllBytes(System.IO.Path.Combine(Path, filename));
                    
                    // Write 1024 bytes at a time
                    for (var i = 0; i < bytes.Length; i += 1024) {
                        var length = Math.Min(1024, bytes.Length - i);
                        var chunk = new byte[length];
                        Array.Copy(bytes, i, chunk, 0, length);

                        _target.WriteMemory(address + (uint)i, (uint)length, chunk);
                    }
                }
            }
        }
        
        try {
            SetupState(_state);
        } catch (ParseException exception) {
            Exit();
            return;
        }
        
        var entry = Settings.Get<string>("General.entry");
        
        if (entry != null) {
            var entryFile = File.ReadAllText(System.IO.Path.Combine(Path, entry));
        
            if (entryFile == null) {
                throw new ScriptException($"Entry point `{entry}` specified in module config, but the file was not found.");
            }

            try {
                _state.DoString(entryFile, entry);
                (_state["OnLoad"] as LuaFunction)?.Call();
            } catch (LuaScriptException exception) {
                Exit();
                throw new ScriptException(exception.Message);
            }
        }
        
        _target.Modules.Add(this);
        IsLoaded = true;
    }
    
    public void Exit() {
        IsLoaded = false;
        if (_target != null && _target.Modules.Contains(this)) _target.Modules.Remove(this);
        
        (_state["OnUnload"] as LuaFunction)?.Call();
        
        Delegate?.CloseAllWindows();

        OnExit?.Invoke();
    }

    private void SetupState(Lua state) {
        if (_target == null) {
            throw new InvalidOperationException("Module has not been loaded before trying to setup Lua state.");
        }
        
        state.UseTraceback = true;
        state.LoadCLRPackage();
        
        if (Delegate == null) {
            throw new ScriptException("Module Delegate not set.");
        }
        
        // Set package path to the runtime folder and the module's folder
        state.DoString($"package.path = package.path .. ';{Application.GetModulesRoot()}/Runtime/?.lua;{Path}/?.lua'", "set package path");
        
        var functions = new LuaFunctions(_target);
        foreach (var (key, value) in functions.Functions) {
            state[key] = value;
        }
        
        state["Module"] = this;
        state["print"] = (string text) => {
            Console.WriteLine($"[{Identifier}] {text}");
        };
        state["Exit"] = Exit;

        state["AddMenu"] = Delegate.AddMenu;

        state["Alert"] = (string text) => {
            if (_target.CanInlineNotify()) {
                _target.Notify(text);
            } else {
                Application.Delegate?.Alert(Settings.Get("General.name", Identifier)!, text);
            }
        };

        state["Execute"] = Run;
        state["LoadFileFromDialog"] = Application.Delegate!.LoadFileFromDialog;
        
        state["Settings"] = _userSettings;

        state["UINT_MAX"] = uint.MaxValue;
        state["INT_MAX"] = int.MaxValue;

        state["Target"] = _target;
        
        state["LoadModule"] = (string title, string moduleName) => {
            Application.LoadModule(_target, title, moduleName);
        };
        state["SetTitleID"] = (string titleId) => {
            _target.TitleId = titleId;
        };
        
        state.DoString(File.ReadAllText(System.IO.Path.Combine(Application.GetModulesRoot(), "Runtime/runtime.lua")), "runtime.lua");
        
        // If the title exists in Runtime/Titles/{Title}, load and run all the Lua files in that directory
        var titlePath = System.IO.Path.Combine(Application.GetModulesRoot(), "Runtime/Titles", Title);
        if (Directory.Exists(titlePath)) {
            foreach (var file in Directory.GetFiles(titlePath, "*.lua")) {
                state.DoString(File.ReadAllText(file), file);
            }
        }
    }

    public void Run(string code) {
        try {
            _state.DoString(code, "Dynamic code");
        } catch (LuaScriptException exception) {
            Console.Error.WriteLine(exception.Message);
        }
    }

    public IWindow CreateWindow(LuaTable luaObject, bool isMainWindow = false) {
        if (luaObject["class"] is not LuaTable luaClass) {
            throw new ScriptException("No class found in object passed to `CreateWindow`.");
        }
        
        if (luaClass["name"] is not string) {
            throw new ScriptException($"`name` not found in class passed to `CreateWindow`.");
        }

        var window = Delegate!.CreateWindow(this, luaObject, isMainWindow);
        window.OnLoad += (window) => {
            _inputs?.BindButtonCombos(window);
        };
        
        return window;
    }
    
    private Settings? GetUserSettings() {
        Settings? userSettings = null;
        
        try {
            userSettings = new Settings(System.IO.Path.Combine(Path, "settings.user.toml"), true);
        } catch (ParseException exception) {
            Application.Delegate?.ConfirmDialog("Delete settings file?", $"Your settings for {Settings.Get("General.name", Identifier)} could not be parsed. Do you want to reset these settings?", (confirmed) => {
                if (confirmed) {
                    File.Delete(System.IO.Path.Combine(Path, "settings.user.toml"));
                    userSettings = new Settings(System.IO.Path.Combine(Path, "settings.user.toml"), true);
                }
            });
        }

        return userSettings;
    }
    
    public Inputs? LoadInputs() {
        if (_inputs != null) {
            return _inputs;
        }
        
        var inputsControllerPath = Settings.Get<string>("General.inputs_controller", null);
        
        if (inputsControllerPath == null) {
            return null;
        }

        Lua state = new Lua();
        
        SetupState(state);
        
        var entryFile = File.ReadAllText(System.IO.Path.Combine(Path, inputsControllerPath));
        
        state.DoString(entryFile, inputsControllerPath);

        _inputs = new Inputs(state, this);
        
        if (state["Settings"] is not Settings userSettings) {
            throw new ScriptException("No Settings in the Module's state.");
        }

        if (!userSettings.Get("Inputs.has_default_combos", false)) {
            var defaultCombos = Settings.Get("Inputs.combos", new List<Dictionary<string, object>>());
            
            userSettings.Set("Inputs.combos", defaultCombos);
            userSettings.Set("Inputs.has_default_combos", true);
        }

        if (Delegate?.Windows is { } windows) {
            foreach (var window in windows) {
                _inputs.BindButtonCombos(window);
            }
        }
        
        return _inputs;
    }
}