using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using TrainManCore.Scripting;

namespace TrainMan;

public class InputDisplayViewController : NSViewController {
    public NSWindow Window;

    public InputDisplayView InputDisplayView;

    public InputDisplayViewController(Inputs inputs) {
        InputDisplayView = new InputDisplayView(inputs);
        var viewFrame = InputDisplayView.Frame;

        View.Frame = viewFrame;

        Window = new NSWindow(
            new CGRect(0, 0, viewFrame.Width, viewFrame.Height),
            NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable,
            NSBackingStore.Buffered,
            true) {
            Title = $"Input Display"
        };

        Window.ContentViewController = this;

        Window.Center();
    }

    public override void ViewDidLoad() {
        base.ViewDidLoad();

        View.AddSubview(InputDisplayView);
    }
}

public class InputDisplayView : NSView {
    private Inputs _inputs;
    
    public InputDisplayView(Inputs inputs) {
        _inputs = inputs;
        
        Initialize();
    }
    
    public struct InputPlot {
        public int drawX { get; set; }
        public int drawY { get; set; }
        public int spriteX { get; set; }
        public int spriteY { get; set; }
        public int spriteWidth { get; set; }
        public int spriteHeight { get; set; }
    }

    class ControllerSkin {
        public NSImage image;
        public Dictionary<string, InputPlot> buttons;
        public int analogPitch = 32;

        public static ControllerSkin Load(string skinPath) {
            var skin = new ControllerSkin();
            skin.buttons = new Dictionary<string, InputPlot>();
            
            skinPath = Path.Combine(Environment.GetEnvironmentVariable("TRAINMAN_ROOT"), skinPath);

            var config = File.ReadAllText(Path.Combine(skinPath, "skin.txt"));

            foreach (var line in config.Split('\n')) {
                if (line.Length < 2 || line[0] == '#')
                    continue;

                var components = line.Split(':');
                if (components.Length < 2)
                    continue;

                string buttonName = components[0];

                if (buttonName == "imageName") {
                    var image = NSBitmapImageRep.ImageRepsFromFile(Path.Combine(skinPath, components[1].Trim()))[0];

                    skin.image = new NSImage(Path.Combine(skinPath, components[1].Trim()));

                    skin.image.Size = new CGSize(image.PixelsWide, image.PixelsHigh);

                    continue;
                }

                if (buttonName == "analogPitch") {
                    skin.analogPitch = int.Parse(components[1].Trim());
                    continue;
                }

                var plot = components[1]
                    .Split(',')
                    .Select(thing => int.Parse(thing.Trim()))
                    .ToArray();

                if (plot.Length < 6)
                    continue;

                var inputPlot = new InputPlot {
                    drawX = plot[0],
                    drawY = plot[1],
                    spriteX = plot[2],
                    spriteY = plot[3],
                    spriteWidth = plot[4],
                    spriteHeight = plot[5]
                };

                skin.buttons[buttonName] = inputPlot;
            }

            return skin;
        }
    }

    private NSTimer timer;
    private ControllerSkin controllerSkin;

    public InputDisplayView() {
        Initialize();
    }

    public InputDisplayView(IntPtr handle) : base(handle) {
        Initialize();
    }

    private void Initialize() {
        // Load the controller skin
        string skinPath = "controllerskins/default"; // Adjust the path as needed
        controllerSkin = ControllerSkin.Load(skinPath);

        // Set the view size to the image size
        Frame = new CGRect(0, 0, controllerSkin.buttons["base"].spriteWidth,
            controllerSkin.buttons["base"].spriteHeight);

        WantsLayer = true;
        Layer.BackgroundColor = NSColor.Purple.CGColor;

        // Start the timer to refresh the view
        timer = NSTimer.CreateRepeatingScheduledTimer(TimeSpan.FromMilliseconds(16.66667), TimerTick);
    }

    private void TimerTick(NSTimer obj) {
        NeedsDisplay = true;
    }

