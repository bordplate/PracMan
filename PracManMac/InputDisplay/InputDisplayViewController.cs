using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using PracManCore;
using PracManCore.Scripting;

namespace PracManMac;

public class InputDisplayViewController : NSViewController, INSMenuDelegate {
    public NSWindow Window;

    private InputDisplayView _inputDisplayView;
    
    private Inputs _inputs;
    
    private NSTitlebarAccessoryViewController _titlebarAccessoryViewController;
    private NSPopUpButton _skinSelectorPopUp;

    private string _selectedSkinName;

    public InputDisplayViewController(Inputs inputs) {
        _inputs = inputs;
        _selectedSkinName = Settings.Default.Get("InputDisplay.skin", "default", false)!;
        
        _inputDisplayView = new InputDisplayView(_inputs, _selectedSkinName);
        _titlebarAccessoryViewController = new NSTitlebarAccessoryViewController();

        Window = new NSWindow(
            new CGRect(0, 0, 150, 150),
            NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable,
            NSBackingStore.Buffered,
            true) {
            Title = $"Input Display"
        };
        
        Window.AddTitlebarAccessoryViewController(_titlebarAccessoryViewController);

        Window.Center();
        
        _skinSelectorPopUp = new NSPopUpButton {
            TranslatesAutoresizingMaskIntoConstraints = false
        };
        _skinSelectorPopUp.Action = new ObjCRuntime.Selector("skinSelected:");
        
        foreach (var skin in ControllerSkin.GetSkins()) {
            _skinSelectorPopUp.AddItem(skin);
            
            if (skin == _selectedSkinName) {
                _skinSelectorPopUp.SelectItem(_skinSelectorPopUp.ItemTitles().ToList().IndexOf(skin));
            }
        }
        
        _skinSelectorPopUp.Items().First().Menu!.Delegate = this;
        
        Window.ContentViewController = this;
        Window.TitlebarAppearsTransparent = false;
    }

    public override void ViewDidLoad() {
        base.ViewDidLoad();

        View.AddSubview(_inputDisplayView);
        
        var viewFrame = _inputDisplayView.Frame;
        var titlebarAccessoryViewController = Window.TitlebarAccessoryViewControllers[0];
        
        titlebarAccessoryViewController.View.AddSubview(_skinSelectorPopUp);
        titlebarAccessoryViewController.LayoutAttribute = NSLayoutAttribute.Right;

        var titlebarHeight = titlebarAccessoryViewController.View.Frame.Height;
        
        titlebarAccessoryViewController.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("|-[skinSelectorPopUp(150)]-4-|",
            NSLayoutFormatOptions.None, null, new NSDictionary("skinSelectorPopUp", _skinSelectorPopUp)));
        titlebarAccessoryViewController.View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-4-[skinSelectorPopUp]",
            NSLayoutFormatOptions.AlignAllBaseline, null, new NSDictionary("skinSelectorPopUp", _skinSelectorPopUp)));
        
        View.Frame = viewFrame;
        _inputDisplayView.Frame = new CGRect(0, 0, viewFrame.Width, viewFrame.Height);
        Window.SetFrame(new CGRect(Window.Frame.X, Window.Frame.Y, viewFrame.Width, viewFrame.Height+titlebarHeight), false);
    }
    
    [Export("skinSelected:")]
    public void SkinSelected(NSObject sender) {
        if (sender is not NSPopUpButton popUpButton) {
            return;
        }

        _selectedSkinName = popUpButton.SelectedItem.Title;
        
        Settings.Default.Set("InputDisplay.skin", _selectedSkinName);
        
        _inputDisplayView.RemoveFromSuperview();
        _inputDisplayView.Dispose();
        
        _inputDisplayView = new InputDisplayView(_inputs, _selectedSkinName);
        
        View.AddSubview(_inputDisplayView);
        
        var viewFrame = _inputDisplayView.Frame;
        var titlebarAccessoryViewController = Window.TitlebarAccessoryViewControllers[0];
        var titlebarHeight = titlebarAccessoryViewController.View.Frame.Height;
        
        Window.SetFrame(new CGRect(Window.Frame.X, Window.Frame.Y, viewFrame.Width, viewFrame.Height+titlebarHeight), true, true);
    }
    
    [Export("menuWillOpen:")]
    public void MenuWillOpen(NSMenu menu) {
        _skinSelectorPopUp.RemoveAllItems();
        foreach (var skin in ControllerSkin.GetSkins()) {
            _skinSelectorPopUp.AddItem(skin);
            
            if (skin == _selectedSkinName) {
                _skinSelectorPopUp.SelectItem(_skinSelectorPopUp.ItemTitles().ToList().IndexOf(skin));
            }
        }
    }
}

public class InputDisplayView : NSView {
    private readonly Inputs _inputs;
    private Inputs.InputState _currentInputs = new();
    private ControllerSkin _controllerSkin;
    
    public InputDisplayView(Inputs inputs, string skinPath) {
        _inputs = inputs;
        
        _controllerSkin = new ControllerSkin(Path.Combine("controllerskins", skinPath));

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