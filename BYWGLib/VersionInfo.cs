namespace BYWGLib
{
    /// <summary>
    /// BYWGLib版本信息类
    /// 提供库的版本信息和相关元数据
    /// </summary>
    public static class VersionInfo
    {
        /// <summary>
        /// 主版本号
        /// </summary>
        public const int Major = 1;
        
        /// <summary>
        /// 次版本号
        /// </summary>
        public const int Minor = 0;
        
        /// <summary>
        /// 修订号
        /// </summary>
        public const int Patch = 0;
        
        /// <summary>
        /// 构建号
        /// </summary>
        public const int Build = 0;
        
        /// <summary>
        /// 完整版本号字符串
        /// </summary>
        public static readonly string Version = $"{Major}.{Minor}.{Patch}.{Build}";
        
        /// <summary>
        /// 产品名称
        /// </summary>
        public const string ProductName = "BYWGLib";
        
        /// <summary>
        /// 产品描述
        /// </summary>
        public const string ProductDescription = "高性能工业协议通信库";
        
        /// <summary>
        /// 版权信息
        /// </summary>
        public const string Copyright = "Copyright © 2024";
        
        /// <summary>
        /// 支持的框架版本
        /// </summary>
        public const string TargetFramework = ".NET 8.0";
        
        /// <summary>
        /// 获取简短版本号
        /// </summary>
        public static string ShortVersion => $"{Major}.{Minor}.{Patch}";
        
        /// <summary>
        /// 获取完整的版本信息字符串
        /// </summary>
        public static string FullVersionInfo => $"{ProductName} v{Version} ({ProductDescription})\n{Copyright}\nTarget: {TargetFramework}";
    }
}