    public override void DrawRect(CGRect dirtyRect) {
        base.DrawRect(dirtyRect);

        var context = NSGraphicsContext.CurrentContext.GraphicsPort;

        NSImage sprite = controllerSkin.image;

        var buttons = controllerSkin.buttons;

        // Replace with your actual Inputs implementation
        var inputs = _inputs.GetCurrentInputs();

        // Draw the base image
        DrawButton(sprite, buttons["base"], context);

        // Draw analog sticks
        DrawAnalogStick(sprite, buttons["r3"], buttons["r3Press"], inputs.rx, inputs.ry,
            inputs.Mask.Contains(Inputs.Buttons.r3), context);
        DrawAnalogStick(sprite, buttons["l3"], buttons["l3Press"], inputs.lx, inputs.ly,
            inputs.Mask.Contains(Inputs.Buttons.l3), context);

        // Draw D-Pad buttons
        DrawConditionalButton(sprite, buttons["dpadLeft"], inputs.Mask.Contains(Inputs.Buttons.left), context);
        DrawConditionalButton(sprite, buttons["dpadRight"], inputs.Mask.Contains(Inputs.Buttons.right), context);
        DrawConditionalButton(sprite, buttons["dpadUp"], inputs.Mask.Contains(Inputs.Buttons.up), context);
        DrawConditionalButton(sprite, buttons["dpadDown"], inputs.Mask.Contains(Inputs.Buttons.down), context);

        // Draw face buttons
        DrawConditionalButton(sprite, buttons["cross"], inputs.Mask.Contains(Inputs.Buttons.cross), context);
        DrawConditionalButton(sprite, buttons["circle"], inputs.Mask.Contains(Inputs.Buttons.circle), context);
        DrawConditionalButton(sprite, buttons["triangle"], inputs.Mask.Contains(Inputs.Buttons.triangle), context);
        DrawConditionalButton(sprite, buttons["square"], inputs.Mask.Contains(Inputs.Buttons.square), context);

        // Draw other buttons
        DrawConditionalButton(sprite, buttons["select"], inputs.Mask.Contains(Inputs.Buttons.select), context);
        DrawConditionalButton(sprite, buttons["start"], inputs.Mask.Contains(Inputs.Buttons.start), context);
        DrawConditionalButton(sprite, buttons["r1"], inputs.Mask.Contains(Inputs.Buttons.r1), context);
        DrawConditionalButton(sprite, buttons["l1"], inputs.Mask.Contains(Inputs.Buttons.l1), context);
        DrawConditionalButton(sprite, buttons["r2"], inputs.Mask.Contains(Inputs.Buttons.r2), context);
        DrawConditionalButton(sprite, buttons["l2"], inputs.Mask.Contains(Inputs.Buttons.l2), context);
    }

    private void DrawButton(NSImage sprite, InputPlot plot, CGContext context) {
        var flippedDrawY = (int)(Frame.Height - plot.drawY - plot.spriteHeight);
        var flippedSpriteY = (int)(sprite.Size.Height - plot.spriteY - plot.spriteHeight);

        var destRect = new CGRect(plot.drawX, flippedDrawY, plot.spriteWidth, plot.spriteHeight);
        var sourceRect = new CGRect(plot.spriteX, flippedSpriteY, plot.spriteWidth, plot.spriteHeight);

        sprite.Draw(destRect, sourceRect, NSCompositingOperation.SourceOver, 1.0f);
    }

    private void DrawConditionalButton(NSImage sprite, InputPlot plot, bool condition, CGContext context) {
        if (condition)
            DrawButton(sprite, plot, context);
    }

    private void DrawAnalogStick(NSImage sprite, InputPlot normalPlot, InputPlot pressedPlot, float xAxis, float yAxis,
        bool isPressed, CGContext context) {
        // Apparently we accidentally inverted pressedPlot and normalPlot
        var plot = isPressed ? normalPlot : pressedPlot;

        var flippedDrawY = (int)(Frame.Height - plot.drawY - plot.spriteHeight);
        var flippedSpriteY = (int)(sprite.Size.Height - plot.spriteY - plot.spriteHeight);

        var adjustedX = (xAxis * controllerSkin.analogPitch);
        var adjustedY = flippedDrawY - (yAxis * controllerSkin.analogPitch);

        var destRect = new CGRect(adjustedX + plot.drawX, adjustedY, plot.spriteWidth, plot.spriteHeight);
        var sourceRect = new CGRect(plot.spriteX, flippedSpriteY, plot.spriteWidth, plot.spriteHeight);

        sprite.Draw(destRect, sourceRect, NSCompositingOperation.SourceOver, 1.0f);
    }

    public override void RemoveFromSuperview() {
        base.RemoveFromSuperview();
        timer?.Invalidate();
    }
}