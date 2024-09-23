using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TrainManCore.Target;

public class Ratchetron(string ip) : Target(ip) {
    string Ip { get; set; } = ip;

    private readonly int _port = 9671;

    private int _pid = 0;

    private TcpClient? _client;
    private UdpClient? _udpClient;
    private NetworkStream? _stream;
    private bool _connected = false;

    private IPEndPoint? _remoteEndpoint;

    private readonly Dictionary<int, Action<byte[]>> _memSubCallbacks = new();
    private readonly Dictionary<int, uint> _memSubTickUpdates = new();
    private readonly Dictionary<int, UInt32> _frozenAddresses = new();

    public new static string Name() {
        return "PS3";
    }
    
    public new static string PlaceholderAddress() {
        return "192.168.1.100";
    }

    public override bool Start(AttachedCallback callback) {
        try {
            this._client = new TcpClient(this.Ip, this._port);
            this._client.NoDelay = true;

            this._stream = _client.GetStream();

            byte[] connMsg = new byte[6];
            _ = _stream.Read(connMsg, 0, 6);

            uint apiRev = BitConverter.ToUInt32(connMsg.Skip(2).Take(4).Reverse().ToArray(), 0);

            if (apiRev < 2) {
                callback(false, "The Ratchetron module loaded on your PS3 is too old, you need to restart your PS3 to load the new version.");
                
                return false;
            }

            if (connMsg[0] == 0x01) {
                this._remoteEndpoint = new IPEndPoint(IPAddress.Parse(this.Ip), 0);

                this._connected = true;

#if DEBUG
                this.EnableDebugMessages();
#endif

                _pid = GetCurrentPID();
                
                callback(true, null);
                
                return true;
            }
        }
        catch (SocketException) {
            callback(false, "Socket error: Failed to connect to PS3");
            return false;
        } catch (Exception) {
            callback(false, "Unknown error: Failed to connect to PS3");
            return false;
        }
        
        callback(false, "Mega unknown error: Failed to connect to PS3");

        return false;
    }

    public override bool Stop() {
        base.Stop();
        
        this._connected = false;
        this._udpClient?.Close();
        this._client?.Close();

        return true;
    }

    public override string GetGameTitleID() {
        if (!_connected) {
            throw new Exception("I ain't connected");
        }

        byte[] cmd = { 0x06 };

        WriteStream(cmd, 0, 1);

        byte[] titleIdBuf = new byte[16];
        _ = _stream.Read(titleIdBuf, 0, 16);

        return System.Text.Encoding.Default.GetString(titleIdBuf).Replace("\0", string.Empty);
    }

    public int[] GetPIDList() {
        if (!_connected) {
            throw new Exception("I ain't connected");
        }
        
        Debug.Assert(_stream != null, nameof(_stream) + " != null");

        byte[] cmd = { 0x03 };

        WriteStream(cmd, 0, 1);

        byte[] pidListBuf = new byte[64];

        int nBytes = 0;
        while (nBytes < 64) {
            nBytes += _stream.Read(pidListBuf, 0, 64);
        }

        int[] pids = new int[16];

        for (int i = 0; i < 64; i += 4) {
            byte[] bytes = pidListBuf.Skip(i).Take(4).ToArray();

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(bytes);
            }

            pids[i / 4] = BitConverter.ToInt32(bytes, 0);
        }

        return pids;
    }

    public void EnableDebugMessages() {
        byte[] cmd = { 0x0d };

        WriteStream(cmd, 0, 1);
    }

    public override int GetCurrentPID() {
        return this.GetPIDList()[2];
    }

    private static Mutex writeLock = new Mutex();

    private void WriteStream(byte[] array, int offset, int count) {
        writeLock.WaitOne();
        Debug.Assert(_stream != null, nameof(_stream) + " != null");

        if (this._stream.CanWrite) {
            this._stream.Write(array, offset, count);
        }

        writeLock.ReleaseMutex();
    }

    public override void WriteMemory(uint address, uint size, byte[] memory) {
        var cmdBuf = new List<byte>();
        cmdBuf.Add(0x05);
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)_pid).Reverse());
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)address).Reverse());
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)size).Reverse());
        cmdBuf.AddRange(memory);


        this.WriteStream(cmdBuf.ToArray(), 0, cmdBuf.Count);
    }

    public override byte[] ReadMemory(uint address, uint size) {
        var cmdBuf = new List<byte>();
        cmdBuf.Add(0x04);
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)_pid).Reverse());
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)address).Reverse());
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)size).Reverse());

#if DEBUG
        var watch = new System.Diagnostics.Stopwatch();

        watch.Start();
#endif

        this.WriteStream(cmdBuf.ToArray(), 0, cmdBuf.Count);

        byte[] memory = new byte[size];

        int nBytes = 0;
        while (nBytes < size) {
            nBytes += _stream.Read(memory, 0, (int)size);
        }

#if DEBUG
        watch.Stop();

        //Console.WriteLine($"Request for {size} bytes memory at {address.ToString("X")} took: {watch.ElapsedMilliseconds} ms");
