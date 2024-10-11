using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using FluentFTP;
using PracManCore.Exceptions;
using PracManCore.Scripting;

namespace PracManCore.Target;

public class Ratchetron(string ip) : Target(ip) {
    private readonly string _ip = ip;
    private const int Port = 9671;

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

    private void EnsureRatchetronEnabledOnTarget() {
        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(5);
        
        var response = httpClient.GetAsync($"http://{_ip}/home.ps3mapi").Result;
        var responseString = response.Content.ReadAsStringAsync().Result;

        if (responseString.Contains("ratchetron_server.sprx")) {
            return;
        }

        var ftpClient = new FtpClient(_ip);
        ftpClient.AutoConnect();

        if (!ftpClient.IsConnected) {
            throw new TargetException("Failed to connect to PS3 via FTP. Make sure FTP is enabled on port 21 in WebMAN.");
        }
        
        ftpClient.UploadFile($"Scripts/SPRX/ratchetron_server.sprx", "/dev_hdd0/tmp/ratchetron_server.sprx");
        ftpClient.Disconnect();
        
        var loadResponse = httpClient.GetAsync($"http://{ip}/vshplugin.ps3mapi?prx=%2Fdev_hdd0%2Ftmp%2Fratchetron_server.sprx&load_slot=6").Result;
        _ = loadResponse.Content.ReadAsStringAsync().Result;
    }

    public override bool Start(AttachedCallback callback) {
        var connectionThread = new Thread(() => {
            try {
                try {
                    EnsureRatchetronEnabledOnTarget();
                }
                catch (AggregateException ex) {
                    ex.Handle(x => {
                        if (x is TaskCanceledException) {
                            ;
                            Application.Delegate?.RunOnMainThread(() => {
                                callback(false,
                                    "Timed out trying to connect to PS3. Check that the IP address is correct.");
                            });
                            return true;
                        }

                        if (x is HttpRequestException) {
                            Application.Delegate?.RunOnMainThread(() => {
                                callback(false,
                                    "Failed to connect to WebMAN. Check that the IP address is correct and that WebMAN is running on your PS3.");
                            });
                        }

                        return false;
                    });
                    return;
                }
                catch (TargetException exception) {
                    Application.Delegate?.RunOnMainThread(() => {
                        callback(false, exception.Message);
                    });
                    return;
                }
                
                _client = new TcpClient();

                if (!_client.ConnectAsync(_ip, Port).Wait(5000)) {
                    Application.Delegate?.RunOnMainThread(() => {
                        callback(false, "Timed out connecting to PS3");
                    });

                    return;
                }
                
                _client.NoDelay = true;

                _stream = _client.GetStream();

                byte[] connMsg = new byte[6];
                _ = _stream.Read(connMsg, 0, 6);

                uint apiRev = BitConverter.ToUInt32(connMsg.Skip(2).Take(4).Reverse().ToArray(), 0);

                if (apiRev < 2) {
                    callback(false, "The Ratchetron module loaded on your PS3 is too old, you need to restart your PS3 to load the new version.");
                }

                if (connMsg[0] == 0x01) {
                    _remoteEndpoint = new IPEndPoint(IPAddress.Parse(this._ip), 0);
                    _connected = true;
                    _pid = GetCurrentPID();
                    
                    Application.ActiveTargets.Add(this);

                    OpenDataChannel();
                    
                    Application.Delegate?.RunOnMainThread(() => {
                        callback(true, null);
                    });
                }
            } catch (SocketException) {
                Application.Delegate?.RunOnMainThread(() => {
                    callback(false, "Socket error: Failed to connect to PS3.");
                });
            } catch (Exception e) {
                Application.Delegate?.RunOnMainThread(() => {
                    callback(false, $"Unknown error: Failed to connect to PS3\n\n{e.Message}");
                });
            }
        });

        connectionThread.Start();
        
        return false;
    }

    public override bool Stop() {
        base.Stop();
        
        _connected = false;
        _udpClient?.Close();
        _client?.Close();

        return true;
    }

    public override string GetGameTitleID() {
        if (!_connected) {
            throw new TargetException("Not connected to PS3");
        }

        byte[] cmd = [0x06];

        WriteStream(cmd, 0, 1);

        byte[] titleIdBuf = new byte[16];
        _ = _stream?.Read(titleIdBuf, 0, 16);

        return Encoding.Default.GetString(titleIdBuf).Replace("\0", string.Empty);
    }

    public int[] GetPIDList() {
        if (!_connected) {
            throw new TargetException("Not connected to PS3");
        }
        
        Debug.Assert(_stream != null, nameof(_stream) + " != null");

        byte[] cmd = [0x03];

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
        byte[] cmd = [0x0d];

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
        var cmdBuf = new List<byte> { 0x05 };
        
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)_pid).Reverse());
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)address).Reverse());
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)size).Reverse());
        cmdBuf.AddRange(memory);


        this.WriteStream(cmdBuf.ToArray(), 0, cmdBuf.Count);
    }

    public override byte[] ReadMemory(uint address, uint size) {
        if (_stream == null) {
            throw new TargetException("Not connected to PS3");
        }
        
        var cmdBuf = new List<byte> { 0x04 };
        
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)_pid).Reverse());
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)address).Reverse());
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)size).Reverse());

