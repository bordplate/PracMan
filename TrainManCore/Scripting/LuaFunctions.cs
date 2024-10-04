namespace TrainManCore.Scripting;

public static class LuaFunctions {
    public static readonly Dictionary<string, object> Functions = new() {
        ["bytestoint"] = ByteArrayToInt,
        ["bytestouint"] = ByteArrayToUInt,
        ["bytestofloat"] = ByteArrayToFloat
    };
    
    static int ByteArrayToInt(byte[] bytes, int startIndex = 0) {
        return BitConverter.ToInt32(bytes, startIndex);
    }
    
    static uint ByteArrayToUInt(byte[] bytes, int startIndex = 0) {
        return BitConverter.ToUInt32(bytes, startIndex);
    }
    
    static float ByteArrayToFloat(byte[] bytes, int startIndex = 0) {
        return BitConverter.ToSingle(bytes, startIndex);
    }
}