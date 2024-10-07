using System.Data;
using CoreAnimation;
using PracManCore.Scripting;
using PracManCore.Scripting.UI;

namespace PracManMac;

public class ButtonCombosViewController: NSViewController, INSTableViewDelegate, INSTableViewDataSource, IButtonListener {
    public readonly NSWindow Window;

    private readonly Inputs _inputs;
    private bool _shouldReactivateButtonCombos;
    
    private readonly NSTableView _combosTableView;
    private readonly NSSegmentedControl _segmentedControl;
    
    private NSTextField? _selectButtonLabel;
    private NSTextField? _selectComboLabel;
    
    private IButton? _selectedButton;
    private HashSet<Inputs.Buttons> _currentCombo = [];
    
    public ButtonCombosViewController(Inputs inputs) {
        _inputs = inputs;
        
        // Add a segmented control to the bottom of the table view for + and - buttons
        _segmentedControl = new NSSegmentedControl {
            TranslatesAutoresizingMaskIntoConstraints = false,
            SegmentStyle = NSSegmentStyle.SmallSquare,
            TrackingMode = NSSegmentSwitchTracking.Momentary,
            SegmentCount = 3,
            Action = new ObjCRuntime.Selector("segmentedControlAction:"),
        };
        
        _combosTableView = new NSTableView {
            TranslatesAutoresizingMaskIntoConstraints = false,
            UsesAlternatingRowBackgroundColors = true,
            AllowsColumnResizing = false,
            AllowsColumnReordering = false,
            AllowsMultipleSelection = false,
            DataSource = this,
            Delegate = this,
        };
        
        _combosTableView.AddColumn(new NSTableColumn {
            Title = "Button name",
            Identifier = "Button name",
            Width = 200,
        });
        
        _combosTableView.AddColumn(new NSTableColumn {
            Title = "Combination",
            Identifier = "Combination",
            Width = 200,
        });
        
        Window = new NSWindow(
            CGRect.Empty, 
            NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable, 
            NSBackingStore.Buffered, 
            true) {
            Title = $"Configure button combos",
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
        
        var scrollView = new NSScrollView {
            TranslatesAutoresizingMaskIntoConstraints = false,
        };
        
        scrollView.DocumentView = _combosTableView;
        scrollView.BorderType = NSBorderType.BezelBorder;
        
        View.AddSubview(scrollView);
        
        _segmentedControl.SetImage(NSImage.ImageNamed(NSImageName.AddTemplate), 0);
        _segmentedControl.SetImage(NSImage.ImageNamed(NSImageName.RemoveTemplate), 1);
        _segmentedControl.SetLabel("", 2);
        _segmentedControl.SetWidth(20, 0);
        _segmentedControl.SetWidth(20, 1);
        _segmentedControl.SetEnabled(false, 2);
        
        if (_inputs.ButtonCombos().Count <= 0) {
            _segmentedControl.SetEnabled(false, 1);
        }
        
        View.AddSubview(_segmentedControl);
        
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-10-[scrollView(>=250)]-10-|", 
            NSLayoutFormatOptions.None, null, new NSDictionary("scrollView", scrollView)));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-10-[segmentedControl]-10-|", 
            NSLayoutFormatOptions.None, null, new NSDictionary("segmentedControl", _segmentedControl)));
        View.AddConstraints(
            NSLayoutConstraint.FromVisualFormat("V:|-10-[scrollView(>=250)][segmentedControl]-10-|", 
            NSLayoutFormatOptions.None, 
            null, 
            new NSDictionary("scrollView", scrollView, "segmentedControl", _segmentedControl))
        );
        
        _combosTableView.ReloadData();
    }
    
    [Export("segmentedControlAction:")]
    public void SegmentedControlAction(NSObject sender) {
        if (sender is not NSSegmentedControl segmentedControl) {
            return;
        }
        
        if (segmentedControl.SelectedSegment == 0) {
            _combosTableView.InsertRows(NSIndexSet.FromIndex(_combosTableView.RowCount), NSTableViewAnimation.SlideDown);
            Inputs.ButtonListener = this;
            
            segmentedControl.SetEnabled(false, 0);
        } else if (segmentedControl.SelectedSegment == 1) {
            var selectedRow = _combosTableView.SelectedRow;
            
            if (selectedRow < 0) {
                return;
            }
            
            if (selectedRow >= _inputs.ButtonCombos().Count) {
                _combosTableView.RemoveRows(NSIndexSet.FromIndex(selectedRow), NSTableViewAnimation.SlideUp);

                if (_selectedButton != null) {
                    _inputs.OnInput -= OnInput;

                    if (_shouldReactivateButtonCombos) {
                        _inputs.EnableButtonCombos();
                    }
                }
                
                _selectedButton = null;
                _currentCombo = new();
                segmentedControl.SetEnabled(true, 0);
                
                return;
            }
            
            _inputs.RemoveButtonCombo(_inputs.ButtonCombos()[(int)selectedRow]);
            _combosTableView.RemoveRows(NSIndexSet.FromIndex(selectedRow), NSTableViewAnimation.SlideUp);
            
            if (_combosTableView.RowCount <= 0) {
                segmentedControl.SetEnabled(false, 1);
            }
        } else {
            var alert = new NSAlert {
                AlertStyle = NSAlertStyle.Informational,
                InformativeText = "How did you get here?",
                MessageText = "What are you doing? Stop it. Get some help.",
            };
            alert.RunSheetModal(Window);
        }
    }
    
    public void SelectionDidChange(NSNotification notification) {
        if (_combosTableView.SelectedRow < 0) {
            _segmentedControl.SetEnabled(false, 1);
        } else {
            _segmentedControl.SetEnabled(true, 1);
        }
    }
    
    public nint GetRowCount(NSTableView tableView) {
        if (_inputs.ButtonCombos().Count > 0) {
            _segmentedControl.SetEnabled(true, 0);
        }
        
        return _inputs.ButtonCombos().Count;
    }
    
    public NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row) {
        var cell = new NSTableCellView();

        var label = new NSTextField {
            Editable = false,
            Bordered = false,
            DrawsBackground = false,
            TranslatesAutoresizingMaskIntoConstraints = false
        };
        
        cell.AddSubview(label);
        
        cell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-2-[label]-2-|", 
            NSLayoutFormatOptions.None, null, new NSDictionary("label", label)));
        cell.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-2-[label]-2-|", 
            NSLayoutFormatOptions.None, null, new NSDictionary("label", label)));
        
        if (row >= _inputs.ButtonCombos().Count) {
            if (_selectedButton == null && tableColumn.Identifier == "Button name") {
                label.Bordered = true;
                label.DrawsBackground = true;
                label.BecomeFirstResponder();
                label.StringValue = "Select a button...";
                
                _selectButtonLabel = label;
            } else if (tableColumn.Identifier == "Combination") {
                _selectComboLabel = label;
            }
            
            return cell;
        }
        
        var combo = _inputs.ButtonCombos()[(int)row];
        
        if (tableColumn.Identifier == "Button name") {
            label.StringValue = combo.Button.Title;
        } else {
            label.StringValue = combo.ToString();
        }
        
        return cell;
    }

    public void OnButtonPressed(IButton button) {
        Inputs.ButtonListener = null;
        
        _selectedButton = button;
        
        if (_selectButtonLabel != null) {
            _selectButtonLabel.StringValue = button.Title;
            _selectButtonLabel.Bordered = false;
            _selectButtonLabel.DrawsBackground = false;
        }
        
        if (_selectComboLabel != null) {
            if (_inputs.ButtonCombosListening) {
                _inputs.DisableButtonCombos();
                _shouldReactivateButtonCombos = true;
            }
            
            _inputs.OnInput += OnInput;
            _selectComboLabel.StringValue = "Press a button combination...";
            _selectComboLabel.Bordered = true;
            _selectComboLabel.DrawsBackground = true;
        }
    }
    
    public void OnInput(Inputs.InputState inputs) {
        if (_selectComboLabel == null || _selectedButton == null) {
            return;
        }

        if (inputs.ToString() != "") {
            _selectComboLabel.StringValue = inputs.ToString();
        }
        
        // If the user releases a button, stop listening for input
        if (_currentCombo.Count > inputs.Mask.Count) {
            _inputs.OnInput -= OnInput;

            if (_shouldReactivateButtonCombos) {
                _inputs.EnableButtonCombos();
            }
            
            _selectComboLabel.Bordered = false;
            _selectComboLabel.DrawsBackground = false;
            _selectComboLabel.StringValue = string.Join("+", _currentCombo.Select(button => button.ToString()).ToList());
            
            _inputs.AddOrUpdateButtonCombo(new Inputs.ButtonCombo(_selectedButton, _currentCombo));
            _selectedButton = null;
            _currentCombo = new();
            
            _combosTableView.ReloadData();
            
            return;
        }
        
        // Copy the current inputs to the current combo
        _currentCombo = new(inputs.Mask);
    }
}