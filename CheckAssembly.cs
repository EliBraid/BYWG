using System;
using System.Reflection;
using System.Linq;

class Program
{
    static void Main()
    {
        try
        {
            Assembly assembly = Assembly.LoadFile(@"f:\C#\BYWG\BYWG.Contracts\bin\Debug\net6.0\BYWG.Contracts.dll");
            var types = assembly.GetTypes()
                .Where(t => t.Name.Contains("Request") || t.Name.Contains("Response") || t.Name.Contains("OpcUa"))
                .OrderBy(t => t.Namespace)
                .ThenBy(t => t.Name);

            Console.WriteLine("BYWG.Contracts.dll 中的相关类型:");
            Console.WriteLine("-----------------------------------");
            foreach (var type in types)
            {
                Console.WriteLine($"{type.Namespace}.{type.Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }
}