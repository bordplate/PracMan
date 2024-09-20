using NLua;
using TrainManCore.Scripting.UI;

namespace TrainMan;

public class TrainerViewController: NSViewController, IWindow {
    public NSWindow Window;
    
    LuaTable? _luaContext;

    private bool _viewLoaded = false;

    private NSStackView _stackView;
    
    public TrainerViewController() : base() {
        Window = new NSWindow(
            CGRect.Empty, 
            NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable, 
            NSBackingStore.Buffered, 
            true) {
            Title = "",
            ContentViewController = this,
        };
        
        Window.Center();
    }
    
    public override void ViewDidLoad() {
        base.ViewDidLoad();
        
        View.SetFrameSize(new CGSize(0, 0));

        _stackView = new NSStackView() {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Orientation = NSUserInterfaceLayoutOrientation.Horizontal,
            Distribution = NSStackViewDistribution.FillEqually,
            Spacing = 0,
        };
        
        View.AddSubview(_stackView);
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|[stackView]|", NSLayoutFormatOptions.None, null, new NSDictionary("stackView", _stackView)));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|[stackView]|", NSLayoutFormatOptions.None, null, new NSDictionary("stackView", _stackView)));

        _viewLoaded = true;
        
        if (_luaContext != null) {
            (_luaContext["OnLoad"] as LuaFunction)?.Call();
        }
    }
    
    public void SetLuaContext(LuaTable luaContext) {
        _luaContext = luaContext;
        
        // If the view is already loaded, call OnLoad
        if (_viewLoaded) {
            (_luaContext["OnLoad"] as LuaFunction)?.Call([_luaContext]);
        }
    }
    
    public void SetTitle(string title) {
        Window.Title = title;
    }

    public void Show() {
        Window.MakeKeyAndOrderFront(null);
    }

    public void Close() {
        throw new NotImplementedException();
    }

    public IContainer AddColumn() {
        var column = new TrainerUI.Column();
        
        _stackView.AddArrangedSubview(column);
        
        _stackView.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-10-[column]-10-|", NSLayoutFormatOptions.None, null, new NSDictionary("column", column)));
        column.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:[column(>=250)]", NSLayoutFormatOptions.None, null, new NSDictionary("column", column)));

        return column;
    }
}