using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PracManCore;
using PracManCore.Target;
using PracManCore.Scripting;
using PracManCore.Scripting.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WinUIEx;

namespace PracMinWin;

public class AttachWindow : Window {
    private readonly List<Type> _targets = new();
    private Type _targetType = null!;

    private Grid _rootGrid;
    private StackPanel _segmentPanel;
    private AttachComboBox _targetComboBox;
    private Button _attachButton;
    private ProgressRing _progressRing;

    public AttachWindow() {
        // Basic Window setup
        Title = "Attach to Process";

        // Build the UI container
        _rootGrid = new Grid {
            RowDefinitions = {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            },
            ColumnDefinitions = {
                new ColumnDefinition { Width = new GridLength(50) },         // left margin
                new ColumnDefinition { Width = GridLength.Auto },           // segmented panel
                new ColumnDefinition { Width = new GridLength(50) },         // right margin (1)
                new ColumnDefinition { Width = GridLength.Auto },           // combo box
                new ColumnDefinition { Width = GridLength.Auto },           // attach button
                new ColumnDefinition { Width = GridLength.Auto },           // progress ring
                new ColumnDefinition { Width = new GridLength(20) }         // right margin (2)
            }
        };

        Content = _rootGrid;

        // Create a horizontal panel to mimic "segmented control" behavior
        _segmentPanel = new StackPanel {
            Orientation = Orientation.Horizontal
        };
        Grid.SetRow(_segmentPanel, 0);
        Grid.SetColumn(_segmentPanel, 1);
        Grid.SetColumnSpan(_segmentPanel, 5);
        _rootGrid.Children.Add(_segmentPanel);

        // The custom ComboBox
        _targetComboBox = new AttachComboBox {
            MinWidth = 150,
            IsEditable = true,
        };
        Grid.SetRow(_targetComboBox, 1);
        Grid.SetColumn(_targetComboBox, 3);
        _rootGrid.Children.Add(_targetComboBox);

        // Attach Button
        _attachButton = new Button {
            Content = "Attach"
        };
        _attachButton.Click += AttachButton_Click;
        Grid.SetRow(_attachButton, 1);
        Grid.SetColumn(_attachButton, 4);
        _rootGrid.Children.Add(_attachButton);

        // Progress Ring
        _progressRing = new ProgressRing {
            IsActive = false,
            Visibility = Visibility.Collapsed,
            Width = 20,
            Height = 20,
            Margin = new Thickness(10, 0, 0, 0)
        };
        Grid.SetRow(_progressRing, 1);
        Grid.SetColumn(_progressRing, 5);
        _rootGrid.Children.Add(_progressRing);

        // Simulate _targets
        _targets.Add(typeof(Ratchetron));
        _targets.Add(typeof(RPCS3));
#if DEBUG
        _targets.Add(typeof(DummyTarget));
#endif


        // Dynamically add “segmented” radio buttons
        for (int i = 0; i < _targets.Count; i++) {
            var t = _targets[i];
            var rb = new RadioButton {
                Content = InvokeStaticNameMethod(t),
                Tag = t,
                Margin = new Thickness(5, 0, 5, 0)
            };
            rb.Checked += SegmentButton_Checked;
            _segmentPanel.Children.Add(rb);

            // Default to the first target
            if (i == 0) {
                rb.IsChecked = true;
                _targetType = t;
            }
        }

        // You could replicate your old “LoadView” or “ViewDidLoad” logic here
        // For demonstration, call SetTargetType on the first target
        SetTargetType(_targets[0]);

        this.SetWindowSize(500, 150);

        App.ConfigureTitleBar(this);
    }

    private void SegmentButton_Checked(object sender, RoutedEventArgs e) {
        if (sender is RadioButton rb && rb.Tag is Type newTarget && newTarget != _targetType) {
            Console.WriteLine("target changed");
            SetTargetType(newTarget);
        }
    }

    private void AttachButton_Click(object sender, RoutedEventArgs e) {
        // This replicates your Attach() function
        Attach();
    }

    private void Attach() {
        // If you have an address from the ComboBox:
        var targetAddress = _targetComboBox.Text;

        if (string.IsNullOrWhiteSpace(targetAddress)) {
            // Show an error dialog – in WinUI, you could use a ContentDialog
            ShowAlert("Empty address", "Please enter a valid address.");
            return;
        }

        // Start spinner
        _progressRing.IsActive = true;
        _progressRing.Visibility = Visibility.Visible;
        _attachButton.IsEnabled = false;
        _targetComboBox.IsEnabled = false;

        // If you have a logic that saves settings:
        Settings.Default.Set($"Target.{_targetType.Name}.LastAddress", targetAddress);

        var target = CreateTargetInstance(targetAddress);

        target.Start((success, message) => {
            _progressRing.IsActive = false;
            _progressRing.Visibility = Visibility.Collapsed;
            _attachButton.IsEnabled = true;
            _targetComboBox.IsEnabled = true;

            if (success) {
                if (target.TitleId == "") {
                    ShowAlert("No game running", "You must run a game before attaching.");
                    target.Stop();
                    return;
                }

                var modules = PracManCore.Scripting.Application.GetModulesForTitle(target.TitleId);

                foreach (var module in modules) {
                    if (module.Settings.Get<bool>("General.autorun")) {
                        try {
                            module.Load(target);
                        } catch (ScriptException exception) {
                            ShowAlert("Failed to load", exception.Message);
                        } catch (Exception exception) {
                            ShowAlert("Failed to load", exception.Message);
                        }
                    }
                }
            } else {
                ShowAlert("Failed to attach", message ?? "An unknown error occurred.");
            }
        });

        _attachButton.Content = "Attach";
    }

