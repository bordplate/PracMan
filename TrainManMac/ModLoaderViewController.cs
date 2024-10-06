using TrainManCore.Scripting;
using TrainManCore.Target;

namespace TrainMan;

public class ModLoaderViewController: NSViewController, INSTableViewDataSource, INSTableViewDelegate {
    public readonly NSWindow Window;

    private readonly Target _target;
    private List<Module> _mods;

    private readonly NSTableView _modsTableView;
    
    private readonly NSTextField _modNameTextField;
    private readonly NSTextField _modAuthorTextField;
    private readonly NSTextField _modVersionTextField;
    private readonly NSTextField _modLinkTextField;
    private readonly NSTextField _modDescriptionTextField;
    
    public ModLoaderViewController(Target target) : base() {
        _target = target;

        _mods = Application.GetModulesForTitle(target.TitleId);
        
        _modsTableView = new NSTableView {
            TranslatesAutoresizingMaskIntoConstraints = false,
            UsesAlternatingRowBackgroundColors = true,
            AllowsColumnResizing = false,
            AllowsColumnReordering = false,
            AllowsMultipleSelection = false,
            DataSource = this,
            Delegate = this,
        };
        
        _modNameTextField = new NSTextField {
            TranslatesAutoresizingMaskIntoConstraints = false,
            PlaceholderString = "N/A",
            DrawsBackground = false,
            Editable = false,
            Bordered = false,
            Selectable = true,
        };
        
        _modAuthorTextField = new NSTextField {
            TranslatesAutoresizingMaskIntoConstraints = false,
            PlaceholderString = "N/A",
            DrawsBackground = false,
            Editable = false,
            Bordered = false,
            Selectable = true,
        };
        
        _modVersionTextField = new NSTextField {
            PlaceholderString = "N/A",
            DrawsBackground = false,
            Editable = false,
            Bordered = false,
            Selectable = true,
        };
        
        _modLinkTextField = new NSTextField {
            PlaceholderString = "N/A",
            DrawsBackground = false,
            Editable = false,
            Bordered = false,
            Selectable = true,
        };
        
        _modDescriptionTextField = new NSTextField {
            TranslatesAutoresizingMaskIntoConstraints = false,
            PlaceholderString = "No description",
            DrawsBackground = false,
            Editable = false,
            Bordered = false,
            Selectable = true,
            UsesSingleLineMode = false,
            LineBreakMode = NSLineBreakMode.ByWordWrapping,
            MaximumNumberOfLines = 0,
        };

        Window = new NSWindow(
            CGRect.Empty, 
            NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable, 
            NSBackingStore.Buffered, 
            true) {
            Title = $"Mod Loader ({target.TitleId})",
            ContentViewController = this,
        };
        
        Window.Center();
        
        Window.DidBecomeKey += WindowDidBecomeKey;
    }
    
