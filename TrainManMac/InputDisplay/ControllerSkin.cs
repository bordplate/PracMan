namespace TrainMan;

class ControllerSkin {
        public readonly NSImage Image = new();
        public readonly Dictionary<string, InputPlot> Buttons;
        public readonly int AnalogPitch = 32;

        public ControllerSkin(string skinPath) {
            Buttons = new Dictionary<string, InputPlot>();

            var config = File.ReadAllText(Path.Combine(skinPath, "skin.txt"));

            foreach (var line in config.Split('\n')) {
                if (line.Length < 2 || line[0] == '#')
                    continue;

                var components = line.Split(':');
                if (components.Length < 2)
                    continue;

                string buttonName = components[0];

                if (buttonName == "imageName") {
                    var imageRep = NSBitmapImageRep.ImageRepsFromFile(Path.Combine(skinPath, components[1].Trim()))[0];

                    Image = new NSImage(Path.Combine(skinPath, components[1].Trim()));

                    Image.Size = new CGSize(imageRep.PixelsWide, imageRep.PixelsHigh);

                    continue;
                }

                if (buttonName == "analogPitch") {
                    AnalogPitch = int.Parse(components[1].Trim());
                    continue;
                }

                var plot = components[1]
                    .Split(',')
                    .Select(thing => int.Parse(thing.Trim()))
                    .ToArray();

                if (plot.Length < 6)
                    continue;

                var inputPlot = new InputPlot {
                    DrawX = plot[0],
                    DrawY = plot[1],
                    SpriteX = plot[2],
                    SpriteY = plot[3],
                    SpriteWidth = plot[4],
                    SpriteHeight = plot[5]
                };

                Buttons[buttonName] = inputPlot;
            }
        }
        
        public static List<string> GetSkins() {
            var skins = new List<string>();

            var skinsPath = "controllerskins";

            if (!Directory.Exists(skinsPath))
                return skins;

            foreach (var skin in Directory.GetDirectories(skinsPath)) {
                if (File.Exists(Path.Combine(skin, "skin.txt"))) {
                    skins.Add(Path.GetFileName(skin));
                }
            }

            return skins;
        }
    }