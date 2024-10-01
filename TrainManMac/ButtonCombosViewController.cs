using System.Data;
using CoreAnimation;
using TrainManCore.Scripting;
using TrainManCore.Scripting.UI;

namespace TrainMan;

public class ButtonCombosViewController: NSViewController, INSTableViewDelegate, INSTableViewDataSource, IButtonListener {
    public NSWindow Window;

    private Inputs _inputs;
    private NSTableView _combosTableView;
    
    private IButton _selectedButton;
    private Inputs.ButtonCombo _selectedCombo;
    
    private NSSegmentedControl _segmentedControl;

    private NSTextField _selectButtonLabel;
    private NSTextField _selectComboLabel;
    
    private HashSet<Inputs.Buttons> _currentCombo = new();
    
    public ButtonCombosViewController(Inputs inputs) {
        _inputs = inputs;
        
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

    public override void ViewDidLoad() {
        base.ViewDidLoad();
        
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
        
        var scrollView = new NSScrollView {
            TranslatesAutoresizingMaskIntoConstraints = false,
        };
        
        scrollView.DocumentView = _combosTableView;
        scrollView.BorderType = NSBorderType.BezelBorder;
        
        View.AddSubview(scrollView);
        
        // Add a segmented control to the bottom of the table view for + and - buttons
        _segmentedControl = new NSSegmentedControl {
            TranslatesAutoresizingMaskIntoConstraints = false,
            SegmentStyle = NSSegmentStyle.SmallSquare,
            TrackingMode = NSSegmentSwitchTracking.Momentary,
            SegmentCount = 3,
            Action = new ObjCRuntime.Selector("segmentedControlAction:"),
        };
        
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
        var segmentedControl = sender as NSSegmentedControl;
        
        if (segmentedControl.SelectedSegment == 0) {
            _combosTableView.InsertRows(NSIndexSet.FromIndex(_combosTableView.RowCount), NSTableViewAnimation.SlideDown);
            Inputs.ButtonListener = this;
            
            segmentedControl.SetEnabled(false, 0);

            _selectedCombo = new();
        } else if (segmentedControl.SelectedSegment == 1) {
            var selectedRow = _combosTableView.SelectedRow;
            
            if (selectedRow < 0) {
                return;
            }
            
            if (selectedRow >= _inputs.ButtonCombos().Count) {
                _combosTableView.RemoveRows(NSIndexSet.FromIndex(selectedRow), NSTableViewAnimation.SlideUp);
                _selectedButton = null;
                _selectedCombo = null;
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
        if (_segmentedControl == null) {
            return 0;
        }
        
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

            _selectedCombo.Button = button;
        }
        
        if (_selectComboLabel != null) {
            _inputs.OnInput += OnInput;
            _selectComboLabel.StringValue = "Press a button combination...";
            _selectComboLabel.Bordered = true;
            _selectComboLabel.DrawsBackground = true;
        }
    }
    
    public void OnInput(Inputs.InputState inputs) {
        if (_selectComboLabel == null) {
            return;
        }

        if (inputs.ToString() != "") {
            _selectComboLabel.StringValue = inputs.ToString();
        }
        
        // If the user releases a button, stop listening for input
        if (_currentCombo.Count > inputs.Mask.Count) {
            _inputs.OnInput -= OnInput;
            _selectComboLabel.Bordered = false;
            _selectComboLabel.DrawsBackground = false;
            _selectedCombo.Combo = new(_currentCombo);
            
            _selectComboLabel.StringValue = string.Join("+", _currentCombo.Select(button => button.ToString()).ToList());
            
            _inputs.AddOrUpdateButtonCombo(_selectedCombo);

            _selectedCombo = null;
            _selectedButton = null;
            _currentCombo = new();
            
            _combosTableView.ReloadData();
            
            return;
        }
        
        // Copy the current inputs to the current combo
        _currentCombo = new(inputs.Mask);
    }
}