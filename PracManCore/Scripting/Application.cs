using System.IO.Compression;
using Nett.Parser;

namespace PracManCore.Scripting;

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
        var rootDir = Directory.GetCurrentDirectory();
        
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

    public static void InstallModuleFromZip(string titleId, string zipPath) {
        var tempExtractPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), titleId);
        ZipFile.ExtractToDirectory(zipPath, tempExtractPath);
        
        // Find the first directory with settings.conf
        var moduleDir = Directory.GetDirectories(tempExtractPath).FirstOrDefault(dir => 
            File.Exists(Path.Combine(dir, "config.toml")) || File.Exists(Path.Combine(dir, "patch.txt"))
            );
        
        if (moduleDir == null) {
            Delegate?.Alert("Invalid mod", "Invalid or corrupt mod. Redownload the ZIP or ask the mod developer for help.");
            return;
        }
        
        // If the module has a legacy racman patch.txt file, we need to convert it to the new format
        var patchFile = Path.Combine(moduleDir, "patch.txt");
        if (File.Exists(patchFile) && !File.Exists(Path.Combine(moduleDir, "config.toml"))) {
            ConvertPatchToConfig(moduleDir);
        }
        
        // Check if we already have a module with the same title, then check the version
        var newIdentifier = Path.GetFileName(moduleDir);
        var oldModule = GetModulesForTitle(titleId).Find(module => module.Identifier == newIdentifier);

        if (oldModule != null) {
            var newSettings = new Settings(Path.Combine(moduleDir, "config.toml"));
            var newVersion = new Version(newSettings.Get("General.version", "1.0")!);
            var oldVersion = new Version(oldModule.Settings.Get("General.version", "0.0")!);
            
            if (newVersion >= oldVersion) {
                var shouldContinue = false;
                Delegate?.ConfirmDialog("Mod already installed", "Currently installed version is newer or the same as the one you're trying to install. Overwrite?", (result) => {
                    if (result) {
                        shouldContinue = true;
                    }
                });

                if (!shouldContinue) {
                    return;
                }
            }
        }

        var source = new DirectoryInfo(tempExtractPath);
        var target = new DirectoryInfo(Path.Combine(GetModulesRoot(), titleId));
        
        CopyAll(source, target);
        
        Delegate?.Alert("Mod installed", "Mod installed successfully.");
    }

    private static void ConvertPatchToConfig(string folderPath) {
        var patchFile = Path.Combine(folderPath, "patch.txt");
        if (File.Exists(patchFile)) {
            var patchLines = File.ReadAllLines(patchFile);
            var config = new Settings(Path.Combine(folderPath, "config.toml"), true);
            
            foreach (var line in patchLines) {
                if (line.Length < 2 || (line[0] == '#' && line[..2] != "#-")) {
                    continue;
                }

                if (line[..2] == "#-") {
                    var metaComponent = line.Split([':'], 2);
                    if (metaComponent.Length != 2) {
                        continue;
                    }
                    
                    var key = metaComponent[0][2..].Trim();
                    var data = metaComponent[1].Trim();

                    if (key == "href") {
                        key = "link";
                    }
                    
                    config.Set($"General.{key}", data);
                }
                
                var components = line.Split([':'], 2);
                if (components.Length != 2) {
                    continue;
                }
                
                var address = components[0].Trim();
                var value = components[1].Trim();

                if (address == "automation") {
                    config.Set("General.entry", value);
                    continue;
                }

                if (address.Contains("0x")) {
                    if (value.Contains("0x")) {
                        var uintValue = Convert.ToInt32(value, 16);
                        config.Set($"Patches.{address}", uintValue);
                    } else {
                        config.Set($"Patches.{address}", value);
                    }
                }
            }
        }
    }
    
    static void CopyAll(DirectoryInfo source, DirectoryInfo target)
    {
        if (source.FullName.ToLower() == target.FullName.ToLower())
        {
            return;
        }

        // Check if the target directory exists, if not, create it.
        if (Directory.Exists(target.FullName) == false)
        {
            Directory.CreateDirectory(target.FullName);
        }

        // Copy each file into it's new directory.
        foreach (FileInfo fi in source.GetFiles())
        {
            Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
            fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
        }

        // Copy each subdirectory using recursion.
        foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
        {
            DirectoryInfo nextTargetSubDir =
                target.CreateSubdirectory(diSourceSubDir.Name);
            CopyAll(diSourceSubDir, nextTargetSubDir);
        }
    }
}