    public void SetTargetType(Type targetType) {
        _targetType = targetType;

        // Load any previous address from settings if desired
        // var lastAddress = Settings.Default.Get($"Target.{_targetType.Name}.LastAddress", "") ?? "";
        var lastAddress = "";

        _targetComboBox.Text = lastAddress;

        // Suppose you want to do discovered target logic:
        // This is just example – you’d do it asynchronously in real code
        InvokeStaticDiscoverTargetsMethod(_targetType, discoveredTargets => {
            _targetComboBox.SetDiscovered(discoveredTargets);
        });

        // Recents
        // var recents = GetRecentItems(_targetType.Name);
        var recents = new List<string>();
        _targetComboBox.SetRecents(recents);
    }

    private void ShowAlert(string title, string message) {
        var dlg = new ContentDialog {
            Title = title,
            Content = message,
            PrimaryButtonText = "OK",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.Content.XamlRoot
        };
        _ = dlg.ShowAsync();
    }

    private Target CreateTargetInstance(string targetAddress) {
        // This mimics your dynamic instance creation
        // “Target” is presumably an interface or base from your code
        return (Target)Activator.CreateInstance(_targetType, targetAddress)!;
    }

    private string InvokeStaticNameMethod(Type targetType) {
        var method = targetType.GetMethod("Name", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (method == null)
            throw new InvalidOperationException($"{targetType.Name} does not have a Name method");
        return (string)method.Invoke(null, null)!;
    }

    private void InvokeStaticDiscoverTargetsMethod(Type targetType, Target.DicoveredTargetsCallback callback) {
        if (targetType.GetMethod("DiscoverTargets", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) is not { } discoverTargetsMethod) {
            return;
        }

        discoverTargetsMethod.Invoke(null, [callback]);
    }

    // If you’re storing recents, etc.:
    // private List<string> GetRecentItems(string targetName) { ... }
    // private void AddRecentItem(string targetName, string address) { ... }
}

/// <summary>
/// A ComboBox that mimics your custom Mac version: 
/// keeps track of “discovered” and “recent” items, and 
/// tries to block selecting headers. 
/// </summary>
public class AttachComboBox : ComboBox {
    private List<string> _discoveredItems = new();
    private List<string> _recentItems = new();
    private List<ComboBoxItemViewModel> _allItems = new();

    // Basic constructor
    public AttachComboBox() {
        SelectionChanged += AttachComboBox_SelectionChanged;
    }

    public void SetRecents(List<string> recentItems) {
        _recentItems = recentItems;
        UpdateAllItems();
    }

    public void SetDiscovered(List<string> discoveredItems) {
        _discoveredItems = discoveredItems;
        UpdateAllItems();
    }

    private void UpdateAllItems() {
        _allItems.Clear();

        if (_recentItems.Count > 0) {
            _allItems.Add(new ComboBoxItemViewModel("Recents", isSectionTitle: true));
            foreach (var item in _recentItems) {
                _allItems.Add(new ComboBoxItemViewModel(item));
            }
        }

        if (_discoveredItems.Count > 0) {
            _allItems.Add(new ComboBoxItemViewModel("Discovered", isSectionTitle: true));
            foreach (var item in _discoveredItems) {
                _allItems.Add(new ComboBoxItemViewModel(item));
            }
        }

        Items.Clear();
        foreach (var vm in _allItems) {
            // In code-only approach, we can treat the “section title” items distinctly
            ComboBoxItem cbi = new() {
                Content = vm.IsSectionTitle ? $"\t{vm.ItemText}" : vm.ItemText,
                IsEnabled = !vm.IsSectionTitle 
            };
            Items.Add(cbi);
        }
    }

    private void AttachComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
        // If the user picks a disabled item (a “header”), forcibly revert
        if (SelectedItem is ComboBoxItem cbi && !cbi.IsEnabled) {
            SelectedItem = null;
        }
    }

    private class ComboBoxItemViewModel {
        public string ItemText { get; }
        public bool IsSectionTitle { get; }

        public ComboBoxItemViewModel(string text, bool isSectionTitle = false) {
            ItemText = text;
            IsSectionTitle = isSectionTitle;
        }
    }
}