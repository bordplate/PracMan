using NLua;

namespace TrainManCore.Scripting;

public class Inputs {
    public HashSet<Buttons> Mask = new HashSet<Buttons>();

    public float lx = 0.0f;
    public float ly = 0.0f;
    public float rx = 0.0f;
    public float ry = 0.0f;

    private Lua _state;

    public Inputs(Lua state) {
        _state = state;
        
        _state["NewInputState"] = NewInputState; 
        
        (_state["OnLoad"] as LuaFunction)?.Call();
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

    public InputState GetCurrentInputs() {
        var inputs = (_state["GetInputs"] as LuaFunction)?.Call()[0];

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

    public enum Buttons {
        Cross,
        Circle,
        Triangle,
        Square,
        Up,
        Down,
        Left,
        Right,
        L1,
        L2,
        L3,
        R1,
        R2,
        R3,
        Start,
        Select
    }

    public class InputState {
        public HashSet<Buttons> Mask { get; set; }
        public float lx { get; set; }
        public float ly { get; set; }
        public float rx { get; set; }
        public float ry { get; set; }
    }
}