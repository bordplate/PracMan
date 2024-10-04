using NLua;
using TrainManCore.Exceptions;
using TrainManCore.Scripting.Exceptions;
using TrainManCore.Scripting.UI;

namespace TrainManCore.Scripting;

public class Inputs {
    public static IButtonListener? ButtonListener { get; set; }
    public Lua State { get; }
    public bool ButtonCombosListening { get; private set; }
    public event Action<InputState> OnInput {
        add {
            _onInput += value;
            StartListening();
        }
        remove {
            _onInput -= value;

            // Stop listening if no more listeners
            if (_onInput == null) {
                StopListening();
            }
        }
    }

    private Action<InputState>? _onInput;
    private Module _module;
    private readonly List<ButtonCombo> _buttonCombos = [];
    private int _lastButtonDebounce = 0;
    private Timer? _timer;
    private SynchronizationContext? _context;
    private bool _listening;

    public Inputs(Lua state, Module module) {
        State = state;
        _module = module;
        
        State["NewInputState"] = NewInputState; 
        
        (State["OnLoad"] as LuaFunction)?.Call();
    }

    public static InputState NewInputState(
            bool cross = false,
            bool circle = false,
            bool triangle = false,
            bool square = false,
            bool up = false,
            bool down = false,
            bool left = false,
            bool right = false,
            bool l1 = false,
            bool l2 = false,
            bool l3 = false,
            bool r1 = false,
            bool r2 = false,
            bool r3 = false,
            bool start = false,
            bool select = false,
            float lx = 0.0f,
            float ly = 0.0f,
            float rx = 0.0f,
            float ry = 0.0f
        ) {
        var mask = new HashSet<Buttons>();
        
        if (cross) mask.Add(Buttons.Cross);
        if (circle) mask.Add(Buttons.Circle);
        if (triangle) mask.Add(Buttons.Triangle);
        if (square) mask.Add(Buttons.Square);
        if (up) mask.Add(Buttons.Up);
        if (down) mask.Add(Buttons.Down);
        if (left) mask.Add(Buttons.Left);
        if (right) mask.Add(Buttons.Right);
        if (l1) mask.Add(Buttons.L1);
        if (l2) mask.Add(Buttons.L2);
        if (l3) mask.Add(Buttons.L3);
        if (r1) mask.Add(Buttons.R1);
        if (r2) mask.Add(Buttons.R2);
        if (r3) mask.Add(Buttons.R3);
        if (start) mask.Add(Buttons.Start);
        if (select) mask.Add(Buttons.Select);
        
        return new InputState {
            Mask = mask,
            lx = lx,
            ly = ly,
            rx = rx,
            ry = ry
        };
    }

    public void StartListening() {
        if (_listening) {
            return;
        }
        
        _listening = true;
        
        _timer = new Timer(sender => {
            _context?.Post(_ => _onInput?.Invoke(GetCurrentInputs()), null);
        }, null, 0, 1000 / 60);
        _context = SynchronizationContext.Current ?? new SynchronizationContext();
    }
    
    public void StopListening() {
        if (!_listening) {
            return;
        }
        
        _listening = false;
        
        _timer?.Dispose();
    }

    public InputState GetCurrentInputs() {
        var inputs = (State["GetInputs"] as LuaFunction)?.Call()[0];

        if (inputs == null) {
            return new InputState {
                Mask = new HashSet<Buttons>(),
                lx = 0.0f,
                ly = 0.0f,
                rx = 0.0f,
                ry = 0.0f
            };
        }

        return (InputState)inputs;
    }

    private void ButtonComboListener(InputState inputs) {
        _lastButtonDebounce -= 1;
        
        if (_lastButtonDebounce > 0) {
            return;
        }
        
        // Check if any button combos are exclusively pressed
        foreach (var combo in _buttonCombos) {
            if (combo.Combo.All(button => inputs.Mask.Contains(button)) &&
                combo.Combo.Count == inputs.Mask.Count) {
                combo.Button.Activate();
                _lastButtonDebounce = 60;
            }
        }
    }
    
    public void EnableButtonCombos() {
        if (ButtonCombosListening) {
            return;
        }
        
        ButtonCombosListening = true;

        OnInput += ButtonComboListener;
    }
    
