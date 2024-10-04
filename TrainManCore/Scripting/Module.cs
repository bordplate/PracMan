using System.Globalization;
using Nett;
using NLua;
using NLua.Exceptions;
using TrainManCore.Scripting.Exceptions;
using TrainManCore.Scripting.UI;
using TrainManCore.Target;

namespace TrainManCore.Scripting;

public class Module(string title, string path, Target.Target target) {
    public event Action? OnExit;
    
    public string Title = title;
    public readonly string ModulePath = path;
    public bool IsLoaded;
    public readonly Settings Settings = new(Path.Combine(path, "config.toml"));

    private readonly Target.Target _target = target;
    private Inputs? _inputs;

    private readonly Lua _state = new();

    public ITrainer? TrainerDelegate;

    public void Load() {
        var entry = Settings.Get<string>("General.entry");
        
        if (entry != null) {
            var entryFile = File.ReadAllText(Path.Combine(ModulePath, entry));
        
            if (entryFile == null) {
                throw new ScriptException("Entry point file not found.");
            }

            SetupState(_state);

            try {
                _state.DoString(entryFile, entry);
                (_state["OnLoad"] as LuaFunction)?.Call();
            } catch (LuaScriptException exception) {
                throw new ScriptException(exception.Message);
            }
        }

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
                    var bytes = File.ReadAllBytes(Path.Combine(ModulePath, filename));
                    
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
        
        IsLoaded = true;
    }
    
    public void Exit() {
        IsLoaded = false;
        
        (_state["OnUnload"] as LuaFunction)?.Call();
        
        TrainerDelegate?.CloseAllWindows();

        OnExit?.Invoke();
    }

    private void SetupState(Lua state) {
        state.UseTraceback = true;
        state.LoadCLRPackage();
        
        if (TrainerDelegate == null) {
            throw new ScriptException("TrainerDelegate not set.");
        }
        
        // Set package path to the runtime folder and the module's folder
        state.DoString($"package.path = package.path .. ';{GetModulesRoot()}/Runtime/?.lua;{ModulePath}/?.lua'", "set package path");
        
        foreach (var (key, value) in LuaFunctions.Functions) {
            state[key] = value;
        }
        
        state.DoString(File.ReadAllText(Path.Combine(GetModulesRoot(), "Runtime/runtime.lua")), "runtime.lua");
        
        state["Module"] = this;
        state["print"] = (string text) => {
            Console.WriteLine(text);
        };

        state["AddMenu"] = TrainerDelegate.AddMenu;

        state["Alert"] = (string text) => {
            if (_target.CanInlineNotify()) {
                _target.Notify(text);
            } else {
                TrainerDelegate.Alert(text);
            }
        };

        state["Settings"] = new Settings(Path.Combine(ModulePath, "settings.user.toml"), true);
        
        state["UINT_MAX"] = uint.MaxValue;
        state["INT_MAX"] = int.MaxValue;

        state["Ratchetron"] = _target;
        state["Target"] = _target;
    }

    public IWindow CreateWindow(LuaTable luaObject, bool isMainWindow = false) {
        if (luaObject["class"] is not LuaTable luaClass) {
            throw new ScriptException("No class found in object passed to `CreateWindow`.");
        }
        
        if (luaClass["name"] is not string) {
            throw new ScriptException($"`name` not found in class passed to `CreateWindow`.");
        }

        var window = TrainerDelegate!.CreateWindow(this, luaObject, isMainWindow);
        window.OnLoad += (window) => {
            _inputs?.BindButtonCombos(window);
        };
        
        return window;
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
        
        var entryFile = File.ReadAllText(Path.Combine(ModulePath, inputsControllerPath));
        
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

        if (TrainerDelegate?.Windows is { } windows) {
            foreach (var window in windows) {
                _inputs.BindButtonCombos(window);
            }
        }
        
        return _inputs;
    }
    
    public static string GetModulesRoot() {
        var rootDir = Environment.GetEnvironmentVariable("TRAINMAN_ROOT");
        
        if (rootDir == null) {
            rootDir = Directory.GetCurrentDirectory();
        }
        
        return Path.Combine(rootDir, "Scripts");
    }
    
    public static List<Module> GetModulesForTitle(string title, Target.Target target) {
        if (title == "") {
            return [];
        }
        
        var scriptsDir = Path.Combine(GetModulesRoot(), title);
        
        if (Directory.Exists(scriptsDir)) {
            var dirs = Directory.GetDirectories(scriptsDir);
            List<Module> modules = [];
            
            foreach (var dir in dirs) {
                modules.Add(new Module(title, dir, target));
            }
        
            return modules;
        }
        
        return [];
    }
}