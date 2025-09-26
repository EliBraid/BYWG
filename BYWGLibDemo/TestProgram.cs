using System;
using System.Threading.Tasks;
using BYWGLib;
using BYWGLib.Logging;

namespace BYWGLibDemo
{
    class TestProgram
    {
        public static async Task RunAsync(string[] args)
        {
            try
            {
                Log.Information("BYWGLibDemo 启动");
                Log.Information(VersionInfo.FullVersionInfo);

                // 运行S7协议测试
                await global::BYWGLibDemo.SimpleTest.RunTestAsync();

                Log.Information("BYWGLibDemo 已停止");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "程序执行出错");
            }
        }
    }
}