    public void DisableButtonCombos() {
        if (!ButtonCombosListening) {
            return;
        }
        
        ButtonCombosListening = false;

        OnInput -= ButtonComboListener;
    }
    
    public void BindButtonCombos(IWindow window) {
        if (State["Settings"] is not Settings settings) {
            throw new InputsException("No Settings in the Inputs' state.");
        }

        if (settings.Get("Inputs.combos", new List<Dictionary<string, object>>()) is not { } combos) {
            return;
        }

        foreach (var combo in combos) {
            if (combo["combo"] is not Int64 mask || 
                combo["button"] is not string buttonTitle || 
                combo["window"] is not string windowClassName
            ) {
                throw new InputsException("Invalid button combo in settings.");
            }
            
            if (window.ClassName != windowClassName) {
                continue;
            }
            
            // Don't rebind button if it's already bound
            if (_buttonCombos.Any(c => c.Button.Title == buttonTitle && c.Button.Window.ClassName == windowClassName)) {
                continue;
            }
            
            // Cast the mask to a list of buttons
            var buttons = new HashSet<Buttons>();
            foreach (var buttonValue in Enum.GetValues(typeof(Buttons))) {
                if (((int)buttonValue & mask) != 0) {
                    buttons.Add((Buttons)buttonValue);
                }
            }
            
            if (window.GetButton(buttonTitle) is not { } button) {
                // throw new InputsException($"Button {buttonTitle} not found in window '{windowClassName}'.");
                continue;
            }
            
            _buttonCombos.Add(new ButtonCombo(button, buttons));
        }
    }
    
    public void AddOrUpdateButtonCombo(ButtonCombo combo, bool save = true) {
        var existing = _buttonCombos.FirstOrDefault(c => c.Button == combo.Button);

        if (existing != null) {
            existing.Combo = combo.Combo;
        } else {
            _buttonCombos.Add(combo);
        }

        EnableButtonCombos();
        
        if (save) {
            SaveCombosToSettings();
        }
    }
    
    public void RemoveButtonCombo(ButtonCombo combo) {
        _buttonCombos.Remove(combo);
        
        if (_buttonCombos.Count == 0 && ButtonCombosListening) {
            DisableButtonCombos();
        }
        
        SaveCombosToSettings();
    }

    public void SaveCombosToSettings() {
        if (State["Settings"] is not Settings settings) {
            throw new InputsException("No Settings in the Inputs' state.");
        }

        var combos = new List<Dictionary<string, object>>();

        foreach (var combo in _buttonCombos) {
            // Cast the combo to a mask of buttons
            var mask = combo.Combo.Select(button => (int)button).Aggregate((a, b) => a | b);
            
            combos.Add(new() {
                ["combo"] = mask, 
                ["button"] = combo.Button.Title, 
                ["window"] = combo.Button.Window.ClassName
            });
        }
        
        settings.Set("Inputs.combos", combos);
    }

    public List<ButtonCombo> ButtonCombos() {
        return _buttonCombos;
    }

    public enum Buttons {
        Cross = 0x1,
        Circle = 0x2,
        Triangle = 0x4,
        Square = 0x8,
        Up = 0x10,
        Down = 0x20,
        Left = 0x40,
        Right = 0x80,
        L1 = 0x100,
        L2 = 0x200,
        L3 = 0x400,
        R1 = 0x800,
        R2 = 0x1000,
        R3 = 0x2000,
        Start = 0x4000,
        Select = 0x8000
    }

    public class InputState {
        public HashSet<Buttons> Mask { get; set; } = new();
        public float lx { get; set; }
        public float ly { get; set; }
        public float rx { get; set; }
        public float ry { get; set; }
        
        public string ToString() {
            var buttons = Mask.Select(button => button.ToString()).ToList();

            return string.Join("+", buttons);
        }
    }

    public class ButtonCombo(IButton button, HashSet<Buttons> combo) {
        public IButton Button { get; set; } = button;
        public HashSet<Buttons> Combo { get; set; } = combo;

        public string ToString() {
            var buttons = Combo.Select(button => button.ToString()).ToList();
            
            return string.Join("+", buttons);
        }
        
        public bool Equals(ButtonCombo other) {
            return Button == other.Button && Combo.SetEquals(other.Combo);
        }
    }
}