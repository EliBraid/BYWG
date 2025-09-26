using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace BYWGLib.Logging
{
    /// <summary>
    /// 高性能日志实现，使用无锁队列和异步写入
    /// </summary>
    public sealed class HighPerformanceLogger : ILogger, IDisposable
    {
        private readonly ConcurrentQueue<LogEntry> _logQueue;
        private readonly Thread _logThread;
        private readonly AutoResetEvent _logEvent;
        private readonly StringBuilder _stringBuilder;
        private volatile bool _disposed;
        private volatile bool _stopLogging;
        
        private static readonly string[] LogLevelNames = { "DEBUG", "INFO", "WARN", "ERROR", "FATAL" };
        
        public HighPerformanceLogger()
        {
            _logQueue = new ConcurrentQueue<LogEntry>();
            _logEvent = new AutoResetEvent(false);
            _stringBuilder = new StringBuilder(1024);
            
            _logThread = new Thread(LogWorker)
            {
                Name = "BYWGLib-Logger",
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal
            };
            _logThread.Start();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Log(LogLevel level, string message, params object[] args)
        {
            if (_disposed || _stopLogging) return;
            
            var entry = new LogEntry
            {
                Level = level,
                Message = message,
                Args = args,
                Timestamp = DateTime.Now,
                ThreadId = Thread.CurrentThread.ManagedThreadId
            };
            
            _logQueue.Enqueue(entry);
            _logEvent.Set();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Log(LogLevel level, Exception exception, string message, params object[] args)
        {
            if (_disposed || _stopLogging) return;
            
            var entry = new LogEntry
            {
                Level = level,
                Message = message,
                Args = args,
                Exception = exception,
                Timestamp = DateTime.Now,
                ThreadId = Thread.CurrentThread.ManagedThreadId
            };
            
            _logQueue.Enqueue(entry);
            _logEvent.Set();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Debug(string message, params object[] args) => Log(LogLevel.Debug, message, args);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Info(string message, params object[] args) => Log(LogLevel.Info, message, args);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Warning(string message, params object[] args) => Log(LogLevel.Warning, message, args);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(string message, params object[] args) => Log(LogLevel.Error, message, args);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(Exception exception, string message, params object[] args) => Log(LogLevel.Error, exception, message, args);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fatal(string message, params object[] args) => Log(LogLevel.Fatal, message, args);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Fatal(Exception exception, string message, params object[] args) => Log(LogLevel.Fatal, exception, message, args);
        
        private void LogWorker()
        {
            while (!_stopLogging)
            {
                try
                {
                    _logEvent.WaitOne(1000); // 1秒超时
                    
                    while (_logQueue.TryDequeue(out var entry))
                    {
                        WriteLogEntry(entry);
                    }
                }
                catch (Exception ex)
                {
                    // 日志系统本身出错时，使用Debug输出
                    System.Diagnostics.Debug.WriteLine($"Logger error: {ex.Message}");
                }
            }
        }
        
        private void WriteLogEntry(LogEntry entry)
        {
            try
            {
                _stringBuilder.Clear();
                
                // 格式化时间戳
                _stringBuilder.AppendFormat("[{0:HH:mm:ss.fff}] ", entry.Timestamp);
                
                // 添加日志级别
                _stringBuilder.AppendFormat("[{0}] ", LogLevelNames[(int)entry.Level]);
                
                // 添加线程ID
                _stringBuilder.AppendFormat("[T{0}] ", entry.ThreadId);
                
                // 格式化消息
                if (entry.Args != null && entry.Args.Length > 0)
                {
                    _stringBuilder.AppendFormat(entry.Message, entry.Args);
                }
                else
                {
                    _stringBuilder.Append(entry.Message);
                }
                
                // 添加异常信息
                if (entry.Exception != null)
                {
                    _stringBuilder.AppendFormat(" | Exception: {0}", entry.Exception.Message);
                }
                
                // 输出到控制台（生产环境可以改为文件或其他输出）
                Console.WriteLine(_stringBuilder.ToString());
                
                // 在Debug模式下也输出到Debug
                #if DEBUG
                System.Diagnostics.Debug.WriteLine(_stringBuilder.ToString());
                #endif
            }
            catch
            {
                // 忽略格式化错误
            }
        }
        
        public void Dispose()
        {
            if (_disposed) return;
            
            _disposed = true;
            _stopLogging = true;
            
            _logEvent?.Set();
            _logThread?.Join(2000); // 等待2秒
            
            _logEvent?.Dispose();
        }
        
        private struct LogEntry
        {
            public LogLevel Level;
            public string Message;
            public object[] Args;
            public Exception Exception;
            public DateTime Timestamp;
            public int ThreadId;
        }
    }
    
    /// <summary>
    /// 静态日志管理器
    /// </summary>
    public static class Log
    {
        private static readonly Lazy<HighPerformanceLogger> _logger = new Lazy<HighPerformanceLogger>(() => new HighPerformanceLogger());
        
        public static ILogger Instance => _logger.Value;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Debug(string message, params object[] args) => Instance.Debug(message, args);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Information(string message, params object[] args) => Instance.Info(message, args);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(string message, params object[] args) => Instance.Warning(message, args);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(string message, params object[] args) => Instance.Error(message, args);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(Exception exception, string message, params object[] args) => Instance.Error(exception, message, args);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fatal(string message, params object[] args) => Instance.Fatal(message, args);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Fatal(Exception exception, string message, params object[] args) => Instance.Fatal(exception, message, args);
    }
}