#endif
        return memory.Take((int)size).ToArray();
    }

    public override bool CanInlineNotify() {
        return true;
    }

    public override void Notify(string message) {
        var cmdBuf = new List<byte>();
        cmdBuf.Add(0x02);
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)message.Length).Reverse());
        cmdBuf.AddRange(Encoding.ASCII.GetBytes(message));

        this.WriteStream(cmdBuf.ToArray(), 0, cmdBuf.Count);
    }

    private void DataChannelReceive() {
        IPEndPoint end = new IPEndPoint(IPAddress.Any, 0);

        while (this._connected) {
            try {
                byte[] cmdBuf = this._udpClient.Receive(ref end);
                byte command = cmdBuf.Take(1).ToArray()[0];

                switch (command) {
                    case 0x06: {
                        UInt32 memSubID = BitConverter.ToUInt32(cmdBuf.Skip(1).Take(4).Reverse().ToArray(), 0);
                        UInt32 size = BitConverter.ToUInt32(cmdBuf.Skip(5).Take(4).Reverse().ToArray(), 0);
                        uint tickUpdated = BitConverter.ToUInt32(cmdBuf.Skip(9).Take(4).Reverse().ToArray(), 0);
                        var value = cmdBuf.Skip(13).Take((int)size).Reverse().ToArray();

                        if (this._memSubTickUpdates.ContainsKey((int)memSubID) &&
                            this._memSubTickUpdates[(int)memSubID] != tickUpdated) {
                            this._memSubTickUpdates[(int)memSubID] = tickUpdated;
                            this._memSubCallbacks[(int)memSubID](value);
                        }

                        break;
                    }
                }
            }
            catch (SocketException) {
                // Who gives a shit
            }
        }
    }

    public void OpenDataChannel() {
        byte[] data = new byte[1024];
        int port = 4000;
        bool udpStarted = false;
        while (!udpStarted) {
            try {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
                this._udpClient = new UdpClient(ipep);
                udpStarted = true;
            }
            catch (SocketException) {
                if (port++ > 5000) {
                    // FIXME: Throw exception instead
                    //MessageBox.Show("Tried to open data connection on all ports between 4000 and 5000, but that failed. Did you deny RaCMAN firewall access?");
                    return;
                }
            }
        }

        var assignedPort = ((IPEndPoint)this._udpClient.Client.LocalEndPoint).Port;

        var cmdBuf = new List<byte>();
        cmdBuf.Add(0x09);
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)assignedPort).Reverse());

        this.WriteStream(cmdBuf.ToArray(), 0, cmdBuf.Count);

        byte[] returnValue = new byte[1];

        int nBytes = 0;
        while (nBytes < 1) {
            nBytes += _stream.Read(returnValue, 0, 1);
        }

        if (returnValue[0] == 128) {
            Console.WriteLine("Waiting for connection on port " + assignedPort);

            //this.udpClient.Send(new byte[] { 0x01 }, 1, remoteEndpoint);

            Thread dataThread = new Thread(this.DataChannelReceive);
            dataThread.Start();
        }
        else if (returnValue[0] == 2) {
            Console.WriteLine("Tried to open data channel, but server says we already have one open.");
            _udpClient.Close();
        }
        else {
            Console.WriteLine("Server error trying to open data channel.");
            _udpClient.Close();
        }
    }

    public override int SubMemory(uint address, uint size, MemoryCondition condition, byte[] memory,
        Action<byte[]> callback) {
        var cmdBuf = new List<byte>();
        cmdBuf.Add(0x0a);
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)_pid).Reverse());
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)address).Reverse());
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)size).Reverse());
        cmdBuf.AddRange(new byte[] { (byte)condition });
        cmdBuf.AddRange(memory);

        this.WriteStream(cmdBuf.ToArray(), 0, cmdBuf.Count);

        byte[] memSubIDBuf = new byte[4];

        int nBytes = 0;
        while (nBytes < 4) {
            nBytes += _stream.Read(memSubIDBuf, 0, 4);
        }

        var memSubID = (int)BitConverter.ToInt32(memSubIDBuf.Take(4).Reverse().ToArray(), 0);

        this._memSubCallbacks[memSubID] = callback;
        this._memSubTickUpdates[memSubID] = 0;

        Console.WriteLine($"Subscribed to address {address.ToString("X")} with subscription ID {memSubID}");

        return memSubID;
    }

    public override int FreezeMemory(uint address, uint size, MemoryCondition condition, byte[] memory) {
        var cmdBuf = new List<byte>();
        cmdBuf.Add(0x0b);
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)_pid).Reverse());
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)address).Reverse());
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)size).Reverse());
        cmdBuf.AddRange(new byte[] { (byte)condition });
        cmdBuf.AddRange(memory);

        this.WriteStream(cmdBuf.ToArray(), 0, cmdBuf.Count);

        byte[] memSubIDBuf = new byte[4];

        int nBytes = 0;
        while (nBytes < 4) {
            nBytes += _stream.Read(memSubIDBuf, 0, 4);
        }

        var memSubID = (int)BitConverter.ToInt32(memSubIDBuf.Take(4).Reverse().ToArray(), 0);

        Console.WriteLine($"Froze address {address.ToString("X")} with subscription ID {memSubID}");

        _frozenAddresses[memSubID] = address;

        return memSubID;
    }

    public override void ReleaseSubID(int memSubID) {
        var cmdBuf = new List<byte>();
        cmdBuf.Add(0x0c);
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)memSubID).Reverse());

        this.WriteStream(cmdBuf.ToArray(), 0, cmdBuf.Count);

        byte[] resultBuf = new byte[1];

        int nBytes = 0;
        while (nBytes < 1) {
            nBytes += _stream.Read(resultBuf, 0, 1);
        }

        this._memSubCallbacks.Remove(memSubID);
        this._memSubTickUpdates.Remove(memSubID);
        this._frozenAddresses.Remove(memSubID);

        Console.WriteLine($"Released memory subscription ID {memSubID}");

        // we're ignoring the results because yolo
    }

    public override int MemSubIDForAddress(uint address) {
        foreach (KeyValuePair<int, uint> entry in _frozenAddresses) {
            if (address == entry.Value) {
                return entry.Key;
            }
        }

        return -1;
    }
}