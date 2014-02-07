using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectBioloid
{
    class Dynamixel
    {
        protected static int MaxBufferSize = 1024;
        protected byte[] buffer = new byte[MaxBufferSize];

        private static byte checkSumatory(byte[] data, int length)
        {
            uint cs = 0;
            for (int i = 2; i < length; i++)
            {
                cs += data[i];
            }

            cs = ~cs;

            return (byte)(cs & 0x0FF);
        }

        public static void toHexHLConversion(int pos, out byte hexH, out byte hexL)
        {
            ushort uPos = (ushort)pos;
            hexH = (byte)(uPos >> 8);
            hexL = (byte)uPos;
        }

        public static ushort fromHexHLConversion(byte hexH, byte hexL)
        {
            return (ushort)((hexL << 8) + hexH);
        }

        public static short fromHexHLConversionToShort(byte hexH, byte hexL)
        {
            return (short)((hexL << 8) + hexH);
        }

        public static void toHexHLConversion(int pos, out string hexH, out string hexL)
        {
            string hex;
            int lon, start;
            hex = String.Format("{0:X4}", pos);
            lon = hex.Length;
            if (lon < 2)
            {
                hexL = hex;
                hexH = "0";
            }
            else
            {
                start = lon - 2;// lon = 4, start = 2; lon=3, start=1
                hexL = hex.Substring(start);
                hexH = hex.Substring(0, start);
            }
        }

        public void sendTossModeCommand(SerialPort2Dynamixel sp2d)
        {
            byte[] buffer = { (byte)'t', (byte)'\r' };
            sp2d.rawWrite(buffer, 2);
            System.Threading.Thread.Sleep(100);
            sp2d.rawRead();
        }

        private static int getReadPositionCommand(byte[] buffer, byte id)
        {
            //OXFF 0XFF ID LENGTH INSTRUCTION PARAMETER1 …PARAMETER N CHECK SUM
            int pos = 0;

            buffer[pos++] = 0xff;
            buffer[pos++] = 0xff;
            buffer[pos++] = id;

            // bodyLength = 4
            buffer[pos++] = 4; //placeholder

            //the instruction, rawRead => 2
            buffer[pos++] = 2;

            // pos registers 36 and 37
            buffer[pos++] = 0x24;

            //bytes to rawRead
            buffer[pos++] = 2;

            byte checksum = checkSumatory(buffer, pos);
            buffer[pos++] = checksum;

            return pos;
        }

        private static int getSetPositionCommand(byte[] buffer, byte id, short goal)
        {
            int pos = 0;
            byte numberOfParameters = 0;
            //OXFF 0XFF ID LENGTH INSTRUCTION PARAMETER1 …PARAMETER N CHECK SUM

            buffer[pos++] = 0xff;
            buffer[pos++] = 0xff;
            buffer[pos++] = id;

            // bodyLength
            buffer[pos++] = 0; //place holder

            //the instruction, query => 3
            buffer[pos++] = 3;

            // goal registers 30 and 31
            buffer[pos++] = 0x1E;// 30;

            //bytes to write
            byte hexH = 0;
            byte hexL = 0;
            toHexHLConversion(goal, out hexH, out hexL);

            buffer[pos++] = hexL;
            numberOfParameters++;
            buffer[pos++] = hexH;
            numberOfParameters++;

            // bodyLength
            buffer[3] = (byte)(numberOfParameters + 3);

            byte checksum = checkSumatory(buffer, pos);
            buffer[pos++] = checksum;

            return pos;
        }

        public short getPosition(SerialPort2Dynamixel sp2d, int id)
        {
            //byte[] localbuffer = new byte[MaxBufferSize];
            int size = getReadPositionCommand(buffer, (byte)id);
            byte[] res = sp2d.query(buffer, size, true);

            short position = -1;

            if (res != null)
            {
                int length = res.Length;
                if (res != null && length > 4 && res[4] == 0)
                {
                    byte l = 0;
                    byte h = res[5];
                    if (length > 6)
                    {
                        l = res[6];
                    }

                    position = fromHexHLConversionToShort(h, l);
                }
            }

            return position;
        }

        public bool setPosition(SerialPort2Dynamixel sp2d, int id, int goal)
        {
            bool couldSet = false;

            short position = (short)goal;
            int size = getSetPositionCommand(buffer, (byte)id, (short)goal);
            byte[] res = sp2d.query(buffer, size, false);

            //ushort value = 1;
            if (res != null && res.Length > 4 && res[4] == 0)
                couldSet = true;

            return couldSet;
        }
    }
}
