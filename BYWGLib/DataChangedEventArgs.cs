using System;
using System.Collections.Generic;

namespace BYWGLib
{
    /// <summary>
    /// 数据变化事件参数
    /// </summary>
    public class DataChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 变化的数据项
        /// </summary>
        public List<IndustrialDataItem> ChangedItems { get; }
        
        public DataChangedEventArgs(List<IndustrialDataItem> changedItems)
        {
            ChangedItems = changedItems;
        }
    }
}