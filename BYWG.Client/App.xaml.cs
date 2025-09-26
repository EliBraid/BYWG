using BYWG.Client.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace BYWG.Client
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private IHost _host;

        private void App_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // 创建并配置主机
                _host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        // 配置核心服务
                        ConfigureServices(services);
                    })
                    .Build();

                // 启动主机
                _host.Start();

                // 从依赖注入容器中获取主窗口并显示
                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("应用程序启动失败: " + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(-1);
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // 配置连接管理
            services.AddSingleton<ServiceConnectionManager>();
            
            // 配置客户端服务代理
            services.AddSingleton<ClientServiceProxy>();

            // 配置窗口
            services.AddTransient<MainWindow>();
            services.AddTransient<ProtocolConfigWindow>();
            services.AddTransient<NodeConfigWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // 关闭主机和释放资源
            _host?.Dispose();
            base.OnExit(e);
        }
    }
}