using System;

namespace BYWG.Utils
{
    /// <summary>
    /// 简化的日志类
    /// </summary>
    public static class Log
    {
        public static void Information(string message, params object[] args)
        {
            Console.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss.fff} {string.Format(message, args)}");
        }

        public static void Warning(string message, params object[] args)
        {
            Console.WriteLine($"[WARN] {DateTime.Now:HH:mm:ss.fff} {string.Format(message, args)}");
        }

        public static void Error(Exception ex, string message, params object[] args)
        {
            Console.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss.fff} {string.Format(message, args)}\n{ex}");
        }

        public static void Debug(string message, params object[] args)
        {
            #if DEBUG
            Console.WriteLine($"[DEBUG] {DateTime.Now:HH:mm:ss.fff} {string.Format(message, args)}");
            #endif
        }
    }
}
