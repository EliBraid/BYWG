using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;

namespace BYWG
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // 初始化日志记录器
            InitializeLogger();
            
            // 注册未捕获异常处理程序
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            
            // Logging configured via BYWGLib if needed
        }
        
        private void InitializeLogger()
        {
            try
            {
                // 创建日志目录
                string logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                
                // 配置Serilog日志记录器
                // Logging configured via BYWGLib if needed
            }
            catch (Exception ex)
            {
                // 如果无法初始化日志记录器，输出到控制台
                Console.WriteLine("日志记录器初始化失败: " + ex.Message);
            }
        }
        
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Log.Error(e.Exception, "UI线程未处理异常"); // Removed Serilog
            MessageBox.Show("发生未处理的UI异常: " + e.Exception.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
        
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            // Log.Fatal(ex, "应用程序未处理异常，即将退出"); // Removed Serilog
            if (!e.IsTerminating)
            {
                MessageBox.Show("发生严重异常: " + (ex?.Message ?? "未知错误"), "严重错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            // Log.Error(e.Exception, "任务调度器未观察到的异常"); // Removed Serilog
            e.SetObserved();
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            
            // 关闭日志记录器
            // Log.CloseAndFlush(); // Removed Serilog
        }
    }

}
