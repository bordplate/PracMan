using System.Globalization;
using Nett;
using NLua;
using TrainManCore.Scripting.UI;
using TrainManCore.Target;

namespace TrainManCore.Scripting;

public class Module {
    public event Action OnExit;
    
    public string Title;
    public string ModulePath;
    public bool IsLoaded;
    public Settings Settings;

    private Target.Target _target;

    private Lua _state = new();

    public ITrainer TrainerDelegate;
    
    public Module(string title, string path, Target.Target target) {
        Title = title;
        ModulePath = path;
        _target = target;
        
        string settingsPath = Path.Combine(path, "config.toml");
        Settings = new Settings(settingsPath);
    }

    public void Load() {
        string entry = Settings.Get<string>("General.entry");
        
        if (entry != null) {
            string entryFile = File.ReadAllText(Path.Combine(ModulePath, entry));
        
            if (entryFile == null) {
                throw new Exception("Entry point file not found.");
            }

            SetupState(_state);
        
            _state.DoString(entryFile, entry);

            (_state["OnLoad"] as LuaFunction)?.Call();
        }

        var patches = Settings.GetTableForSection("Patches");
        if (patches != null) {
            foreach (string addressString in patches.Keys) {
                uint address = Convert.ToUInt32(addressString, 16);
                
                if (patches[addressString].TomlType == TomlObjectType.Int) {
                    var value = patches.Get<int>(addressString);
                    
                    _target.WriteMemory(address, (uint)value);
                }

                if (patches[addressString].TomlType == TomlObjectType.String) {
                    // This is a binary file that we need to load and write to memory
                    var filename = patches.Get<string>(addressString);
                    var bytes = File.ReadAllBytes(Path.Combine(ModulePath, filename));
                    
                    // Write 1024 bytes at a time
                    for (int i = 0; i < bytes.Length; i += 1024) {
                        int length = Math.Min(1024, bytes.Length - i);
                        byte[] chunk = new byte[length];
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
        
        TrainerDelegate.CloseAllWindows();

        OnExit();
    }

    public void SetupState(Lua state) {
        state.LoadCLRPackage();
                
        // Set package path to the runtime folder and the module's folder
        state.DoString($"package.path = package.path .. ';{ModulesRoot()}/Runtime/?.lua;{ModulePath}/?.lua'", "set package path");

        state["bytestoint"] = ByteArrayToInt;
        state["bytestouint"] = ByteArrayToUInt;
        state["bytestofloat"] = ByteArrayToFloat;
        
        state.DoString(File.ReadAllText(Path.Combine(ModulesRoot(), "Runtime/runtime.lua")), "runtime.lua");
        
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

    public IWindow CreateWindow(bool isMainWindow = false) {
        return TrainerDelegate.CreateWindow(this, isMainWindow);
    }
    
    public Inputs LoadInputs() {
        var inputsControllerPath = Settings.Get<string>("General.inputs_controller", null);
        
        if (inputsControllerPath == null) {
            return null;
        }

        Lua state = new Lua();
        
        SetupState(state);
        
        string entryFile = File.ReadAllText(Path.Combine(ModulePath, inputsControllerPath));
        
        state.DoString(entryFile, inputsControllerPath);
        
        return new Inputs(state);
    }
    
    public static string ModulesRoot() {
        var rootDir = Path.Combine(Directory.GetCurrentDirectory());
        
        if (Environment.GetEnvironmentVariable("TRAINMAN_ROOT") != null) {
            rootDir = Environment.GetEnvironmentVariable("TRAINMAN_ROOT");
        }
        
        return Path.Combine(rootDir, "Scripts");
    }
    
    public static List<Module> ModulesForTitle(string title, Target.Target target) {
        if (title == null || title == "") {
            return [];
        }
        
        string scriptsDir = Path.Combine(ModulesRoot(), title);
        
        if (Directory.Exists(scriptsDir)) {
            string[] dirs = Directory.GetDirectories(scriptsDir);
            List<Module> modules = [];
            
            foreach (string dir in dirs) {
                modules.Add(new Module(title, dir, target));
            }
        
            return modules;
        }
        
        return [];
    }

    public static int ByteArrayToInt(byte[] bytes, int startIndex = 0) {
        return BitConverter.ToInt32(bytes, startIndex);
    }
    
    public static uint ByteArrayToUInt(byte[] bytes, int startIndex = 0) {
        return BitConverter.ToUInt32(bytes, startIndex);
    }
    
    public static float ByteArrayToFloat(byte[] bytes, int startIndex = 0) {
        return BitConverter.ToSingle(bytes, startIndex);
    }
}