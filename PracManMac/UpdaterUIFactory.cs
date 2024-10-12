using NetSparkleUpdater;
using NetSparkleUpdater.Interfaces;

namespace PracManMac;

public class UpdaterUIFactory: IUIFactory {
    UpdateViewController? _updateViewController;
    
    public IUpdateAvailable CreateUpdateAvailableWindow(List<AppCastItem> updates, ISignatureVerifier? signatureVerifier,
        string currentVersion = "", string appName = "the application", bool isUpdateAlreadyDownloaded = false) {
        foreach (var update in updates) {
            if (update.IsMacOSUpdate) {
                // TODO: Actually check optional version and release notes link
                _updateViewController = new UpdateViewController(appName, currentVersion, update.Version!, update.ReleaseNotesLink!, update);
               return _updateViewController;
            }
        }

        return null!;
    }

    public IDownloadProgress CreateProgressWindow(string downloadTitle, string actionButtonTitleAfterDownload) {
        return new UpdateProgressViewController(downloadTitle, actionButtonTitleAfterDownload);
    }

    public ICheckingForUpdates ShowCheckingForUpdates() {
        return new CheckingForUpdatesViewController();
    }

    public void ShowUnknownInstallerFormatMessage(string downloadFileName) {
        new NSAlert {
            AlertStyle = NSAlertStyle.Critical,
            InformativeText = $"Someone has uploaded the wrong file as the new file. Go complain to then.\n" +
                              $"The downloaded file '{downloadFileName}' is not a recognized installer format.",
            MessageText = "Error",
        }.RunModal();
    }

    public void ShowVersionIsUpToDate() {
        new NSAlert {
            AlertStyle = NSAlertStyle.Informational,
            InformativeText = "You are on the latest version.",
            MessageText = "Up to Date",
        }.RunModal();
    }

    public void ShowVersionIsSkippedByUserRequest() {
        var alert = new NSAlert {
            AlertStyle = NSAlertStyle.Informational,
            InformativeText = "You have chosen to skip the next version.",
            MessageText = "Version Skipped",
        };
        
        alert.AddButton("Install Anyway");
        alert.AddButton("Cancel");
        
        var result = alert.RunModal();
        
        var appDelegate = (AppDelegate)NSApplication.SharedApplication.Delegate;
        
        if (result == 1000) {
            appDelegate.Updater?.Configuration.SetVersionToSkip("0.0");
            appDelegate.Updater?.ShowUpdateNeededUI();
        }
    }

    public void ShowCannotDownloadAppcast(string? appcastUrl) {
        new NSAlert {
            AlertStyle = NSAlertStyle.Critical,
            InformativeText = "Could not download the update information. Please try again later.",
            MessageText = "Error",
        }.RunModal();
    }

    public bool CanShowToastMessages() {
        return true;
    }

    public void ShowToast(Action clickHandler) {
        // Show notification
        var notification = new NSUserNotification {
            Title = "Update Available",
            InformativeText = "An update is available for the application.",
            HasActionButton = true,
            ActionButtonTitle = "Install",
        };
        
        NSUserNotificationCenter.DefaultUserNotificationCenter.DeliverNotification(notification);
    }

    public void ShowDownloadErrorMessage(string message, string? appcastUrl) {
        throw new NotImplementedException();
    }

    public void Shutdown() {
        
    }

    public bool HideReleaseNotes { get; set; }
    public bool HideSkipButton { get; set; }
    public bool HideRemindMeLaterButton { get; set; }
    public string? ReleaseNotesHTMLTemplate { get; set; }
    public string? AdditionalReleaseNotesHeaderHTML { get; set; }
}

class CheckingForUpdatesViewController: NSViewController, ICheckingForUpdates {
    public NSWindow Window;
    
    public CheckingForUpdatesViewController() {
        Window = new NSWindow(
            CGRect.Empty, 
            NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable, 
            NSBackingStore.Buffered, 
            true) {
            Title = "Checking for Updates",
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
        
        var progressIndicator = new NSProgressIndicator {
            Style = NSProgressIndicatorStyle.Bar,
            ControlSize = NSControlSize.Regular,
            TranslatesAutoresizingMaskIntoConstraints = false,
            Indeterminate = true,
        };
        
        View.AddSubview(progressIndicator);
        
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-20-[progressIndicator(250)]-20-|", NSLayoutFormatOptions.AlignAllCenterX, null, new NSDictionary("progressIndicator", progressIndicator)));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-20-[progressIndicator]-20-|", NSLayoutFormatOptions.AlignAllCenterX, null, new NSDictionary("progressIndicator", progressIndicator)));
        
        progressIndicator.StartAnimation(this);
    }

    public void Show() {
        Window.MakeKeyAndOrderFront(this);
    }

    public void Close() {
        Window.Close();
    }

    public event EventHandler? UpdatesUIClosing;
}