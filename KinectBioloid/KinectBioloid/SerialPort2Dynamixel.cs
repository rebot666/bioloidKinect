using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;


namespace KinectBioloid
{
    public class SerialPort2Dynamixel
    {
        const int HeaderSize = 4;
        const int PacketLengthByteInx = 3;
        const int BufferSize = 1024;
        const int MaximuTimesTrying = 250;
        const int waitTime = 5;

        private string portName = "COM3";

        private byte[] buffer = new byte[BufferSize];

        protected void initBuffer()
        {
            for (int i = 0; i < BufferSize; i++)
            {
                buffer[i] = 0;
            }
        }

        private SerialPort serialPort = new SerialPort();

        public bool open(String com)
        {
            bool error = false;
            portName = com;

            serialPort.PortName = com;
            serialPort.BaudRate = 57600;
            serialPort.DataBits = 8;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;

            try
            {
                if (serialPort.IsOpen)
                    Console.WriteLine("Serial port is already open");
                else
                    serialPort.Open();
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                error = true;
            }

            return error;
        }

        public void close()
        {
            if (serialPort.IsOpen)
                serialPort.Close();
            Console.WriteLine("Conecction closed!");
        }

        public byte[] query(byte[] buffer, int pos, int time)
        {
            return query(buffer, pos, true, time);
        }

        public byte[] query(byte[] buffer, int pos, bool waitResult)
        {
            return query(buffer, pos, waitResult, waitTime);
        }

        private void cleanConnection()
        {
            serialPort.DiscardInBuffer();
        }

        public byte[] query(byte[] buffer, int pos, bool waitResult, int waitTime)
        {
            byte[] outBuffer = null;
            try
            {
                serialPort.Write(buffer, 0, pos);
                System.Threading.Thread.Sleep(waitTime);
                outBuffer = rawRead();
            }
            catch (Exception exc)
            {
                Console.Out.WriteLine(exc.Message);
            }

            return outBuffer;
        }

        public void rawWrite(byte[] buffer, int pos)
        {
            try
            {
                serialPort.Write(buffer, 0, pos);
            }
            catch (Exception exc)
            {
                Console.Out.WriteLine(exc.Message);
            }
        }

        public byte[] rawRead()
        {
            byte[] localbuffer = null;
            int n = serialPort.BytesToRead;
            if (n != 0)
            {
                localbuffer = new byte[n];
                try
                {
                    serialPort.Read(localbuffer, 0, n);
                }
                catch (Exception exc)
                {
                    Console.Out.WriteLine(exc.Message);
                }
            }
            return localbuffer;
        }
    }
}
