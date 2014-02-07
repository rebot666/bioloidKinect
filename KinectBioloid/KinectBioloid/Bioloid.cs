using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectBioloid
{
    
    class Bioloid
    {
        private SerialPort2Dynamixel serialPort;
        private Dynamixel dynamixel;

        //Especificas de la Conexion
        private bool flagConexion;

        public Bioloid() {
            flagConexion = true;
            serialPort = new SerialPort2Dynamixel();
            dynamixel = new Dynamixel();
        }

        //Conexion Serial para el Bioloid
        public bool Bioloid_Initialize()
        {
            if (flagConexion)
            {
                
                if (serialPort.open("COM1") == false)
                {
                    dynamixel.sendTossModeCommand(serialPort);
                    //msjConexion.Text = "Robot Conectado!";
                    return true;
                }
                else
                {
                    //msjConexion.Text="Robot Conectado!";
                    return false;
                }
            }
            return false;
        }

        public void arriba()
        {
            dynamixel.setPosition(serialPort, 003, 812);
            dynamixel.setPosition(serialPort, 004, 212);
        }

        public void abajo()
        {
            dynamixel.setPosition(serialPort, 003, 212);
            dynamixel.setPosition(serialPort, 004, 812);
        }

        public void estatico()
        {
            dynamixel.setPosition(serialPort, 003, 512);
            dynamixel.setPosition(serialPort, 004, 512);
        }

        public void inicial()
        {
            //Pies
            dynamixel.setPosition(serialPort, 018, 512);
            dynamixel.setPosition(serialPort, 017, 512);

            //Tobillos
            dynamixel.setPosition(serialPort, 016, 512);
            dynamixel.setPosition(serialPort, 015, 512);

            //Rodillas
            dynamixel.setPosition(serialPort, 014, 512);
            dynamixel.setPosition(serialPort, 013, 512);

            //Muslos
            dynamixel.setPosition(serialPort, 012, 512);
            dynamixel.setPosition(serialPort, 011, 512);

            //Trasero
            dynamixel.setPosition(serialPort, 010, 512);
            dynamixel.setPosition(serialPort, 009, 512);

            //Cadera
            dynamixel.setPosition(serialPort, 008, 512);
            dynamixel.setPosition(serialPort, 007, 512);

            //Antebrazo
            dynamixel.setPosition(serialPort, 006, 512);
            dynamixel.setPosition(serialPort, 005, 512);

            //Brazo
            dynamixel.setPosition(serialPort, 004, 512);
            dynamixel.setPosition(serialPort, 003, 512);

            //Hombros
            dynamixel.setPosition(serialPort, 002, 812);
            dynamixel.setPosition(serialPort, 001, 212);

            //
        }

        public void escribir(int pos, int id) {
            dynamixel.setPosition(serialPort, id, pos);
        }
    }
}
