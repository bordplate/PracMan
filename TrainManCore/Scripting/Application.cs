using Nett.Parser;

namespace TrainManCore.Scripting;

public static class Application {
    public static IApplication? Delegate;
    public static List<Target.Target> ActiveTargets = [];
    
    public static void LoadModule(Target.Target target, string title, string moduleName) {
        if (target.Modules.Find(module => module.Identifier == moduleName && module.Title == title) != null) {
            return;
        }
        
        var module = new Module(title, Path.Combine(GetModulesRoot(), title, moduleName));
        module.Load(target);
    }
    
    public static void UnloadModule(Target.Target target, string title, string moduleName) {
        target.Modules.Find(module => module.Identifier == moduleName && module.Title == title)?.Exit();
    }
    
    public static string GetModulesRoot() {
        var rootDir = Environment.GetEnvironmentVariable("TRAINMAN_ROOT");
        
        if (rootDir == null) {
            rootDir = Directory.GetCurrentDirectory();
        }
        
        return Path.Combine(rootDir, "Scripts");
    }
    
    public static List<Module> GetModulesForTitle(string title) {
        if (title == "") {
            return [];
        }
        
        var scriptsDir = Path.Combine(GetModulesRoot(), title);
        
        if (Directory.Exists(scriptsDir)) {
            var dirs = Directory.GetDirectories(scriptsDir);
            List<Module> modules = [];
            
            foreach (var dir in dirs) {
                try {
                    var module = new Module(title, dir);
                    modules.Add(module);
                } catch (ParseException exception) {
                    Console.Error.WriteLine($"Could not parse config file for module {dir}: {exception.Message}");
                }
            }
        
            return modules;
        }
        
        return [];
    }
}