    public override void ViewDidLoad() {
        base.ViewDidLoad();
        
        View.SetFrameSize(new CGSize(150, 150));

        
        // Check column should not be resizable
        var checkColumn = new NSTableColumn("Check") {
            Title = "",
            Width = 16,
        };
        
        var nameColumn = new NSTableColumn("Name") {
            Title = "Name",
            Width = 100,
        };
        
        _modsTableView.AddColumn(checkColumn);
        _modsTableView.AddColumn(nameColumn);

        var scrollView = new NSScrollView {
            TranslatesAutoresizingMaskIntoConstraints = false,
        };
        
        scrollView.DocumentView = _modsTableView;
        scrollView.BorderType = NSBorderType.BezelBorder;
        
        View.AddSubview(scrollView);
        
        var horizontalStackView = new NSStackView {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Orientation = NSUserInterfaceLayoutOrientation.Horizontal,
            Distribution = NSStackViewDistribution.Fill,
            Spacing = 10,
        };

        View.AddSubview(horizontalStackView);

        var modInfoGrid = new NSGridView {
            TranslatesAutoresizingMaskIntoConstraints = false,
            RowSpacing = 10,
            ColumnSpacing = 10,
        };

        modInfoGrid.AddRow([
            new NSTextField {
                TranslatesAutoresizingMaskIntoConstraints = false,
                StringValue = "Name:",
                DrawsBackground = false,
                Editable = false,
                Bordered = false,
            }, 
            _modNameTextField
        ]);
        
        modInfoGrid.AddRow([
            new NSTextField {
                TranslatesAutoresizingMaskIntoConstraints = false,
                StringValue = "Author:",
                DrawsBackground = false,
                Editable = false,
                Bordered = false,
            }, 
            _modAuthorTextField
        ]);
        
        modInfoGrid.AddRow([
            new NSTextField {
                TranslatesAutoresizingMaskIntoConstraints = false,
                StringValue = "Version:",
                DrawsBackground = false,
                Editable = false,
                Bordered = false,
            }, 
            _modVersionTextField
        ]);
        
        modInfoGrid.AddRow([
            new NSTextField {
                TranslatesAutoresizingMaskIntoConstraints = false,
                StringValue = "Link:",
                DrawsBackground = false,
                Editable = false,
                Bordered = false,
            }, 
            _modLinkTextField
        ]);
        
        horizontalStackView.AddArrangedSubview(modInfoGrid);
        modInfoGrid.GetColumn(0).Width = 50;

        // Constrain modInfoGrid to Max Width of 300
        modInfoGrid.WidthAnchor.ConstraintLessThanOrEqualTo(250).Active = true;
        
        var descriptionView = new NSView {
            TranslatesAutoresizingMaskIntoConstraints = false,
        };
        
        horizontalStackView.AddArrangedSubview(descriptionView);
        descriptionView.AddSubview(_modDescriptionTextField);
        
        // Limit the width of the description text field to the of the available column width
        _modDescriptionTextField.LeadingAnchor.ConstraintEqualTo(descriptionView.LeadingAnchor).Active = true;
        _modDescriptionTextField.TrailingAnchor.ConstraintEqualTo(descriptionView.TrailingAnchor).Active = true;
        _modDescriptionTextField.TopAnchor.ConstraintEqualTo(descriptionView.TopAnchor).Active = true;
        
        _modDescriptionTextField.SetContentHuggingPriorityForOrientation(249, NSLayoutConstraintOrientation.Horizontal);
        _modDescriptionTextField.SetContentCompressionResistancePriority(1, NSLayoutConstraintOrientation.Horizontal);
        
        var buttonsStackView = new NSStackView {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Orientation = NSUserInterfaceLayoutOrientation.Vertical,
            Distribution = NSStackViewDistribution.FillEqually,
            Spacing = 10,
            Alignment = NSLayoutAttribute.Trailing
        };
        
        horizontalStackView.AddView(buttonsStackView, NSStackViewGravity.Trailing);
        
        var scriptingButton = new NSButton {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Title = "Scripting",
        };

        var consoleButton = new NSButton {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Title = "Console",
        };
        
        var addZipButton = new NSButton {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Title = "Add ZIP...",
        };
        
        scriptingButton.Action = new ObjCRuntime.Selector("scriptingButtonClicked:");
        consoleButton.Action = new ObjCRuntime.Selector("consoleButtonClicked:");
        addZipButton.Action = new ObjCRuntime.Selector("addZipButtonClicked:");
        
        buttonsStackView.AddArrangedSubview(scriptingButton);
        buttonsStackView.AddArrangedSubview(consoleButton);
        buttonsStackView.AddArrangedSubview(addZipButton);

        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-10-[scrollView(>=600)]-10-|", NSLayoutFormatOptions.None, null, new NSDictionary("scrollView", scrollView)));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-10-[scrollView(>=300)]-10-[bottomView]-10-|", NSLayoutFormatOptions.None, null, new NSDictionary("scrollView", scrollView, "bottomView", horizontalStackView)));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-10-[horizontalStackView]-10-|", NSLayoutFormatOptions.None, null, new NSDictionary("horizontalStackView", horizontalStackView)));
        
        modInfoGrid.SetContentHuggingPriorityForOrientation(251, NSLayoutConstraintOrientation.Horizontal);
        buttonsStackView.SetContentHuggingPriorityForOrientation(251, NSLayoutConstraintOrientation.Horizontal);
    }
    
    public nint GetRowCount(NSTableView tableView) {
        return _mods.Count;
    }
    
    public NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, nint row) {
        var mod = _mods[(int)row];
        
        if (tableColumn.Identifier == "Check") {
            var view = new NSView {
                TranslatesAutoresizingMaskIntoConstraints = true,
                AutoresizesSubviews = true
            };
            
            var checkBox = new NSButton {
                TranslatesAutoresizingMaskIntoConstraints = true,
                Title = "",
                BezelStyle = NSBezelStyle.RegularSquare,
                State = _target.Modules.Find(m => m.Path == _mods[(int)row].Path) != null
                    ? NSCellStateValue.On : NSCellStateValue.Off
            };
            
            checkBox.Tag = (int)row;
            
            checkBox.SetButtonType(NSButtonType.Switch);
            
            view.AddSubview(checkBox);
            
            // Add autoresizing constraints to checkbox, not autolayout constraints
            checkBox.AutoresizingMask = NSViewResizingMask.NotSizable;
            checkBox.SetFrameSize(new CGSize(18, 18));
            checkBox.SetFrameOrigin(new CGPoint(1, 2));
            
            checkBox.Action = new ObjCRuntime.Selector("checkBoxClicked:");
            checkBox.Target = this;
            
            return view;
        } else if (tableColumn.Identifier == "Name") {
            var cell = new NSTableCellView() {
                VerticalContentSizeConstraintActive = true,
            };
            
            var textField = new NSTextField(frameRect: new CGRect(0, 0, tableColumn.Width, 20)) {
                DrawsBackground = false,
                Bordered = false,
                Editable = false,
                Selectable = false,
                StringValue = mod.Settings.Get("General.name", mod.Identifier)!,
            };
            
            cell.AddSubview(textField);
            
            cell.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-[textField]-|", NSLayoutFormatOptions.AlignAllCenterY, null, new NSDictionary("textField", textField)));
            
            return cell;
        }
        
        return new NSView();
    }
    
    public void SelectionDidChange(NSNotification notification) {
        if (notification.Object is not NSTableView tableView) {
            return;
        }

        int row = (int)tableView.SelectedRow;
        
        if (row < 0) {
            return;
        }
        
        var mod = _mods[row];
        
        _modNameTextField.StringValue = mod.Settings.Get("General.name", "")!;
        _modAuthorTextField.StringValue = mod.Settings.Get("General.author", "")!;
        _modVersionTextField.StringValue = mod.Settings.Get("General.version", "")!;
        _modLinkTextField.StringValue = mod.Settings.Get("General.link", "")!;
        _modDescriptionTextField.StringValue = mod.Settings.Get("General.description", "")!;
    }
    
    public void WindowDidBecomeKey(object? sender, EventArgs eventArgs) {
        _mods = Application.GetModulesForTitle(_target.TitleId);
        _modsTableView.ReloadData();
    }
    
    public override void ViewWillDisappear() {
        base.ViewWillDisappear();

        if (_target.Modules.Count == 0) {
            _target.Stop();
        }
    }
    
    [Export("checkBoxClicked:")]
    public void CheckBoxClicked(NSObject sender) {
        if (sender is not NSButton checkBox) {
            return;
        }
        
        int row = (int)checkBox.Tag;
        bool isChecked = checkBox.State == NSCellStateValue.On;
        
        if (isChecked) {
            _mods[row].Load(_target);
        } else {
            // Find the mod in the target's modules and unload it
            var mod = _target.Modules.Find(m => m.Path == _mods[row].Path);
            mod?.Exit();
        }
    }
    
    [Export("scriptingButtonClicked:")]
    public void ScriptingButtonClicked(NSObject sender) {
        var module = new Module(_target.TitleId, Path.Combine("Scripts", "Runtime"));
        module.Load(_target);
        
        module.Run("ScriptWindow(true):Show()");
    }
    
    [Export("consoleButtonClicked:")]
    public void ConsoleButtonClicked(NSObject sender) {
        ((AppDelegate)NSApplication.SharedApplication.Delegate).OpenConsole();
    }
    
    [Export("addZipButtonClicked:")]
    public void AddZipButtonClicked(NSObject sender) {
        var openPanel = new NSOpenPanel {
            CanChooseFiles = true,
            CanChooseDirectories = false,
            AllowsMultipleSelection = false,
            AllowedFileTypes = ["zip"],
        };
        
        openPanel.BeginSheet(Window, result => {
            if (result == 1) {
                var path = openPanel.Url.Path!;
                
                Application.InstallModuleFromZip(_target.TitleId, path);
            }
        });
    }
}