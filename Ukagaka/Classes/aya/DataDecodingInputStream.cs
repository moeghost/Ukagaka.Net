using System;
using System.IO;
using System.Net.Http;

namespace aya

{
    public class DataDecodingInputStream :StreamReader
    {
      
        public DataDecodingInputStream(Stream stream) : base(stream)
        {
        }

        public int Read()
        {
            int intData = base.Read();
            if (intData == -1)
            {
                return -1;
            }
            byte byteData = (byte)((intData & 0xff) ^ (byte)0xd2);
            return RollByte(byteData, 3);
        }

        public int Read(char[] buffer, int offset, int count)
        {
            int bytesRead = base.ReadBlock(buffer, offset, count);
            if (bytesRead == -1) return -1;

            for (int i = offset; i < offset + bytesRead; i++)
            {
                buffer[i] = (char)RollByte((byte)(buffer[i] ^ (byte)0xd2), 3);
            }
            return bytesRead;
        }

        public static byte RollByte(byte source, int bitsToRoll)
        {
            byte result = source;
            int absBitsToRoll = Math.Abs(bitsToRoll);
            bool shiftLeft = (bitsToRoll > 0);

            for (int i = 0; i < absBitsToRoll; i++)
            {
                result = (shiftLeft ? RollByteLeft(result) : RollByteRight(result));
            }
            return result;
        }

        public static byte RollByteLeft(byte source)
        {
            byte result = source;
            int msb = result & 0x80;

            result <<= 1;
            if (msb == 1)
                result |= 0x01;
            else
                result &= 0xfe;

            return result;
        }

        public static byte RollByteRight(byte source)
        {
            byte result = source;
            int lsb = result & 0x01;

            result >>= 1;
            if (lsb == 1)
                result |= 0x80;
            else
                result &= 0x7f;

            return result;
        }
    }
}
