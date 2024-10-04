using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace TrainManCore.Target.API;

public enum Opcode : byte {
    OP_READ8 = 0,
    OP_READ16 = 1,
    OP_READ32 = 2,
    OP_READ64 = 3,
    OP_WRITE8 = 4,
    OP_WRITE16 = 5,
    OP_WRITE32 = 6,
    OP_WRITE64 = 7,
    OP_VERSION = 8,
    OP_SAVESTATE = 9,
    OP_LOADSTATE = 10,
    OP_GAMETITLE = 11,
    OP_GAMEID = 12,
    OP_GAMEUUID = 13,
    OP_GAMEVERSION = 14,
    OP_EMUSTATUS = 15
}

public enum EmulatorStatus {
    RUNNING = 0,
    PAUSED = 1,
    SHUTDOWN = 2
}

public class PINE : IDisposable {
    private Socket client;
    private NetworkStream stream;
    private BinaryReader reader;
    private BinaryWriter writer;

    public PINE(int slot, int timeout = 1000) {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.ReceiveTimeout = timeout;
            client.Connect(IPAddress.Loopback, slot);
        } else {
            client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            client.ReceiveTimeout = timeout;
            string socketPath = GetSocketPath(slot);
            client.Connect(new UnixDomainSocketEndPoint(socketPath));
        }

