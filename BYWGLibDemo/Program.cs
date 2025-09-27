using System;
using System.Threading.Tasks;
using BYWGLib;
using BYWGLib.Logging;

namespace BYWGLibDemo
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                Log.Information("BYWGLibDemo 启动");
                Log.Information(VersionInfo.FullVersionInfo);

                // 运行精确地址映射测试
                await PreciseAddressTest.RunPreciseAddressTest();

                Log.Information("BYWGLibDemo 已停止");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "程序执行出错: {0}", ex.Message);
            }
        }

    }
}