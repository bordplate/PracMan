using NLua;
using TrainManCore.Scripting.UI;
using TrainManCore.Target;

namespace TrainManCore.Scripting;

public class Module {
    public string Title;
    public string ModulePath;
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
        
        if (entry == null) {
            throw new Exception("No entry point specified in module config.");
        }
        
        string entryFile = File.ReadAllText(Path.Combine(ModulePath, entry));
        
        if (entryFile == null) {
            throw new Exception("Entry point file not found.");
        }
        
        _state.LoadCLRPackage();
        
        // Set package path to the runtime folder and the module's folder
        _state.DoString($"package.path = package.path .. ';{ModulesRoot()}/Runtime/?.lua;{ModulePath}/?.lua'", "set package path");
        
        _state.DoString(File.ReadAllText(Path.Combine(ModulesRoot(), "Runtime/runtime.lua")), "runtime.lua");
        _state.DoString(entryFile, entry);

        SetupState();

        (_state["OnLoad"] as LuaFunction)?.Call();
    }
    
    public void Exit() {
        (_state["OnUnload"] as LuaFunction)?.Call();
        
        TrainerDelegate.CloseAllWindows();

        _target.Stop();
    }

    public void SetupState() {
        _state["Module"] = this;
        _state["print"] = (string text) => {
            Console.WriteLine(text);
        };

        _state["AddMenu"] = TrainerDelegate.AddMenu;

        _state["Alert"] = (string text) => {
            if (_target.CanInlineNotify()) {
                _target.Notify(text);
            } else {
                TrainerDelegate.Alert(text);
            }
        };

        _state["Settings"] = new Settings(Path.Combine(ModulePath, "settings.user.toml"), true);
        
        _state["UINT_MAX"] = uint.MaxValue;
        _state["INT_MAX"] = int.MaxValue;

        _state["Ratchetron"] = _target;
        _state["Target"] = _target;
    }

    public IWindow CreateWindow(bool isMainWindow = false) {
        return TrainerDelegate.CreateWindow(this, isMainWindow);
    }
    
    public static string ModulesRoot() {
        var rootDir = Path.Combine(Directory.GetCurrentDirectory());
        
        if (Environment.GetEnvironmentVariable("TRAINMAN_ROOT") != null) {
            rootDir = Environment.GetEnvironmentVariable("TRAINMAN_ROOT");
        }
        
        return Path.Combine(rootDir, "Scripts");
    }
    
    public static List<Module> ModulesForTitle(string title, Target.Target target) {
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
}