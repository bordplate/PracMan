using System.Reflection;
using PracManCore;
using PracManCore.Scripting;
using PracManCore.Scripting.Exceptions;
using PracManCore.Target;

namespace PracManMac;

public class AttachViewController: NSViewController {
    public readonly NSWindow Window;
    
    private readonly List<Type> _targets = [];
    private Type _targetType = null!;
    
    private readonly NSSegmentedControl _segmentedControl;
    private readonly AttachComboBox _targetComboBox;
    private readonly NSButton _attachButton;
    private readonly NSProgressIndicator _progressIndicator;
    
    public AttachViewController() : base(null, null) {
        _targets.Add(typeof(Ratchetron));
        _targets.Add(typeof(RPCS3));
        #if DEBUG
        _targets.Add(typeof(DummyTarget));
        #endif
        
        _segmentedControl = new NSSegmentedControl {
            SegmentStyle = NSSegmentStyle.TexturedRounded,
            TranslatesAutoresizingMaskIntoConstraints = false,
            SegmentCount = (nint)_targets.Count,
            SelectedSegment = (nint)0
        };
        
        _targetComboBox = new AttachComboBox {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Action = new ObjCRuntime.Selector("comboBoxAction:"),
            Target = this
        };
        
        _attachButton = new NSButton {
            Title = "Attach",
            BezelStyle = NSBezelStyle.Rounded,
            TranslatesAutoresizingMaskIntoConstraints = false,
            Action = new ObjCRuntime.Selector("attachButtonClicked:"),
            Target = this
        };

        _progressIndicator = new NSProgressIndicator {
            Indeterminate = true,
            Style = NSProgressIndicatorStyle.Spinning,
            ControlSize = NSControlSize.Small,
            TranslatesAutoresizingMaskIntoConstraints = false,
            Hidden = true,
        };
        
        Window = new NSWindow(
            CGRect.Empty, 
            NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable, 
            NSBackingStore.Buffered, 
            true) {
            Title = "Attach to Process",
            ContentViewController = this,
        };
        
        Window.Center();
        
        Window.DidBecomeKey += (sender, e) => {
            ((AppDelegate)NSApplication.SharedApplication.Delegate).ActivateMenu();
        };
    }

    public override void LoadView() {
        // If we don't include this, the application crashed on macOS <= 13.0 when we initialize the controller
        View = new NSView();
    }

