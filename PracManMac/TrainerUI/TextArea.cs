using NLua;
using PracManCore.Scripting.UI;

namespace PracManMac.TrainerUI;

public class TextArea : NSScrollView, ITextArea {
    public IWindow Window { get; }
    public int Rows { get; }
    private NSTextView _textView;

    public TextArea(IWindow window, int rows) {
        Window = window;
        Rows = rows;

        _textView = new NSTextView {
            DrawsBackground = true,
            Editable = true,
            Selectable = true,
            VerticallyResizable = true,
            TranslatesAutoresizingMaskIntoConstraints = false,
            AutoresizingMask = NSViewResizingMask.HeightSizable,
        };
        
        _textView.MinSize = new CGSize(ContentSize.Width, rows * 20);
        _textView.MaxSize = new CGSize(float.MaxValue, float.MaxValue);
        
        _textView.TextContainer.ContainerSize = new CGSize(ContentView.Frame.Width, float.MaxValue);
        _textView.TextContainer.WidthTracksTextView = true;
        
        _textView.TextDidChange += (sender, e) => {
            _textView.Frame = new CGRect(0, 0, ContentView.Frame.Width, _textView.LayoutManager!.GetUsedRect(_textView.TextContainer).Height);
        };

        DocumentView = _textView;
        
        HasVerticalScroller = true;
        AutohidesScrollers = true;
        BorderType = NSBorderType.BezelBorder;
        
        AddConstraints(NSLayoutConstraint.FromVisualFormat("|[_textView]|", NSLayoutFormatOptions.None, null, new NSDictionary("_textView", _textView)));
        AddConstraint(NSLayoutConstraint.Create(this, NSLayoutAttribute.Height, NSLayoutRelation.GreaterThanOrEqual, null, NSLayoutAttribute.NoAttribute, 0, rows * 20));
    }

    public override void ViewDidMoveToSuperview() {
        base.ViewDidMoveToSuperview();

        _textView.MinSize = new CGSize(Frame.Width, Rows * 20);
        _textView.Frame = new CGRect(0, 0, Frame.Width, _textView.LayoutManager!.GetUsedRect(_textView.TextContainer).Height);
    }

    public string GetText() {
        return _textView.Value;
    }

    public void SetText(string text) {
        _textView.Value = text;
        
        _textView.DidChangeText();
    }

    public void SetMonospaced(bool monospaced) {
        _textView.Font = monospaced ? NSFont.FromFontName("Menlo", 12)! : NSFont.FromFontName("Helvetica", 12)!;
    }
}