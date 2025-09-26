using System;
using System.Runtime.CompilerServices;

namespace BYWGLib.Utils
{
    /// <summary>
    /// 高性能CRC计算工具
    /// </summary>
    public static unsafe class CRCUtils
    {
        // Modbus CRC16查找表
        private static readonly ushort[] Crc16Table = new ushort[256];
        
        static CRCUtils()
        {
            InitializeCrc16Table();
        }
        
        private static void InitializeCrc16Table()
        {
            for (int i = 0; i < 256; i++)
            {
                ushort crc = (ushort)i;
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) == 0x0001)
                    {
                        crc = (ushort)((crc >> 1) ^ 0xA001);
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
                Crc16Table[i] = crc;
            }
        }
        
        /// <summary>
        /// 计算Modbus CRC16校验码（高性能版本）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort CalculateModbusCRC16(byte[] data, int length)
        {
            if (data == null || length <= 0)
                return 0;
            
            fixed (byte* dataPtr = data)
            {
                return CalculateModbusCRC16Unsafe(dataPtr, length);
            }
        }
        
        /// <summary>
        /// 计算Modbus CRC16校验码（unsafe版本，最高性能）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort CalculateModbusCRC16Unsafe(byte* data, int length)
        {
            ushort crc = 0xFFFF;
            
            for (int i = 0; i < length; i++)
            {
                byte index = (byte)(crc ^ data[i]);
                crc = (ushort)((crc >> 8) ^ Crc16Table[index]);
            }
            
            return crc;
        }
        
        /// <summary>
        /// 验证Modbus CRC16校验码
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool VerifyModbusCRC16(byte[] data, int length)
        {
            if (data == null || length < 2)
                return false;
            
            ushort calculatedCrc = CalculateModbusCRC16(data, length - 2);
            ushort receivedCrc = (ushort)((data[length - 1] << 8) | data[length - 2]);
            
            return calculatedCrc == receivedCrc;
        }
        
        /// <summary>
        /// 计算并添加CRC16到数据末尾
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AppendModbusCRC16(byte[] data, int length)
        {
            if (data == null || length < 2 || length > data.Length - 2)
                return;
            
            ushort crc = CalculateModbusCRC16(data, length);
            data[length] = (byte)(crc & 0xFF);
            data[length + 1] = (byte)(crc >> 8);
        }
    }
}
