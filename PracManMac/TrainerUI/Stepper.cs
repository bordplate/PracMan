using NLua;
using PracManCore.Scripting.UI;

namespace PracManMac.TrainerUI;

// Numeric stepper. NSTextField with NSStepper on the right.
public class Stepper: NSView, IStepper {
    public new IWindow Window { get; }
    
    private readonly NSTextField _textField;
    private readonly NSStepper _stepper;
    
    private readonly LuaFunction _callback;

    public Stepper(IWindow window, long minValue, long maxValue, int step, LuaFunction callback) {
        Window = window;
        _callback = callback;
        
        _textField = new NSTextField {
            StringValue = "0",
            Editable = true,
            Bezeled = true,
            Bordered = true,
            Formatter = new NumberFormatter(minValue, maxValue),
            Alignment = NSTextAlignment.Center
        };
        
        _stepper = new NSStepper {
            MinValue = minValue,
            MaxValue = maxValue,
            Increment = step,
            ValueWraps = false
        };
        
        _stepper.Activated += (sender, e) => {
            _textField.StringValue = _stepper.IntValue.ToString();
            
            ModuleDelegate.TryInvoke(Window, callback, _stepper.IntValue);
        };
        
        _textField.Changed += (sender, e) => {
            if (int.TryParse(_textField.StringValue, out var value)) {
                _stepper.IntValue = value;
                
                ModuleDelegate.TryInvoke(Window, callback, value);
            }
        };
        
        AddSubview(_textField);
        AddSubview(_stepper);
        
        _textField.TranslatesAutoresizingMaskIntoConstraints = false;
        _stepper.TranslatesAutoresizingMaskIntoConstraints = false;
        
        _textField.TopAnchor.ConstraintEqualTo(TopAnchor).Active = true;
        _textField.BottomAnchor.ConstraintEqualTo(BottomAnchor).Active = true;
        _textField.LeadingAnchor.ConstraintEqualTo(LeadingAnchor).Active = true;
        
        _stepper.TopAnchor.ConstraintEqualTo(TopAnchor).Active = true;
        _stepper.BottomAnchor.ConstraintEqualTo(BottomAnchor).Active = true;
        _stepper.LeadingAnchor.ConstraintEqualTo(_textField.TrailingAnchor).Active = true;
        _stepper.TrailingAnchor.ConstraintEqualTo(TrailingAnchor).Active = true;
    }
    
    public void SetValue(int value) {
        _textField.StringValue = value.ToString();
        _stepper.IntValue = value;
    }
    
    public int GetValue() {
        return _stepper.IntValue;
    }
}

public class NumberFormatter : NSNumberFormatter {
    private NSString _lastValidString = new NSString("");
    
    public NumberFormatter(long minValue, long maxValue) {
        FormatterBehavior = NSNumberFormatterBehavior.Version_10_4;
        NumberStyle = NSNumberFormatterStyle.None;
        Minimum = NSNumber.FromInt64(minValue);
        Maximum = NSNumber.FromInt64(maxValue);
        PartialStringValidationEnabled = false;
    }
    
    [Export ("isPartialStringValid:newEditingString:errorDescription:")]
    public bool IsPartialStringValidNewEditingStringErrorDescription(NSString partialString, ref NSString newString, ref NSString error) {
        if (partialString.Length == 0) {
            return true;
        }
    
        var nativeString = partialString.ToString().Replace(" ", "");
        // Remove NBSP that the formatter adds automatically
        nativeString = nativeString.Replace("\u00A0", "");
    
        if (!int.TryParse(nativeString, out int value)) {
            return false;
        }
        
        if (value < Minimum!.Int64Value || value > Maximum!.Int64Value) {
            newString = _lastValidString;
            
            return false;
        }
        
        _lastValidString = newString;
    
        return true;
    }
}