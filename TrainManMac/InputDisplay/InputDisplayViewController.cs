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

    private readonly InputDisplayView _inputDisplayView;

    public InputDisplayViewController(Inputs inputs) {
        _inputDisplayView = new InputDisplayView(inputs);
        var viewFrame = _inputDisplayView.Frame;

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

        View.AddSubview(_inputDisplayView);
    }
}

public class InputDisplayView : NSView {
    private readonly Inputs _inputs;
    private Inputs.InputState _currentInputs = new();
    private ControllerSkin _controllerSkin;
    
    public InputDisplayView(Inputs inputs) {
        _inputs = inputs;
        
        string skinPath = "controllerskins/default";
        _controllerSkin = new ControllerSkin(skinPath);

        Frame = new CGRect(0, 0, _controllerSkin.Buttons["base"].SpriteWidth,
            _controllerSkin.Buttons["base"].SpriteHeight);

        WantsLayer = true;
        Layer!.BackgroundColor = NSColor.Purple.CGColor;

        _inputs.OnInput += TimerTick;
    }

    private void TimerTick(Inputs.InputState inputs) {
        _currentInputs = inputs;
        
        NeedsDisplay = true;
    }

    public override void DrawRect(CGRect dirtyRect) {
        base.DrawRect(dirtyRect);

        if (NSGraphicsContext.CurrentContext == null) {
            return;
        }

        var context = NSGraphicsContext.CurrentContext.GraphicsPort;

        NSImage sprite = _controllerSkin.Image;

        var buttons = _controllerSkin.Buttons;

        // Replace with your actual Inputs implementation
        var inputs = _currentInputs;

        // Draw the base image
        DrawButton(sprite, buttons["base"], context);

        // Draw analog sticks
        DrawAnalogStick(sprite, buttons["r3"], buttons["r3Press"], inputs.rx, inputs.ry,
            inputs.Mask.Contains(Inputs.Buttons.R3), context);
        DrawAnalogStick(sprite, buttons["l3"], buttons["l3Press"], inputs.lx, inputs.ly,
            inputs.Mask.Contains(Inputs.Buttons.L3), context);

        // Draw D-Pad buttons
        DrawConditionalButton(sprite, buttons["dpadLeft"], inputs.Mask.Contains(Inputs.Buttons.Left), context);
        DrawConditionalButton(sprite, buttons["dpadRight"], inputs.Mask.Contains(Inputs.Buttons.Right), context);
        DrawConditionalButton(sprite, buttons["dpadUp"], inputs.Mask.Contains(Inputs.Buttons.Up), context);
        DrawConditionalButton(sprite, buttons["dpadDown"], inputs.Mask.Contains(Inputs.Buttons.Down), context);

        // Draw face buttons
        DrawConditionalButton(sprite, buttons["cross"], inputs.Mask.Contains(Inputs.Buttons.Cross), context);
        DrawConditionalButton(sprite, buttons["circle"], inputs.Mask.Contains(Inputs.Buttons.Circle), context);
        DrawConditionalButton(sprite, buttons["triangle"], inputs.Mask.Contains(Inputs.Buttons.Triangle), context);
        DrawConditionalButton(sprite, buttons["square"], inputs.Mask.Contains(Inputs.Buttons.Square), context);

        // Draw other buttons
        DrawConditionalButton(sprite, buttons["select"], inputs.Mask.Contains(Inputs.Buttons.Select), context);
        DrawConditionalButton(sprite, buttons["start"], inputs.Mask.Contains(Inputs.Buttons.Start), context);
        DrawConditionalButton(sprite, buttons["r1"], inputs.Mask.Contains(Inputs.Buttons.R1), context);
        DrawConditionalButton(sprite, buttons["l1"], inputs.Mask.Contains(Inputs.Buttons.L1), context);
        DrawConditionalButton(sprite, buttons["r2"], inputs.Mask.Contains(Inputs.Buttons.R2), context);
        DrawConditionalButton(sprite, buttons["l2"], inputs.Mask.Contains(Inputs.Buttons.L2), context);
    }

    private void DrawButton(NSImage sprite, InputPlot plot, CGContext context) {
        var flippedDrawY = (int)(Frame.Height - plot.DrawY - plot.SpriteHeight);
        var flippedSpriteY = (int)(sprite.Size.Height - plot.SpriteY - plot.SpriteHeight);

        var destRect = new CGRect(plot.DrawX, flippedDrawY, plot.SpriteWidth, plot.SpriteHeight);
        var sourceRect = new CGRect(plot.SpriteX, flippedSpriteY, plot.SpriteWidth, plot.SpriteHeight);

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

        var flippedDrawY = (int)(Frame.Height - plot.DrawY - plot.SpriteHeight);
        var flippedSpriteY = (int)(sprite.Size.Height - plot.SpriteY - plot.SpriteHeight);

        var adjustedX = (xAxis * _controllerSkin.AnalogPitch);
        var adjustedY = flippedDrawY - (yAxis * _controllerSkin.AnalogPitch);

        var destRect = new CGRect(adjustedX + plot.DrawX, adjustedY, plot.SpriteWidth, plot.SpriteHeight);
        var sourceRect = new CGRect(plot.SpriteX, flippedSpriteY, plot.SpriteWidth, plot.SpriteHeight);

        sprite.Draw(destRect, sourceRect, NSCompositingOperation.SourceOver, 1.0f);
    }

    public override void RemoveFromSuperview() {
        base.RemoveFromSuperview();
        _inputs.OnInput -= TimerTick;
    }
}