    public override void ViewDidLoad() {
        base.ViewDidLoad();
        
        // Set window to minimum size so auto layout resizes it
        View.SetFrameSize(new CGSize(0, 0));
        
        for (var i = 0; i < _targets.Count; i++) {
            var targetType = _targets[i];
            
            _segmentedControl.SetLabel(InvokeStaticNameMethod(targetType), (nint)i);
        }
        
        _segmentedControl.Action = new ObjCRuntime.Selector("segmentedControlClicked:");
        _segmentedControl.Target = this;
        
        View.AddSubview(_segmentedControl);
        View.AddSubview(_targetComboBox);
        View.AddSubview(_attachButton);
        View.AddSubview(_progressIndicator);
        
        var views = new NSDictionary(
            "comboBox", _targetComboBox, 
            "button", _attachButton, 
            "segmentedControl", _segmentedControl, 
            "progressIndicator", _progressIndicator
        );
        
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-50-[segmentedControl]-50-|", NSLayoutFormatOptions.AlignAllTop, null, views));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-50-[comboBox(>=150)]-10-[button]-[progressIndicator]-20-|", NSLayoutFormatOptions.AlignAllTop, null, views));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-20-[segmentedControl]-20-[comboBox]-50-|", NSLayoutFormatOptions.None, null, views));
        
        SetTargetType(_targets[0]);
    }
    
    public void SetTargetType(Type targetType) {
        _targetType = targetType;

        _targetComboBox.StringValue = Settings.Default.Get($"Target.{_targetType.Name}.LastAddress", "")!;
        _targetComboBox.PlaceholderString = InvokeStaticPlaceholderAddressMethod(targetType);
        
        _targetComboBox.SetRecents(GetRecentItems(_targetType.Name));
        InvokeStaticDiscoverTargetsMethod(targetType, (discoveredTargets) => {
            _targetComboBox.SetDiscovered(discoveredTargets);
        });
    }

    public void Attach() {
        var appDelegate = (AppDelegate)NSApplication.SharedApplication.Delegate;
        
        var targetAddress = _targetComboBox.StringValue;

        var alert = new NSAlert {
            AlertStyle = NSAlertStyle.Critical
        };
        
        if (targetAddress == "") {
            alert.MessageText = "Emtpy address";
            alert.InformativeText = "Please enter a valid address";
            
            alert.RunSheetModal(Window);
            
            return;
        }
        
        var target = CreateTargetInstance(targetAddress);
        
        _progressIndicator.Hidden = false;
        _progressIndicator.StartAnimation(this);
        
        _attachButton.Enabled = false;
        _targetComboBox.Enabled = false;
        _segmentedControl.Enabled = false;
        
        Settings.Default.Set($"Target.{_targetType.Name}.LastAddress", targetAddress);
        
        target.Start((success, message) => {
            _progressIndicator.Hidden = true;
            _attachButton.Enabled = true;
            _segmentedControl.Enabled = true;
            _targetComboBox.Enabled = true;
            _targetComboBox.BecomeFirstResponder();

            if (success) {
                AddRecentItem(_targetType.Name, targetAddress);
                
                if (target.TitleId == "") {
                    alert.MessageText = "No game running";
                    alert.InformativeText = "You must run a game before attaching to the process.";

                    alert.RunSheetModal(Window);
                    
                    target.Stop();
                    
                    return;
                }
                
                target.OnStop += () => {
                    appDelegate.ActivateMenu();
                };

                var modules = Application.GetModulesForTitle(target.TitleId);

                bool atLeastOneModLoaded = false;

                foreach (var module in modules) {
                    if (module.Settings.Get<bool>("General.autorun")) {
                        try {
                            module.Load(target);
                        }
                        catch (ScriptException exception) {
                            Console.Error.WriteLine(exception.Message);
                            new NSAlert {
                                AlertStyle = NSAlertStyle.Critical,
                                InformativeText = exception.Message,
                                MessageText = "Error loading module",
                            }.RunModal();
                        } catch {
                            new NSAlert {
                                AlertStyle = NSAlertStyle.Critical,
                                InformativeText = "An unknown error occurred while loading the module.",
                                MessageText = "Error loading module",
                            }.RunModal();
                        }
                        atLeastOneModLoaded = true;
                    }
                }

                if (!atLeastOneModLoaded) {
                    Application.Delegate?.OpenModLoader(target);
                }
                
                Window.Close();
            } else {
                alert.MessageText = "Failed to attach";
                alert.InformativeText = message ?? "";
                
                alert.RunSheetModal(Window);
            }
            
            _attachButton.Title = "Attach";
        });
    }
    
    [Export("comboBoxAction:")]
    public void ComboBoxAction(NSComboBox comboBox) {
        Attach();
    }
    
    [Export("attachButtonClicked:")]
    public void AttachButtonClicked(NSButton button) {
        Attach();
    }
    
    [Export("segmentedControlClicked:")]
    public void SegmentedControlClicked(NSSegmentedControl segmentedControl) {
        if (_targetType == _targets[(int)segmentedControl.SelectedSegment]) {
            return;
        }
        
        Console.WriteLine("target changed");
        
        SetTargetType(_targets[(int)segmentedControl.SelectedSegment]);
    }
    
    private string InvokeStaticNameMethod(Type targetType) {
        ArgumentNullException.ThrowIfNull(targetType);

        if (targetType.GetMethod("Name", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) is
            not { } nameMethod) {
            throw new InvalidOperationException($"{targetType.Name} does not have a Name method");
        }
        
        return (string)nameMethod.Invoke(null, null)!;
    }
    
    private void InvokeStaticDiscoverTargetsMethod(Type targetType, Target.DicoveredTargetsCallback callback) {
        if (targetType.GetMethod("DiscoverTargets", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) is not { } discoverTargetsMethod) {
            return;
        }
        
        discoverTargetsMethod.Invoke(null, [callback]);
    }
    
    private string InvokeStaticPlaceholderAddressMethod(Type targetType) {
        if (targetType.GetMethod("PlaceholderAddress", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) is not { } placeholderAddressMethod) {
            throw new InvalidOperationException($"{targetType.Name} does not have a PlaceholderAddress method");
        }
        
        return (string)placeholderAddressMethod.Invoke(null, null)!;
    }
    
    private Target CreateTargetInstance(string targetAddress) {
        return (Target)Activator.CreateInstance(_targetType, targetAddress)!;
    }

    private List<string> GetRecentItems(string targetName) {
        return Settings.Default.Get($"Target.{targetName}.RecentAddresses", new List<string>())!;
    }

    private void AddRecentItem(string targetName, string address) {
        var recentItems = GetRecentItems(targetName);

        if (recentItems.Contains(address)) {
            recentItems.Remove(address);
        }

        recentItems.Insert(0, address);

        if (recentItems.Count > 5) {
            recentItems.RemoveAt(5);
        }

        Settings.Default.Set($"Target.{targetName}.RecentAddresses", recentItems);
    }
}

