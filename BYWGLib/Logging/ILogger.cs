using System;

namespace BYWGLib.Logging
{
    /// <summary>
    /// 高性能日志接口
    /// </summary>
    public interface ILogger
    {
        void Log(LogLevel level, string message, params object[] args);
        void Log(LogLevel level, Exception exception, string message, params object[] args);
        
        void Debug(string message, params object[] args);
        void Info(string message, params object[] args);
        void Warning(string message, params object[] args);
        void Error(string message, params object[] args);
        void Error(Exception exception, string message, params object[] args);
        void Fatal(string message, params object[] args);
        void Fatal(Exception exception, string message, params object[] args);
    }
    
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        Fatal = 4
    }
}
