using TrainManCore.Scripting;

namespace TrainManCore.Target;

public class DummyTarget(string title): Target(title) {
    public new static string Name() {
        return "Dummy";
    }
    
    public new static void DiscoverTargets(DicoveredTargetsCallback callback) {
        callback(["NPEA00385", "NPEA00386", "NPEA00387"]);
    }
    
    public new static string PlaceholderAddress() {
        return "NPEA00001";
    }
    
    public override bool Start(AttachedCallback callback) {
        if (_address != "ERROR") {
            Application.ActiveTargets.Add(this);
            
            callback(true, null);
        } else {
            callback(false, "Dummy error");
        }
        
        return true;
    }

    public override bool Stop() {
        base.Stop();
        
        return true;
    }

    public override string GetGameTitleID() {
        return _address;
    }

    public override int GetCurrentPID() {
        return 1;
    }

    public override void WriteMemory(uint address, uint size, byte[] memory) {
        
    }

    public override byte[] ReadMemory(uint address, uint size) {
        return new byte[size];
    }

    public override void Notify(string message) {
        
    }

    public override int SubMemory(uint address, uint size, MemoryCondition condition, byte[] memory, Action<byte[]> callback) {
        return 0;
    }

    public override int FreezeMemory(uint address, uint size, MemoryCondition condition, byte[] memory) {
        return 0;
    }

    public override void ReleaseSubID(int memSubID) {
        
    }

    public override int MemSubIDForAddress(uint address) {
        return 0;
    }
}