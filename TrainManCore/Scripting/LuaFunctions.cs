using NLua;

namespace TrainManCore.Scripting;

public class LuaFunctions {
    private Target.Target _target;

    public readonly Dictionary<string, object> Functions;

    public LuaFunctions(Target.Target target) {
        _target = target;
        var ratchetron = new Legacy_Ratchetron(_target);

        Functions = new() {
            ["BytesToUInt"] = ByteArrayToUInt,
            ["BytesToFloat"] = ByteArrayToFloat,
            ["BytesToInt"] = ByteArrayToInt,
            ["Memset"] = Memset,

            // Legacy functions
            ["bytestoint"] = Legacy_ByteArrayToInt,
            ["inttobytes"] = IntToByteArray,
            ["bytestofloat"] = Legacy_ByteArrayToFloat,
            ["floattobytes"] = FloatToByteArray,
            ["memset"] = Memset,
            ["ba"] = LuaTableToByteArray,
            ["dumpbyes"] = DumpByteArray,
            ["read_large"] = ReadLarge,
            ["get_ba_range"] = GetByteArrayRange,
            ["large_lookup"] = LargeLookup,
            ["subscribe_memory"] = SubscribeMemory,
            ["read_byte"] = ReadOneByte,

            ["Ratchetron"] = ratchetron,
            ["GAME_PID"] = 1,
        };
    }

    static int ByteArrayToInt(byte[] bytes, int startIndex = 0) {
        return BitConverter.ToInt32(bytes, startIndex);
    }

    static uint ByteArrayToUInt(byte[] bytes, int startIndex = 0) {
        return BitConverter.ToUInt32(bytes, startIndex);
    }

    static float ByteArrayToFloat(byte[] bytes, int startIndex = 0) {
        return BitConverter.ToSingle(bytes, startIndex);
    }

    // Legacy functions from racman
    public static uint Legacy_ByteArrayToInt(byte[] bytes) {
        if (bytes.Length == 1) {
            return bytes[0];
        }

        if (bytes.Length == 2) {
            return BitConverter.ToUInt16(bytes.Reverse().ToArray(), 0);
        }

        return BitConverter.ToUInt32(bytes.Reverse().ToArray(), 0);
    }

    public static byte[] IntToByteArray(int num, int size) {
        return BitConverter.GetBytes(num).Take(size).Reverse().ToArray();
    }

    public static float Legacy_ByteArrayToFloat(byte[] bytes) {
        return BitConverter.ToSingle(bytes.Reverse().ToArray(), 0);
    }

    public static byte[] FloatToByteArray(float number) {
        return BitConverter.GetBytes(number).Reverse().ToArray();
    }

    public void Memset(uint addr, byte num, uint size) {
        _target.WriteMemory(addr, Enumerable.Repeat<byte>(num, (int)size).ToArray());
    }

    public static byte[] LuaTableToByteArray(object table) {
        List<byte> bytes = new List<byte>();

        foreach (var value in ((NLua.LuaTable)table).Values) {
            bytes.Add(((byte)((Int64)value)));
        }

        return bytes.ToArray();
    }

    public static void DumpByteArray(byte[] bytes) {
        foreach (byte val in bytes) {
            Console.Write($"{val.ToString("X2")} ");
        }

        Console.WriteLine("");
    }

    public byte[] ReadLarge(uint address, uint size) {
        List<byte> buffer = new List<byte>();

        for (uint i = 0; i <= size; i += 0x8000) {
            buffer.AddRange(_target.ReadMemory(address + i, 0x8000));
        }

        return buffer.ToArray();
    }

    public static uint[] LargeLookup(byte[] bytes, int offset, int objectSize, byte[] lookup) {
        List<uint> result = new List<uint>();

        for (int i = 0; i < bytes.Length; i += objectSize) {
            if (bytes.Skip(i + offset).Take(lookup.Length).ToArray() == lookup) {
                result.Add((uint)i);
            }
        }

        return result.ToArray();
    }

    public static byte[] GetByteArrayRange(byte[] bytes, int start, int count) {
        List<byte> result = new List<byte>();

        for (int i = 0; i < count; i++) {
            result.Add(bytes[start + i]);
        }

        return result.ToArray();
    }

    public int SubscribeMemory(int address, int size, LuaFunction callback) {
        var subID = -1;
        subID = _target.SubMemory((uint)address, (uint)size, (value) => { callback.Call(value.Reverse().ToArray()); });

        return subID;
    }

    public int ReadOneByte(int address) {
        var res = _target.ReadMemory((uint)address, 1);
        return res[0];
    }

    // Old racman Ratchetron API that old mods use used to have a first argument for the PID of the process,
    // but we don't use that anymore. So this is a class to wrap the old API to the new one.
    public class Legacy_Ratchetron(Target.Target _target) {
        public string GetGameTitleID() {
            return _target.TitleId;
        }

        public int GetCurrentPID() {
            return _target.GetCurrentPID();
        }

        public void WriteMemory(int _, uint address, uint size, byte[] memory) {
            _target.WriteMemory(address, size, memory);
        }

        public void WriteFloat(int _, uint address, float floatValue) {
            _target.WriteFloat(address, floatValue);
        }

        public void WriteByte(int _, uint address, byte byteValue) {
            _target.WriteByte(address, byteValue);
        }

        public void WriteMemory(int _, uint address, UInt32 intValue) {
            _target.WriteMemory(address, intValue);
        }

        public void WriteMemory(int _, uint address, uint size, string memory) {
            _target.WriteMemory(address, size, memory);
        }

        public void WriteMemory(int _, uint address, byte[] memory) {
            _target.WriteMemory(address, (uint)memory.Length, memory);
        }

        public void Memset(int _, uint address, uint size, byte value) {
            _target.Memset(address, size, value);
        }

        public byte[] ReadMemory(int _, uint address, uint size) {
            return _target.ReadMemory(address, size);
        }

        public uint ReadUInt(int _, uint address) {
            return _target.ReadUInt(address);
        }

        public int ReadInt(int _, uint address) {
            return _target.ReadInt(address);
        }

        public float ReadFloat(int _, uint address) {
            return _target.ReadFloat(address);
        }

        public bool ReadBool(int _, uint address) {
            return _target.ReadBool(address);
        }

        public byte ReadByte(int _, uint address) {
            return _target.ReadByte(address);
        }

        public string ReadMemoryStr(int _, uint address, uint size) {
            return _target.ReadMemoryStr(address, size);
        }

        public void Notify(string message) {
            _target.Notify(message);
        }
    }
}