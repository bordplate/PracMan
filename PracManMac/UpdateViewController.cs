using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using PracManCore;
using WebKit;

namespace PracManMac;

public class UpdateViewController: NSViewController, IUpdateAvailable {
    public NSWindow Window;
    
    private string _appName;
    private string _currentVersion;
    private string _newVersion;
    private string _releaseNotesURL;
    
    private NSButton _installUpdateButton;
    
    public UpdateAvailableResult Result { get; }
    public AppCastItem CurrentItem { get; }
    public event UserRespondedToUpdate? UserResponded;
    
    public UpdateViewController(string appName, string currentVersion, string newVersion, string releaseNotesURL, AppCastItem currentItem) {
        _appName = appName;
        _currentVersion = currentVersion;
        _newVersion = newVersion;
        _releaseNotesURL = releaseNotesURL;
        CurrentItem = currentItem;
        
        _installUpdateButton = new NSButton {
            Title = "Install Update",
            BezelStyle = NSBezelStyle.Rounded,
            TranslatesAutoresizingMaskIntoConstraints = false,
            Action = new ObjCRuntime.Selector("installUpdateClicked:"),
        };

        Window = new NSWindow(
            CGRect.Empty, 
            NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable, 
            NSBackingStore.Buffered, 
            true) {
            Title = "Software Update",
            ContentViewController = this,
        };
        
        Window.Center();
        
        Window.DidBecomeKey += (sender, e) => {
            Window.DefaultButtonCell = _installUpdateButton.Cell;
        };
    }
    
    public override void LoadView() {
        // If we don't include this, the application crashed on macOS <= 13.0 when we initialize the controller
        View = new NSView();
    }
    
