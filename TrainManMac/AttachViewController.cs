using System.Reflection;
using TrainManCore.Target;
using TrainManCore.Scripting;
using Module = TrainManCore.Scripting.Module;

namespace TrainMan;

[Register("AttachWindowController")]
public class AttachViewController: NSViewController {
    public NSWindow Window;
    
    private List<Type> _targets = [];
    private Type _targetType;

    private AttachComboBox _targetComboBox;
    private NSButton _attachButton;
    
    public AttachViewController() : base() {
        _targets.Add(typeof(Ratchetron));
        _targets.Add(typeof(RPCS3));
        _targets.Add(typeof(DummyTarget));

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
    
    public override void ViewDidLoad() {
        base.ViewDidLoad();
        
        // Set window to minimum size so auto layout resizes it
        View.SetFrameSize(new CGSize(0, 0));
        
        var segmentedControl = new NSSegmentedControl {
            SegmentStyle = NSSegmentStyle.TexturedRounded,
            TranslatesAutoresizingMaskIntoConstraints = false,
            SegmentCount = (nint)_targets.Count,
            SelectedSegment = (nint)0
        };
        
        for (var i = 0; i < _targets.Count; i++) {
            var targetType = _targets[i];
            
            segmentedControl.SetLabel(InvokeStaticNameMethod(targetType), (nint)i);
        }
        
        segmentedControl.Action = new ObjCRuntime.Selector("segmentedControlClicked:");
        segmentedControl.Target = this;
        
        View.AddSubview(segmentedControl);
        
        _targetComboBox = new AttachComboBox {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Action = new ObjCRuntime.Selector("comboBoxAction:"),
            Target = this
        };
        
        View.AddSubview(_targetComboBox);
        
        _attachButton = new NSButton {
            Title = "Attach",
            BezelStyle = NSBezelStyle.Rounded,
            TranslatesAutoresizingMaskIntoConstraints = false,
            Action = new ObjCRuntime.Selector("attachButtonClicked:"),
            Target = this
        };
        View.AddSubview(_attachButton);
        
        var views = new NSDictionary("comboBox", _targetComboBox, "button", _attachButton, "segmentedControl", segmentedControl);
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-50-[segmentedControl]-50-|", NSLayoutFormatOptions.AlignAllTop, null, views));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-50-[comboBox(>=150)]-10-[button]-50-|", NSLayoutFormatOptions.AlignAllTop, null, views));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-20-[segmentedControl]-20-[comboBox]-50-|", NSLayoutFormatOptions.None, null, views));
        
        SetTargetType(_targets[0]);
    }
    
    public void SetTargetType(Type targetType) {
        _targetType = targetType;

        _targetComboBox.Clear();
        _targetComboBox.PlaceholderString = InvokeStaticPlaceholderAddressMethod(targetType);
        
        InvokeStaticDiscoverTargetsMethod(targetType, (discoveredTargets) => {
            _targetComboBox.SetDiscovered(discoveredTargets);
        });
    }

    public void Attach() {
        var targetAddress = _targetComboBox.StringValue;

        _attachButton.Title = "Cancel";

        var alert = new NSAlert {
            AlertStyle = NSAlertStyle.Critical
        };
        
        if (targetAddress == "") {
            alert.MessageText = "Emtpy address";
            alert.InformativeText = "Please enter a valid address";
            
            alert.RunSheetModal(Window);
            
            _attachButton.Title = "Attach";
            
            return;
        }
        
        var target = CreateTargetInstance(targetAddress);
        
        target.Start((success, message) => {
            if (success) {
                if (target.GetGameTitleID() == "") {
                    alert.MessageText = "No game running";
                    alert.InformativeText = "You must run a game before attaching to the process.";

                    alert.RunSheetModal(Window);
                    
                    _attachButton.Title = "Attach";

                    target.Stop();
                    
                    return;
                }

                var modules = Module.ModulesForTitle(target.GetGameTitleID(), target);

                foreach (var module in modules) {
                    if (module.Settings.Get<bool>("General.autorun")) {
                        var trainerModule = new TrainerModule();
                        module.TrainerDelegate = trainerModule;
                        
                        module.Load();
                    }
                }
                
                Window.Close();
            } else {
                alert.MessageText = "Failed to attach";
                alert.InformativeText = message;
                
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
        MethodInfo nameMethod = targetType.GetMethod("Name", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        
        if (nameMethod != null) {
            return (string)nameMethod.Invoke(null, null);
        }
        
        throw new InvalidOperationException($"{targetType.Name} does not have a Name method");
    }
    
    private void InvokeStaticDiscoverTargetsMethod(Type targetType, Target.DicoveredTargetsCallback callback) {
        MethodInfo discoverTargetsMethod = targetType.GetMethod("DiscoverTargets", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        
        if (discoverTargetsMethod != null) {
            discoverTargetsMethod.Invoke(null, [callback]);
        }
    }
    
    private string InvokeStaticPlaceholderAddressMethod(Type targetType) {
        MethodInfo placeholderAddressMethod = targetType.GetMethod("PlaceholderAddress", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        
        if (placeholderAddressMethod != null) {
            return (string)placeholderAddressMethod.Invoke(null, null);
        }
        
        throw new InvalidOperationException($"{targetType.Name} does not have a PlaceholderAddress method");
    }
    
    private Target CreateTargetInstance(string targetAddress) {
        return (Target)Activator.CreateInstance(_targetType, [targetAddress]);
    }
}

public class AttachComboBox : NSComboBox, INSComboBoxDelegate, INSComboBoxDataSource {
    private List<string> _discoveredItems = [];
    private List<string> _recentItems = [];
    private List<ComboBoxItem> _allItems = [];
    
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
        var comboBox = notification.Object as NSComboBox;
        var selectedIndex = comboBox.SelectedIndex;
        
        if ((nint)selectedIndex >= 0 && _allItems[(int)selectedIndex].IsSectionTitle) {
            comboBox.DeselectItem(selectedIndex);
        }
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