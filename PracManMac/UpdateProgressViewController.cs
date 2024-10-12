using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;

namespace PracManMac;

public class UpdateProgressViewController: NSViewController, IDownloadProgress {
    public NSWindow Window;
    
    private NSTextField _progressLabel;
    private NSProgressIndicator _progressIndicator;
    private NSButton _actionButton;
    
    bool _isDownloadedFileValid = false;
    bool _cancelled = false;
    
    public UpdateProgressViewController(string downloadTitle, string actionButtonTitleAfterDownload) {
        _progressLabel = new NSTextField {
            StringValue = "Downloading update...",
            Editable = false,
            Bordered = false,
            Bezeled = false,
            DrawsBackground = false,
            TranslatesAutoresizingMaskIntoConstraints = false,
        };
        
        _progressIndicator = new NSProgressIndicator {
            Style = NSProgressIndicatorStyle.Bar,
            ControlSize = NSControlSize.Regular,
            TranslatesAutoresizingMaskIntoConstraints = false,
            Indeterminate = false,
            MinValue = 0,
            MaxValue = 100,
        };
        
        _actionButton = new NSButton {
            Title = "Cancel",
            BezelStyle = NSBezelStyle.Rounded,
            TranslatesAutoresizingMaskIntoConstraints = false,
            Action = new ObjCRuntime.Selector("actionButtonClicked:"),
        };
        
        Window = new NSWindow(
            CGRect.Empty, 
            NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable, 
            NSBackingStore.Buffered, 
            true) {
            Title = downloadTitle,
            ContentViewController = this,
        };
        
        Window.Center();
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
        
        View.AddSubview(appIconImageView);
        View.AddSubview(_progressLabel);
        View.AddSubview(_progressIndicator);
        View.AddSubview(_actionButton);

        var views = new NSDictionary(
            "appIconImageView", appIconImageView,
            "progressLabel", _progressLabel,
            "progressIndicator", _progressIndicator,
            "actionButton", _actionButton
        );
        
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-20-[appIconImageView]-20-|", NSLayoutFormatOptions.AlignAllCenterX, null, views));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-10-[appIconImageView]", NSLayoutFormatOptions.AlignAllCenterX, null, views));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-20-[progressLabel]-10-[progressIndicator]-10-[actionButton]-20-|", NSLayoutFormatOptions.None, null, views));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[appIconImageView]-[progressIndicator(250)]-20-|", NSLayoutFormatOptions.None, null, views));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[appIconImageView]-[progressLabel]", NSLayoutFormatOptions.None, null, views));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[actionButton]-20-|", NSLayoutFormatOptions.None, null, views));
        
        // _progressIndicator.StartAnimation(this);
    }

    public void SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled) {
        _actionButton.Enabled = shouldBeEnabled;
    }

    public void Show() {
        Window.MakeKeyAndOrderFront(null);
    }

    public void OnDownloadProgressChanged(object sender, ItemDownloadProgressEventArgs args) {
        if (args.ProgressPercentage == 100) {
            _progressLabel.StringValue = "Download complete";
            _progressIndicator.DoubleValue = 100.0f;
            _actionButton.Enabled = false;
        } else {
            _progressLabel.StringValue = "Downloading update...";
            _progressIndicator.DoubleValue = args.ProgressPercentage * 1.0f;
        }
    }

    public void Close() {
        Window.Close();
    }

    public void FinishedDownloadingFile(bool isDownloadedFileValid) {
        if (isDownloadedFileValid) {
            _progressLabel.StringValue = "Ready to Install";
            _progressIndicator.DoubleValue = 100.0f;

            _actionButton.Enabled = true;
            _actionButton.Title = "Install and Relaunch";
            _isDownloadedFileValid = true;
        } else {
            if (!_cancelled) {
                DisplayErrorMessage("The downloaded file is not valid.");
            }
        }
    }

    public bool DisplayErrorMessage(string errorMessage) {
        if (_cancelled) return true;
        
        new NSAlert {
            AlertStyle = NSAlertStyle.Critical,
            InformativeText = errorMessage,
            MessageText = "Error",
        }.RunSheetModal(Window);
        
        return true;
    }
    
    [Export("actionButtonClicked:")]
    public void ActionButtonClicked(NSObject sender) {
        if (_isDownloadedFileValid) {
            DownloadProcessCompleted?.Invoke(this, new DownloadInstallEventArgs(true));
        } else {
            _cancelled = true;
            var appDelegate = NSApplication.SharedApplication.Delegate as AppDelegate;
            appDelegate?.Updater?.CancelFileDownload();
            DownloadProcessCompleted?.Invoke(this, new DownloadInstallEventArgs(false));
            Close();
        }
    }

    public event DownloadInstallEventHandler? DownloadProcessCompleted;
}