#if DEBUG
        var watch = new Stopwatch();

        watch.Start();
#endif

        WriteStream(cmdBuf.ToArray(), 0, cmdBuf.Count);

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
        var cmdBuf = new List<byte> { 0x02 };
        
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)message.Length).Reverse());
        cmdBuf.AddRange(Encoding.ASCII.GetBytes(message));

        WriteStream(cmdBuf.ToArray(), 0, cmdBuf.Count);
    }

    private void DataChannelReceive() {
        if (_udpClient == null) {
            return;
        }
        
        IPEndPoint end = new IPEndPoint(IPAddress.Any, 0);

        while (_connected) {
            try { 
                if (_udpClient.Available == 0) {
                    Thread.Sleep(1);
                    continue;
                }
                
                byte[] cmdBuf = _udpClient.Receive(ref end);
                byte command = cmdBuf.Take(1).ToArray()[0];

                switch (command) {
                    case 0x06: {
                        UInt32 memSubID = BitConverter.ToUInt32(cmdBuf.Skip(1).Take(4).Reverse().ToArray(), 0);
                        UInt32 size = BitConverter.ToUInt32(cmdBuf.Skip(5).Take(4).Reverse().ToArray(), 0);
                        uint tickUpdated = BitConverter.ToUInt32(cmdBuf.Skip(9).Take(4).Reverse().ToArray(), 0);
                        var value = cmdBuf.Skip(13).Take((int)size).Reverse().ToArray();

                        if (_memSubTickUpdates.ContainsKey((int)memSubID) &&
                            _memSubTickUpdates[(int)memSubID] != tickUpdated) {
                            _memSubTickUpdates[(int)memSubID] = tickUpdated;
                            Application.Delegate?.RunOnMainThread(() => {
                                _memSubCallbacks[(int)memSubID](value);
                            });
                        }

                        break;
                    }
                }
            }
            catch (SocketException) {
                // TODO: Handle this
            }
        }
    }

    public void OpenDataChannel() {
        var port = 4000;
        var udpStarted = false;
        while (!udpStarted) {
            try {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, port);
                _udpClient = new UdpClient(ipep);
                udpStarted = true;
            }
            catch (SocketException) {
                if (port++ > 5000) {
                    throw new TargetException("Tried to open data connection on all ports between 4000 and 5000, but that failed. Did you deny RaCMAN firewall access?");
                }
            }
        }

        if (_udpClient != null && _udpClient.Client.LocalEndPoint != null) {
            var assignedPort = ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port;

            var cmdBuf = new List<byte> { 0x09 };
            cmdBuf.AddRange(BitConverter.GetBytes((UInt32)assignedPort).Reverse());

            WriteStream(cmdBuf.ToArray(), 0, cmdBuf.Count);

            var returnValue = new byte[1];

            var nBytes = 0;
            while (nBytes < 1) {
                if (_stream != null) nBytes += _stream.Read(returnValue, 0, 1);
            }

            if (returnValue[0] == 128) {
                Console.WriteLine("Waiting for connection on port " + assignedPort);

                //this.udpClient.Send(new byte[] { 0x01 }, 1, remoteEndpoint);

                var dataThread = new Thread(DataChannelReceive);
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
    }

    public override int SubMemory(uint address, uint size, MemoryCondition condition, byte[] memory,
        Action<byte[]> callback) {
        if (_stream == null) {
            throw new TargetException("Not connected to PS3");
        }
        
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

        _memSubCallbacks[memSubID] = callback;
        _memSubTickUpdates[memSubID] = 0;

        Console.WriteLine($"Subscribed to address {address:X} with subscription ID {memSubID}");

        return memSubID;
    }

    public override int FreezeMemory(uint address, uint size, MemoryCondition condition, byte[] memory) {
        if (_stream == null) {
            throw new TargetException("Not connected to PS3");
        }
        
        var cmdBuf = new List<byte> { 0x0b };
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
        if (_stream == null) {
            return;
        }
        
        var cmdBuf = new List<byte> { 0x0c };
        cmdBuf.AddRange(BitConverter.GetBytes((UInt32)memSubID).Reverse());

        WriteStream(cmdBuf.ToArray(), 0, cmdBuf.Count);

        byte[] resultBuf = new byte[1];

        int nBytes = 0;
        while (nBytes < 1) {
            nBytes += _stream.Read(resultBuf, 0, 1);
        }

        _memSubCallbacks.Remove(memSubID);
        _memSubTickUpdates.Remove(memSubID);
        _frozenAddresses.Remove(memSubID);

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