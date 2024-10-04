namespace TrainManCore.Scripting;

public static class LuaFunctions {
    public static Dictionary<string, object> Functions = new() {
        ["bytestoint"] = ByteArrayToInt,
        ["bytestouint"] = ByteArrayToUInt,
        ["bytestofloat"] = ByteArrayToFloat
    };
    
    public static int ByteArrayToInt(byte[] bytes, int startIndex = 0) {
        return BitConverter.ToInt32(bytes, startIndex);
    }
    
    public static uint ByteArrayToUInt(byte[] bytes, int startIndex = 0) {
        return BitConverter.ToUInt32(bytes, startIndex);
    }
    
    public static float ByteArrayToFloat(byte[] bytes, int startIndex = 0) {
        return BitConverter.ToSingle(bytes, startIndex);
    }
}