    public override void ViewDidLoad() {
        base.ViewDidLoad();
        
        var appIconImageView = new NSImageView {
            Image = NSApplication.SharedApplication.ApplicationIconImage,
            TranslatesAutoresizingMaskIntoConstraints = false,
        };

        var titleLabel = new NSTextField {
            StringValue = $"A new version of {_appName} is available!",
            Editable = false,
            Bordered = false,
            Bezeled = false,
            DrawsBackground = false,
            Alignment = NSTextAlignment.Left,
            Font = NSFont.BoldSystemFontOfSize(14)!
        };
        
        var versionLabel = new NSTextField {
            StringValue = $"{_appName} {_newVersion} is now available—you have {_currentVersion}. Would you like to download it now?",
            Editable = false,
            Bordered = false,
            Bezeled = false,
            Alignment = NSTextAlignment.Left,
            DrawsBackground = false,
            Font = NSFont.SystemFontOfSize(12)!
        };
        
        var releaseNotesLabel = new NSTextField {
            StringValue = "Release Notes:",
            Editable = false,
            Bordered = false,
            Bezeled = false,
            Alignment = NSTextAlignment.Left,
            DrawsBackground = false,
            Font = NSFont.BoldSystemFontOfSize(12)!
        };
        
        var webViewConfiguration = new WKWebViewConfiguration();
        var releaseNotesWebView = new WKWebView(CGRect.Empty, webViewConfiguration) {
            TranslatesAutoresizingMaskIntoConstraints = false,
        };
        releaseNotesWebView.WantsLayer = true;
        releaseNotesWebView.Layer!.BorderWidth = 1;
        releaseNotesWebView.Layer!.BorderColor = NSColor.Gray.CGColor;
        
        var request = new NSUrlRequest(new NSUrl(_releaseNotesURL));
        releaseNotesWebView.LoadRequest(request);
        
        releaseNotesWebView.AddConstraint(NSLayoutConstraint.Create(releaseNotesWebView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 200));
        
        var automaticallyDownloadCheckbox = new NSButton {
            Title = "Automatically download and install updates in the future",
            State = Settings.Default.Get("General.AutomaticallyDownloadUpdates", true) ? NSCellStateValue.On : NSCellStateValue.Off,
            TranslatesAutoresizingMaskIntoConstraints = false,
            Action = new ObjCRuntime.Selector("automaticallyDownloadClicked:"),
        };
        automaticallyDownloadCheckbox.SetButtonType(NSButtonType.Switch);
        
        var skipThisVersionButton = new NSButton {
            Title = "Skip This Version",
            BezelStyle = NSBezelStyle.Rounded,
            TranslatesAutoresizingMaskIntoConstraints = false,
            Action = new ObjCRuntime.Selector("skipThisVersionClicked:"),
        };
        
        var remindMeLaterButton = new NSButton {
            Title = "Remind Me Later",
            BezelStyle = NSBezelStyle.Rounded,
            TranslatesAutoresizingMaskIntoConstraints = false,
            Action = new ObjCRuntime.Selector("remindMeLaterClicked:"),
        };
        
        var stackView = new NSStackView {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Orientation = NSUserInterfaceLayoutOrientation.Vertical,
            Distribution = NSStackViewDistribution.Fill,
            Spacing = 10,
            Alignment = NSLayoutAttribute.Left,
        };
        
        stackView.AddArrangedSubview(titleLabel);
        stackView.AddArrangedSubview(versionLabel);
        stackView.AddArrangedSubview(releaseNotesLabel);
        stackView.AddArrangedSubview(releaseNotesWebView);
        stackView.AddArrangedSubview(automaticallyDownloadCheckbox);
        
        var views = new NSDictionary(
            "appIconImageView", appIconImageView, 
            "stackView", stackView, 
            "skipThisVersionButton", skipThisVersionButton, 
            "remindMeLaterButton", remindMeLaterButton, 
            "installUpdateButton", _installUpdateButton
        );
        
        View.AddSubview(appIconImageView);
        View.AddSubview(stackView);
        View.AddSubview(skipThisVersionButton);
        View.AddSubview(remindMeLaterButton);
        View.AddSubview(_installUpdateButton);
        
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-20-[appIconImageView(50)]-20-[stackView]-20-|", NSLayoutFormatOptions.None, null, views));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[appIconImageView]-20-[skipThisVersionButton]", NSLayoutFormatOptions.None, null, views));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[remindMeLaterButton]-[installUpdateButton]-20-|", NSLayoutFormatOptions.None, null, views));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-[stackView]-[skipThisVersionButton]-|", NSLayoutFormatOptions.None, null, views));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-[stackView]-[remindMeLaterButton]-|", NSLayoutFormatOptions.None, null, views));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-[stackView]-[installUpdateButton]-|", NSLayoutFormatOptions.None, null, views));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-20-[appIconImageView(50)]", NSLayoutFormatOptions.None, null, views));
    }

    public void Show() {
        Window.MakeKeyAndOrderFront(null);
    }

    public void HideReleaseNotes() {
        throw new NotImplementedException();
    }

    public void HideRemindMeLaterButton() {
        throw new NotImplementedException();
    }

    public void HideSkipButton() {
        throw new NotImplementedException();
    }

    public void BringToFront() {
        throw new NotImplementedException();
    }

    public void Close() {
        Window.Close();
    }
    
    [Export("automaticallyDownloadClicked:")]
    public void AutomaticallyDownloadClicked(NSObject sender) {
        var button = (NSButton)sender;
        
        Settings.Default.Set("General.AutomaticallyDownloadUpdates", button.State == NSCellStateValue.On);
    }
    
    [Export("installUpdateClicked:")]
    public void InstallUpdateClicked(NSObject sender) {
        UserResponded?.Invoke(this, new UpdateResponseEventArgs(UpdateAvailableResult.InstallUpdate, CurrentItem));
    }
    
    [Export("skipThisVersionClicked:")]
    public void SkipThisVersionClicked(NSObject sender) {
        Settings.Default.Set("General.SkipVersion", CurrentItem.Version);
        
        UserResponded?.Invoke(this, new UpdateResponseEventArgs(UpdateAvailableResult.SkipUpdate, CurrentItem));
    }
    
    [Export("remindMeLaterClicked:")]
    public void RemindMeLaterClicked(NSObject sender) {
        UserResponded?.Invoke(this, new UpdateResponseEventArgs(UpdateAvailableResult.RemindMeLater, CurrentItem));
    }
}