        stream = new NetworkStream(client);
        writer = new BinaryWriter(stream);
        reader = new BinaryReader(stream);
    }

    private string GetSocketPath(int slot) {
        string targetName = "rpcs3";
        string tmpDir;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            tmpDir = Environment.GetEnvironmentVariable("TMPDIR") ?? "/tmp";
        } else {
            tmpDir = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR") ?? "/tmp";
        }

        if (slot >= 28000 && slot <= 30000 && targetName == "pcsx2") {
            return Path.Combine(tmpDir, $"{targetName}.sock");
        } else {
            return Path.Combine(tmpDir, $"{targetName}.sock.{slot}");
        }
    }

    public void Close() {
        stream.Close();
        client.Close();
    }

    public void Dispose() {
        Close();
    }

    private byte[] Mkcmd(byte opcode, params byte[] args) {
        List<byte> cmdList = new List<byte> { opcode };
        cmdList.AddRange(args);
        int length = cmdList.Count + 4; // length of cmd_list + 4
        using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms)) {
                writer.Write(length);
                writer.Write(cmdList.ToArray());
            }

            return ms.ToArray();
        }
    }

    private void Runcmd(byte[] cmd) {
        writer.Write(cmd);
        writer.Flush();
    }

    private (int, byte) ReadHeader() {
        int length = reader.ReadInt32();
        byte returnCode = reader.ReadByte();
        return (length, returnCode);
    }

    public byte Read8(uint address) {
        byte[] addrBytes = BitConverter.GetBytes(address);
        byte[] cmd = Mkcmd((byte)Opcode.OP_READ8, addrBytes);
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
        byte val = reader.ReadByte();
        return val;
    }

    public ushort Read16(uint address) {
        byte[] addrBytes = BitConverter.GetBytes(address);
        byte[] cmd = Mkcmd((byte)Opcode.OP_READ16, addrBytes);
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
        ushort val = reader.ReadUInt16();
        return val;
    }

    public uint Read32(uint address) {
        byte[] addrBytes = BitConverter.GetBytes(address);
        byte[] cmd = Mkcmd((byte)Opcode.OP_READ32, addrBytes);
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
        uint val = reader.ReadUInt32();
        return val;
    }

    public ulong Read64(uint address) {
        byte[] addrBytes = BitConverter.GetBytes(address);
        byte[] cmd = Mkcmd((byte)Opcode.OP_READ64, addrBytes);
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
        ulong val = reader.ReadUInt64();
        return val;
    }

    public void Write8(uint address, byte value) {
        byte[] addrBytes = BitConverter.GetBytes(address);
        byte[] cmd = Mkcmd((byte)Opcode.OP_WRITE8, addrBytes.Concat(new byte[] { value }).ToArray());
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
    }

    public void Write16(uint address, ushort value) {
        byte[] addrBytes = BitConverter.GetBytes(address);
        byte[] valueBytes = BitConverter.GetBytes(value);
        byte[] cmd = Mkcmd((byte)Opcode.OP_WRITE16, addrBytes.Concat(valueBytes).ToArray());
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
    }

    public void Write32(uint address, uint value) {
        byte[] addrBytes = BitConverter.GetBytes(address);
        byte[] valueBytes = BitConverter.GetBytes(value);
        byte[] cmd = Mkcmd((byte)Opcode.OP_WRITE32, addrBytes.Concat(valueBytes).ToArray());
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
    }

    public void Write64(uint address, ulong value) {
        byte[] addrBytes = BitConverter.GetBytes(address);
        byte[] valueBytes = BitConverter.GetBytes(value);
        byte[] cmd = Mkcmd((byte)Opcode.OP_WRITE64, addrBytes.Concat(valueBytes).ToArray());
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
    }

    public string ServerVersion() {
        byte[] cmd = Mkcmd((byte)Opcode.OP_VERSION);
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
        int strlen = reader.ReadInt32();
        byte[] chars = reader.ReadBytes(strlen);
        return Encoding.UTF8.GetString(chars).TrimEnd('\0');
    }

    public void SaveState(byte state) {
        byte[] cmd = Mkcmd((byte)Opcode.OP_SAVESTATE, state);
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
    }

    public void LoadState(byte state) {
        byte[] cmd = Mkcmd((byte)Opcode.OP_LOADSTATE, state);
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
    }

    public string GameTitle() {
        byte[] cmd = Mkcmd((byte)Opcode.OP_GAMETITLE);
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
        int strlen = reader.ReadInt32();
        byte[] chars = reader.ReadBytes(strlen);
        return Encoding.UTF8.GetString(chars).TrimEnd('\0');
    }

    public string GameId() {
        byte[] cmd = Mkcmd((byte)Opcode.OP_GAMEID);
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
        int strlen = reader.ReadInt32();
        byte[] chars = reader.ReadBytes(strlen);
        return Encoding.UTF8.GetString(chars).TrimEnd('\0');
    }

    public string GameUuid() {
        byte[] cmd = Mkcmd((byte)Opcode.OP_GAMEUUID);
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
        int strlen = reader.ReadInt32();
        byte[] chars = reader.ReadBytes(strlen);
        return Encoding.UTF8.GetString(chars).TrimEnd('\0');
    }

    public string GameVersion() {
        byte[] cmd = Mkcmd((byte)Opcode.OP_GAMEVERSION);
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
        int strlen = reader.ReadInt32();
        byte[] chars = reader.ReadBytes(strlen);
        return Encoding.UTF8.GetString(chars).TrimEnd('\0');
    }

    public EmulatorStatus Status() {
        byte[] cmd = Mkcmd((byte)Opcode.OP_EMUSTATUS);
        Runcmd(cmd);
        var (length, returnCode) = ReadHeader();
        if (returnCode != 0)
            throw new IOException();
        int val = reader.ReadInt32();
        return (EmulatorStatus)val;
    }

    public byte[] Read(uint address, int size) {
        List<byte> cmdList = new List<byte>();
        for (int i = 0; i < size; i++) {
            cmdList.Add((byte)Opcode.OP_READ8);
            cmdList.AddRange(BitConverter.GetBytes(address + (uint)i));
        }

        int length = cmdList.Count + 4;
        byte[] cmdBuf;
        using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms)) {
                writer.Write(length);
                writer.Write(cmdList.ToArray());
            }

            cmdBuf = ms.ToArray();
        }

        Runcmd(cmdBuf);
        ReadHeader();
        return reader.ReadBytes(size);
    }

    public void Write(uint address, byte[] bytesData) {
        List<byte> cmdList = new List<byte>();
        for (int i = 0; i < bytesData.Length; i++) {
            cmdList.Add((byte)Opcode.OP_WRITE8);
            cmdList.AddRange(BitConverter.GetBytes(address + (uint)i));
            cmdList.Add(bytesData[i]);
        }

        int length = cmdList.Count + 4;
        byte[] cmdBuf;
        using (var ms = new MemoryStream()) {
            using (var writer = new BinaryWriter(ms)) {
                writer.Write(length);
                writer.Write(cmdList.ToArray());
            }

            cmdBuf = ms.ToArray();
        }

        Runcmd(cmdBuf);
        ReadHeader();
    }
}