public class AttachComboBox : NSComboBox, INSComboBoxDelegate, INSComboBoxDataSource {
    private List<string> _discoveredItems = [];
    private List<string> _recentItems = [];
    private List<ComboBoxItem> _allItems = [];
    
    private int _lastSelectedIndex = -1;
    
    public AttachComboBox() {
        UsesDataSource = true;
        
        Delegate = this;
        DataSource = this;
        
        Completes = true;
    }

    public void Clear() {
        _discoveredItems = new List<string>();
        _recentItems = new List<string>();
        UpdateAllItems();
        
        ReloadData();
        
        // Clear the text field
        StringValue = "";
    }

    public void UpdateAllItems() {
        _allItems = new List<ComboBoxItem>();
        
        if (_recentItems.Count > 0) {
            _allItems.Add(new ComboBoxItem("Recents", true));
            foreach (var item in _recentItems) {
                _allItems.Add(new ComboBoxItem(item));
            }
        }
        
        if (_discoveredItems.Count > 0) {
            _allItems.Add(new ComboBoxItem("Discovered", true));
            foreach (var item in _discoveredItems) {
                _allItems.Add(new ComboBoxItem(item));
            }
        }
    }
    
    public void SetRecents(List<string> recentItems) {
        _recentItems = recentItems;
        UpdateAllItems();
    }
    
    public void SetDiscovered(List<string> discoveredItems) {
        _discoveredItems = discoveredItems;
        UpdateAllItems();
    }
    
    public nint ItemCount(NSComboBox comboBox) {
        return _allItems.Count;
    }
    
    public NSObject ObjectValueForItem(NSComboBox comboBox, nint index) {
        if (_allItems[(int)index].IsSectionTitle) {
            return new NSString($"\t{_allItems[(int)index].ItemText}");
        }
        
        return new NSString(_allItems[(int)index].ItemText);
    }
    
    // Prevent selecting section titles or seperators by intercepting the selection change
    [Export("comboBoxSelectionIsChanging:")]
    public void SelectionIsChanging(NSNotification notification) {
        if (notification.Object is not NSComboBox comboBox) {
            return;
        }
        
        var selectedIndex = comboBox.SelectedIndex;
        if (selectedIndex >= 0 && _allItems[(int)selectedIndex].IsSectionTitle) {
            comboBox.DeselectItem(selectedIndex);
        }
        
        _lastSelectedIndex = (int)selectedIndex;
    }
    
    [Export("comboBoxSelectionDidChange:")]
    public void SelectionDidChange(NSNotification notification) {
        if (notification.Object is not NSComboBox comboBox) {
            return;
        }
        
        if (comboBox.SelectedIndex < 0) {
            return;
        }

        var index = comboBox.SelectedIndex;
        
        if (_allItems[(int)comboBox.SelectedIndex].IsSectionTitle) {
            comboBox.DeselectItem(index);
            
            if (_lastSelectedIndex < index && _allItems.Count > index) {
                comboBox.SelectItem(index+1);
            } else if (_lastSelectedIndex > index && index > 0) {
                comboBox.SelectItem(index-1);
            }
        } else {
           comboBox.SelectItem(index); 
        }
        
        _lastSelectedIndex = (int)index;
    }
}

public class ComboBoxItem {
    public string ItemText { get; set; }
    public bool IsSectionTitle { get; set; }

    public ComboBoxItem(string itemText, bool isSectionTitle = false) {
        ItemText = itemText;
        IsSectionTitle = isSectionTitle;
    }
}