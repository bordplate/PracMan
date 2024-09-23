namespace TrainManCore.Target;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

internal struct MemorySubItem {
    public uint Address;
    public uint Size;
    public Target.MemoryCondition Condition;
    public bool Freeze;
    public bool Released;
    public byte[] LastValue;
    public byte[] SetValue;
    public Action<byte[]> Callback;
}

public class RPCS3(string slot) : Target(slot) {
    public new static string Name() {
        return "RPCS3";
    }
    
    public new static string PlaceholderAddress() {
        return "28012";
    }
    
    public new static void DiscoverTargets(DicoveredTargetsCallback callback) {
        List<string> targets = [];
        
        // If not Windows, look for .sock files in /tmp
        if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
            string tmpDir = Environment.GetEnvironmentVariable("TMPDIR") ?? "/tmp";
            string[] files = Directory.GetFiles(tmpDir, "rpcs3.sock.*");
            foreach (string file in files) {
                string[] parts = file.Split('.');
                if (parts.Length == 3) {
                    targets.Add(parts[2]);
                }
            }
        }
        
        callback(targets);
    }

    private PINE pine;
    private List<MemorySubItem> SubItems = [];
    private Mutex SubMutex = new(false);
    private bool MemoryWorkerStarted = false;
    
    public override bool Start(AttachedCallback callback) {
        // _address must be an integer
        if (!int.TryParse(_address, out int slot)) {
            callback(false, "Invalid slot");
            return false;
        }
        
        try {
            pine = new PINE(slot);
            callback(true, null);
            return true;
        }
        catch (Exception ex) {
            callback(false, "Connection failed: " + ex.Message);
            return false;
        }
    }

    public override bool Stop() {
        base.Stop();

        MemoryWorkerStarted = false;

        try {
            pine.Close();
            return true;
        }
        catch (Exception ex) {
            Console.WriteLine($"Disconnection failed: {ex.Message}");
            return false;
        }
    }

    public override int GetCurrentPID() {
        try {
            var status = pine.Status();
            return status == EmulatorStatus.SHUTDOWN ? 0 : 1;
        }
        catch {
            return 0;
        }
    }

    public override string GetGameTitleID() {
        try {
            string gameId = pine.GameId();
            return string.IsNullOrEmpty(gameId) ? "" : gameId;
        }
        catch {
            return "";
        }
    }

    public override int MemSubIDForAddress(uint address) {
        throw new NotImplementedException();
    }

    public override void Notify(string message) {
        //MessageBox.Show(message);
    }

    public override byte[] ReadMemory(uint address, uint size) {
        try {
            uint adjustedAddress = address;
            return pine.Read(adjustedAddress, (int)size);
        }
        catch (Exception ex) {
            Console.WriteLine($"ReadMemory failed: {ex.Message}");
            return new byte[size];
        }
    }

    public override void WriteMemory(uint address, uint size, byte[] memory) {
        try {
            uint adjustedAddress = address;
            pine.Write(adjustedAddress, memory);
        }
        catch (Exception ex) {
            Console.WriteLine($"WriteMemory failed: {ex.Message}");
        }
    }

    public override void ReleaseSubID(int memSubID) {
        if (memSubID < 0 || memSubID >= SubItems.Count)
            return;

        var subItem = SubItems[memSubID];
        subItem.Released = true;

        SubMutex.WaitOne();
        SubItems[memSubID] = subItem;
        SubMutex.ReleaseMutex();
    }

    public override int SubMemory(uint address, uint size, MemoryCondition condition, byte[] memory,
        Action<byte[]> callback) {
        var item = new MemorySubItem {
            Address = address,
            Size = size,
            Condition = condition,
            Callback = callback,
            SetValue = memory,
            Freeze = false
        };

        SubMutex.WaitOne();
        SubItems.Add(item);
        SubMutex.ReleaseMutex();

        if (!MemoryWorkerStarted) {
            StartMemorySubWorker();
        }

        return SubItems.Count - 1;
    }

    public override int FreezeMemory(uint address, uint size, MemoryCondition condition, byte[] memory) {
        var item = new MemorySubItem {
            Address = address,
            Size = size,
            Condition = condition,
            SetValue = memory,
            Freeze = true
        };

        SubMutex.WaitOne();
        SubItems.Add(item);
        SubMutex.ReleaseMutex();

        if (!MemoryWorkerStarted) {
            StartMemorySubWorker();
        }

        return SubItems.Count - 1;
    }

    private void MemorySubWorker() {
        MemoryWorkerStarted = true;

        while (MemoryWorkerStarted) {
            SubMutex.WaitOne();

            for (int i = 0; i < SubItems.Count; i++) {
                var item = SubItems[i];

                if (item.Released)
                    continue;

                bool hitConditional = false;
                byte[] currentValue = ReadMemory(item.Address, item.Size);

                if (item.Condition == MemoryCondition.Any) {
                    hitConditional = true;
                }
                else if (item.Condition == MemoryCondition.Changed) {
                    if (item.LastValue != null && !currentValue.SequenceEqual(item.LastValue)) {
                        hitConditional = true;
                    }
                }

                if (hitConditional) {
                    if (item.Freeze) {
                        WriteMemory(item.Address, item.Size, item.SetValue);
                    }

                    RunOnMainThread(() => {
                        item.Callback?.Invoke(currentValue.Reverse().ToArray());
                    });
                }

                item.LastValue = currentValue;
                SubItems[i] = item;
            }

            SubMutex.ReleaseMutex();
            Thread.Sleep(1000 / 120); // Adjust as needed
        }
    }

    private void StartMemorySubWorker() {
        Thread thread = new Thread(MemorySubWorker);
        thread.Start();
    }

    private void StopMemorySubWorker() {
        MemoryWorkerStarted = false;
    }
}