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
        
        if (cross) mask.Add(Buttons.cross);
        if (circle) mask.Add(Buttons.circle);
        if (triangle) mask.Add(Buttons.triangle);
        if (square) mask.Add(Buttons.square);
        if (up) mask.Add(Buttons.up);
        if (down) mask.Add(Buttons.down);
        if (left) mask.Add(Buttons.left);
        if (right) mask.Add(Buttons.right);
        if (l1) mask.Add(Buttons.l1);
        if (l2) mask.Add(Buttons.l2);
        if (l3) mask.Add(Buttons.l3);
        if (r1) mask.Add(Buttons.r1);
        if (r2) mask.Add(Buttons.r2);
        if (r3) mask.Add(Buttons.r3);
        if (start) mask.Add(Buttons.start);
        if (select) mask.Add(Buttons.select);
        
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
        cross,
        circle,
        triangle,
        square,
        up,
        down,
        left,
        right,
        l1,
        l2,
        l3,
        r1,
        r2,
        r3,
        start,
        select
    }

    public class InputState {
        public HashSet<Buttons> Mask { get; set; }
        public float lx { get; set; }
        public float ly { get; set; }
        public float rx { get; set; }
        public float ry { get; set; }
    }
}