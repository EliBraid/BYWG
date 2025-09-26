using System;
using System.Collections.Generic;

namespace BYWGLib
{
    /// <summary>
    /// 数据接收事件参数
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// 协议名称
        /// </summary>
        public string ProtocolName { get; }
        
        /// <summary>
        /// 接收到的数据项
        /// </summary>
        public List<IndustrialDataItem> DataItems { get; }
        
        public DataReceivedEventArgs(string protocolName, List<IndustrialDataItem> dataItems)
        {
            ProtocolName = protocolName;
            DataItems = dataItems;
        }
    }
}