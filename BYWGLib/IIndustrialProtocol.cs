using System;
using System.Collections.Generic;

namespace BYWGLib
{
    /// <summary>
    /// 工业协议接口
    /// </summary>
    public interface IIndustrialProtocol : IDisposable
    {
        /// <summary>
        /// 协议名称
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 协议是否运行中
        /// </summary>
        bool IsRunning { get; }
        
        /// <summary>
        /// 协议配置
        /// </summary>
        IndustrialProtocolConfig Config { get; }
        
        /// <summary>
        /// 数据接收事件
        /// </summary>
        event EventHandler<DataReceivedEventArgs> DataReceived;
        
        /// <summary>
        /// 启动协议
        /// </summary>
        void Start();
        
        /// <summary>
        /// 停止协议
        /// </summary>
        void Stop();
        
        /// <summary>
        /// 轮询数据
        /// </summary>
        void PollData();
        
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <param name="dataType">数据类型</param>
        /// <returns>读取的结果</returns>
        object Read(string address, string dataType);
        
        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="address">数据地址</param>
        /// <param name="dataType">数据类型</param>
        /// <param name="value">要写入的值</param>
        /// <returns>写入是否成功</returns>
        bool Write(string address, string dataType, object value);
    }
}