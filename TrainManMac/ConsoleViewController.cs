using System.Text;

namespace TrainMan;

public class ConsoleViewController : NSViewController {
    public NSWindow Window { get; set; }
    private NSTextView _textView;

    public ConsoleViewController() {
        _textView = new NSTextView {
            TranslatesAutoresizingMaskIntoConstraints = false,
            Editable = false,
            Font = NSFont.MonospacedSystemFont(12, NSFontWeight.Regular)!,
            BackgroundColor = NSColor.Black,
            TextColor = NSColor.White,
            DrawsBackground = true,
        };

        Window = new NSWindow(
            new CGRect(0, 0, 500, 400),
            NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable,
            NSBackingStore.Buffered,
            true) {
            Title = "Console",
            ContentViewController = this,
        };

        Window.Center();
    }

    public override void ViewDidLoad() {
        base.ViewDidLoad();

        var scrollView = new NSScrollView {
            TranslatesAutoresizingMaskIntoConstraints = false,
            HasVerticalScroller = true,
            DocumentView = _textView,
            BackgroundColor = NSColor.ControlBackground,
            BorderType = NSBorderType.NoBorder,
        };
        
        scrollView.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-0-[textView]-0-|", 0,  null, new NSDictionary("textView", _textView)));

        View = new NSView { TranslatesAutoresizingMaskIntoConstraints = false };
        View.WantsLayer = true;
        View.Layer!.BackgroundColor = NSColor.Black.CGColor;
        View.AddSubview(scrollView);

        var viewsDict = NSDictionary.FromObjectsAndKeys(
            new NSObject[] { scrollView },
            new NSString[] { new NSString("scrollView") }
        );

        View.AddConstraints(NSLayoutConstraint.FromVisualFormat(
            "H:|-0-[scrollView(>=500)]-0-|",
            0,
            null,
            viewsDict
        ));
        View.AddConstraints(NSLayoutConstraint.FromVisualFormat(
            "V:|-0-[scrollView(>=400)]-0-|",
            0,
            null,
            viewsDict
        ));

        RedirectConsoleOutput();
    }

    private void RedirectConsoleOutput() {
        var outputWriter = new ConsoleWriter(_textView);
        var errorWriter = new ConsoleWriter(_textView, true);
        Console.SetOut(outputWriter);
        Console.SetError(errorWriter);
    }

    private class ConsoleWriter : TextWriter {
        private readonly NSTextView _textView;
        private readonly StringBuilder _buffer;
        private readonly bool _error;

        public ConsoleWriter(NSTextView textView, bool error = false) {
            _textView = textView;
            _buffer = new StringBuilder();
            _error = error;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value) {
            _buffer.Append(value);

            if (value == '\n') {
                Flush();
            }
        }

        public override void Flush() {
            if (_buffer.Length > 0) {
                var text = _buffer.ToString();
                _buffer.Clear();
                _textView.InvokeOnMainThread(() => {
                    _textView.TextStorage.Append(new NSAttributedString(text, new NSDictionary(
                        NSStringAttributeKey.ForegroundColor, _error ? NSColor.Red : NSColor.White, NSStringAttributeKey.Font, NSFont.MonospacedSystemFont(12, NSFontWeight.Regular)
                    )));
                    _textView.NeedsDisplay = true;
                    _textView.LayoutManager?.EnsureLayoutForTextContainer(_textView.TextContainer);
                    _textView.Frame = new CGRect(
                        _textView.Frame.X, 
                        _textView.Frame.Y, 
                        _textView.Frame.Width, 
                        _textView.LayoutManager!.GetUsedRect(_textView.TextContainer).Height
                    );
                    
                    _textView.ScrollRangeToVisible(new NSRange(_textView.TextStorage.Length - 1, 0));
                });
            }
        }
    }
}
