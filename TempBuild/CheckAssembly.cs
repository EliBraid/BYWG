using System;
using System.Reflection;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // 加载DLL文件
            string dllPath = @"F:\C#\BYWG\BYWG.Contracts\bin\Debug\net8.0-windows\BYWG.Contracts.dll";
            Console.WriteLine($"尝试加载DLL文件: {dllPath}");
            
            Assembly assembly = Assembly.LoadFile(dllPath);
            Console.WriteLine($"DLL文件加载成功: {assembly.FullName}");
            
            // 显示DLL文件中的所有类型
            Console.WriteLine("\nDLL文件中的所有类型:");
            var types = assembly.GetTypes();
            
            if (types.Length == 0)
            {
                Console.WriteLine("DLL文件中没有找到任何类型。");
            }
            else
            {
                foreach (var type in types)
                {
                    Console.WriteLine($"类型: {type.FullName}");
                }
                
                Console.WriteLine($"\n总共找到 {types.Length} 个类型。");
            }
            
            // 特别检查Contracts命名空间
            Console.WriteLine("\n检查Contracts命名空间中的类型:");
            var contractsTypes = types.Where(t => t.Namespace?.Contains("Contracts") ?? false);
            
            if (!contractsTypes.Any())
            {
                Console.WriteLine("没有找到任何包含'Contracts'的命名空间中的类型。");
            }
            else
            {
                foreach (var type in contractsTypes)
                {
                    Console.WriteLine($"Contracts类型: {type.FullName}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发生错误: {ex.Message}");
            Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
        }
        
        Console.WriteLine("\n按任意键继续...");
        Console.ReadKey();
    }
}