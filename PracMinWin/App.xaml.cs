using Microsoft.UI.Windowing;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using PracManCore.Scripting;
using PracManCore.Target;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using System.Threading;
using Microsoft.UI.Dispatching;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PracMinWin;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Microsoft.UI.Xaml.Application, IApplication
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App() {
        InitializeComponent();
    }

    private static readonly Windows.UI.ViewManagement.UISettings _uiSettings = new Windows.UI.ViewManagement.UISettings();

    static public void ConfigureTitleBar(Window window) {
        _uiSettings.ColorValuesChanged += (sender, args) =>
        {
            // Run on Main Thread
            window.DispatcherQueue.TryEnqueue(() => {
                _configureTitleBar(window);
            });
        };

        _configureTitleBar(window); // Initial configuration
    }

    private static void _configureTitleBar(Window window) {
        // Get the AppWindow from the WindowId
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        if (appWindow is not null) {
            var titleBar = appWindow.TitleBar;

            // Determine the system theme
            var uiSettings = new Windows.UI.ViewManagement.UISettings();
            var backgroundColor = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background);

            bool isDarkMode = backgroundColor.R < 128 && backgroundColor.G < 128 && backgroundColor.B < 128;

            if (isDarkMode) {
                // Dark mode colors
                titleBar.ForegroundColor = Colors.White;
                titleBar.BackgroundColor = Colors.Black;
                titleBar.ButtonForegroundColor = Colors.White;
                titleBar.ButtonBackgroundColor = Colors.Black;
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 64, 64, 64); // Dark gray
                titleBar.ButtonPressedForegroundColor = Colors.Black;
                titleBar.ButtonPressedBackgroundColor = Colors.White;
            } else {
                // Light mode colors
                titleBar.ForegroundColor = Colors.Black;
                titleBar.BackgroundColor = Colors.White;
                titleBar.ButtonForegroundColor = Colors.Black;
                titleBar.ButtonBackgroundColor = Colors.White;
                titleBar.ButtonHoverForegroundColor = Colors.Black;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 200, 200, 200); // Light gray
                titleBar.ButtonPressedForegroundColor = Colors.White;
                titleBar.ButtonPressedBackgroundColor = Colors.Black;
            }
        }
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        PracManCore.Scripting.Application.Delegate = this;

        // Create a new window
        attachWindow = new AttachWindow();

        // Show the new window
        attachWindow.Activate();
    }

    public void OnModuleLoad(Module module, Target target) {
        var moduleDelegate = new ModuleDelegate();
        module.Delegate = moduleDelegate;
    }

    public void OpenModLoader(Target target) {
        throw new NotImplementedException("IApplication.OpenModLoader");
    }

    public void RunOnMainThread(Action action) {
        attachWindow?.DispatcherQueue.TryEnqueue(() => action());
    }

    public void Alert(string title, string message) {
        throw new NotImplementedException("IApplication.Alert");
    }

    public void ConfirmDialog(string title, string message, Action<bool> callback) {
        throw new NotImplementedException();
    }

    public string LoadFileFromDialog() {
        throw new NotImplementedException();
    }

    private AttachWindow? attachWindow;
}
