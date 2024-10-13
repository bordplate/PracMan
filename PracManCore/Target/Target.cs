using System.Text;
using PracManCore.Scripting;

namespace PracManCore.Target;

public abstract class Target {
    public List<Module> Modules = new();
    
    protected string _address;
    private string? _titleId;
    
    public string TitleId {
        get {
            if (_titleId == null) {
                _titleId = GetGameTitleID();
            }

            return _titleId;
        }
        set => _titleId = value;
    }
    
    public event Action? OnStop;
    
    public delegate void DicoveredTargetsCallback(List<string> targets);
    public delegate void AttachedCallback(bool success, string? message);
    
    public static string Name() {
        throw new NotImplementedException();
    }
    
    public static void DiscoverTargets(DicoveredTargetsCallback callback) {
        callback([]);
    }

    public static string PlaceholderAddress() {
        return "?";
    }
    
    public Target(string address) {
        _address = address;
    }

    public abstract bool Start(AttachedCallback callback);

    public virtual bool Stop() {
        OnStop?.Invoke();
        Application.ActiveTargets.Remove(this);

        return true;
    }

    public abstract string GetGameTitleID();

    public abstract int GetCurrentPID();
    public abstract void WriteMemory(uint address, uint size, byte[] memory);
    
    public void WriteInt(uint address, int intValue) {
        WriteMemory(address, (uint)intValue);
    }
    
    public void WriteUInt(uint address, UInt32 intValue) {
        WriteMemory(address, 4, BitConverter.GetBytes(intValue).Reverse().ToArray());
    }
    
    public virtual void WriteFloat(uint address, float floatValue) {
        WriteMemory(address, 4, BitConverter.GetBytes(floatValue).Reverse().ToArray());
    }
    
    public virtual void WriteShort(uint address, short shortValue) {
        WriteMemory(address, 2, BitConverter.GetBytes(shortValue).Reverse().ToArray());
    }
    
    public virtual void WriteByte(uint address, byte byteValue) {
        WriteMemory(address, 1, [ byteValue ]);
    }

    public virtual void WriteMemory(uint address, UInt32 intValue) {
        WriteMemory(address, 4, BitConverter.GetBytes(intValue).Reverse().ToArray());
    }

    public virtual void WriteMemory(uint address, uint size, string memory) {
        byte[] mem = Enumerable.Range(0, memory.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(memory.Substring(x, 2), 16))
            .ToArray();

        WriteMemory(address, size, mem);
    }

    public void WriteMemory(uint address, byte[] memory) {
        WriteMemory(address, (uint)memory.Length, memory);
    }
    
    public void Memset(uint address, uint size, byte value) {
        WriteMemory(address, size, Enumerable.Repeat(value, (int)size).ToArray());
    }

    public abstract byte[] ReadMemory(uint address, uint size);

    public virtual uint ReadUInt(uint address) {
        return BitConverter.ToUInt32(ReadMemory(address, 4).Reverse().ToArray(), 0);
    }
    
    public virtual int ReadInt(uint address) {
        return BitConverter.ToInt32(ReadMemory(address, 4).Reverse().ToArray(), 0);
    }
    
    public virtual float ReadFloat(uint address) {
        return BitConverter.ToSingle(ReadMemory(address, 4).Reverse().ToArray(), 0);
    }
    
    public virtual bool ReadBool(uint address) {
        return BitConverter.ToBoolean(ReadMemory(address, 1), 0);
    }
    
    public virtual byte ReadByte(uint address) {
        return ReadMemory(address, 1)[0];
    }

    public virtual string ReadMemoryStr(uint address, uint size) {
        byte[] memory = ReadMemory(address, size);

        StringBuilder hex = new StringBuilder(memory.Length * 2);
        foreach (byte b in memory)
            hex.AppendFormat("{0:x2}", b);

        return hex.ToString();
    }

    public virtual bool CanInlineNotify() {
        return false;
    }

    public abstract void Notify(string message);

    /// <summary>
    /// Any blasts the data channel with values all the time
    /// Changed only sends data when the value changes
    /// The other things do other things thanks for reading my Ted talk
    /// </summary>
    public enum MemoryCondition : byte {
        Any = 1,
        Changed = 2,
        Above = 3,
        Below = 4,
        Equal = 5, // equal and not equal are not really useful for freezing
        NotEqual = 6
    }

    public abstract int SubMemory(uint address, uint size, MemoryCondition condition, byte[] memory,
        Action<byte[]> callback);

    // Defaults to changed because why blast yourself with data?
    public int SubMemory(uint address, uint size, Action<byte[]> callback) {
        return SubMemory(address, size, MemoryCondition.Changed, new byte[size], callback);
    }

    public int SubMemory(uint address, uint size, MemoryCondition condition, Action<byte[]> callback) {
        return SubMemory(address, size, condition, new byte[size], callback);
    }

    public abstract int FreezeMemory(uint address, uint size, MemoryCondition condition, byte[] memory);

    public virtual int FreezeMemory(uint address, MemoryCondition condition, UInt32 intValue) {
        return FreezeMemory(address, 4, condition,
            BitConverter.GetBytes(intValue).Reverse().ToArray());
    }
    
    public virtual int FreezeMemory(uint address, byte condition, UInt32 intValue) {
        return FreezeMemory(address, 4, (MemoryCondition)condition,
            BitConverter.GetBytes(intValue).Reverse().ToArray());
    }

    public virtual int FreezeMemory(uint address, UInt32 intValue) {
        return FreezeMemory(address, 4, MemoryCondition.Any,
            BitConverter.GetBytes(intValue).Reverse().ToArray());
    }
    
    public int FreezeByte(uint address, byte condition, byte byteValue) {
        return FreezeMemory(address, 1, (MemoryCondition)condition, [ byteValue ]);
    }

    public abstract void ReleaseSubID(int memSubID);

    public abstract int MemSubIDForAddress(uint address);
}