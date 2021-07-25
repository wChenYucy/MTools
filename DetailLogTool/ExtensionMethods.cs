// ****************************************************
//     文件：ExtensionMethonds.cs
//     作者：积极向上小木木
//     邮箱: positivemumu@126.com
//     日期：2021/07/23 20:14
//     功能：拓展方法类，对object类进行拓展，可直接调用日志方法。
// *****************************************************

public static partial class ExtensionMethods
{
    public static void Log(this object obj, object log) {
        DetailLogTool.DetailLog.Log(log);
    }
    public static void Log(this object obj, string msg, params object[] args) {
        DetailLogTool.DetailLog.Log(string.Format(msg, args));
    }
    public static void ColorLog(this object obj, DetailLogTool.LogColor color, object log) {
        DetailLogTool.DetailLog.ColorLog(color, log);
    }
    public static void ColorLog(this object obj, DetailLogTool.LogColor color, string msg, params object[] args) {
        DetailLogTool.DetailLog.ColorLog(color, string.Format(msg, args));
    }
    public static void Trace(this object obj, object log) {
        DetailLogTool.DetailLog.Trace(log);
    }
    public static void Trace(this object obj, string msg, params object[] args) {
        DetailLogTool.DetailLog.Trace(string.Format(msg, args));
    }
    public static void Warn(this object obj, object log) {
        DetailLogTool.DetailLog.Warn(log);
    }
    public static void Warn(this object obj, string msg, params object[] args) {
        DetailLogTool.DetailLog.Warn(string.Format(msg, args));
    }
    public static void Error(this object obj, string log) {
        DetailLogTool.DetailLog.Error(log);
    }
    public static void Error(this object obj, string msg, params object[] args) {
        DetailLogTool.DetailLog.Error(string.Format(msg, args));
    }
}