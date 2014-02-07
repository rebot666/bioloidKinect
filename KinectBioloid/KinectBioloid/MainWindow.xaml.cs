using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;


namespace KinectBioloid
{

    public partial class MainWindow : Window
    {
        //Variables del sistema

        //Especificas del Kinect
            private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
            ColorImageFormat lastImageFormat = ColorImageFormat.Undefined;
            WriteableBitmap outputImage;
            WriteableBitmap outputBitmap;
            KinectSensor kinectS;
            byte[] pixelData;

            //DepthImageFormat lastImageFormat1 = DepthImageFormat.Undefined;
            //short[] pixelData1;
            byte[] depthFrame32;
            short[] pixelDataD;
            int RedIndex = 2;
            int GreenIndex = 1;
            int BlueIndex = 0;
            readonly int[] IntensityShiftByPlayerR = { 1, 2, 0, 2, 0, 0, 2, 0 };
            readonly int[] IntensityShiftByPlayerG = { 1, 2, 2, 0, 2, 0, 0, 1 };
            readonly int[] IntensityShiftByPlayerB = { 1, 0, 2, 2, 0, 2, 0, 2 };

            //Body Skeleton
            Skeleton[] skeletons;

            //Para la deteccion
                //Cabeza
                Joint cabeza;
                //Manos
                int HandLeftX, HandLeftY, HandRightX, HandRightY;
                Joint manoIzq, manoDer;
                //Codos
                int ElbowLeftX, ElbowLeftY, ElbowRightX, ElbowRightY;
                Joint codoIzq, codoDer;
                //Hombros
                Joint hombroIzq, hombroDer, hombroCentro;
                //Piernas

            //Para el cierre
                    bool cuenta_cierre = false;


        //Ventana
        CamaraNormal simulador = new CamaraNormal();
        CamaraObscura camaraObscura = new CamaraObscura();

        //Especificas del Bioloid
            Bioloid robot = new Bioloid();
            bool izq = false;
            bool der = false;
            bool ava = false;
            bool rev = false;
            bool stop = false;

        //Bandera de lectura
            long lectura = 0;
            long limiteLectura = 1;
        
        //

        //Inicializaciones

        public MainWindow()
        {
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;/*
            this.Left = System.Windows.SystemParameters.WorkArea.Width - this.Width;
            this.Top = System.Windows.SystemParameters.WorkArea.Height - this.Height;*/
            InitializeComponent();
            bool res;
            res = robot.Bioloid_Initialize();
            if(res){
                conexionBioloid.Foreground = Brushes.Black;
                conexionBioloid.Content = "Conectado";
            }else{
                conexionBioloid.Content = "No Conectado";
            }
            MainWindow_Loaded();
            kinectLayers();
            //simulador.Show();
            //camaraObscura.Show();
            //camaraNormal1.Click += new RoutedEventHandler(button1_Click);
            robot.inicial();
        }

        /*
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //Click Event seen, so something about it
            
            simulador.Show();
        }
        */
        void MainWindow_Loaded() {
            
            try
            {
                kinectS = KinectSensor.KinectSensors[0];
                kinectS.ColorStream.Enable();
                kinectS.DepthStream.Enable();
                kinectS.SkeletonStream.Enable();
                
                kinectS.Start();
                conexionKinect.Foreground = Brushes.Black;
                conexionKinect.Content = "Conectado";
            }
            catch
            {
                conexionKinect.Content = "No Conectado";
                MessageBox.Show("Kinect No encontrado, Terminando Programa.");
                Environment.Exit(0);
            }
        }

        void MainWindow_Closed()
        {
            kinectS.Stop();
            Environment.Exit(0);
            //simulador.MainWindow_Closed();
        }

        void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            MainWindow_Closed();
        }
        


        public void kinectLayers()
        {
            
            //Videoframe ready Initialize
            kinectS.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            kinectS.ColorFrameReady += ColorImageReady;
            //DepthFrameReady
            kinectS.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
            kinectS.DepthFrameReady += new EventHandler<DepthImageFrameReadyEventArgs>(DepthImageReady);
            //Skeleton
            kinectS.SkeletonStream.Enable();
            kinectS.AllFramesReady += FramesReady;
        }

        //Kinect detección
        private void ColorImageReady(object sender, ColorImageFrameReadyEventArgs e)
        {


            using (ColorImageFrame imageFrame = e.OpenColorImageFrame())
            {
                if (imageFrame != null)
                {
                    // We need to detect if the format has changed.
                    bool haveNewFormat = this.lastImageFormat != imageFrame.Format;

                    if (haveNewFormat)
                    {
                        this.pixelData = new byte[imageFrame.PixelDataLength];
                    }

                    imageFrame.CopyPixelDataTo(this.pixelData);

                    // A WriteableBitmap is a WPF construct that enables resetting the Bits of the image.
                    // This is more efficient than creating a new Bitmap every frame.
                    if (haveNewFormat)
                    {

                        //image1.Visibility = Visibility.Visible;
                        //simulador.image3.Visibility = Visibility.Visible;
                        normalImage.Visibility = Visibility.Visible;
                        this.outputImage = new WriteableBitmap(imageFrame.Width, imageFrame.Height, 96, 96, PixelFormats.Bgr32, null);

                        //Imagen camara
                        //this.image1.Source = this.outputImage;
                        //simulador.image3.Source = this.outputImage;
                        normalImage.Source = this.outputImage;
                    }

                    this.outputImage.WritePixels(new Int32Rect(0, 0, imageFrame.Width, imageFrame.Height), this.pixelData, imageFrame.Width * Bgr32BytesPerPixel, 0);

                    this.lastImageFormat = imageFrame.Format;

                    //UpdateFrameRate();
                }
            }
        }

        private void DepthImageReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame imageFrame = e.OpenDepthImageFrame())
            {
                if (imageFrame != null)
                {
                    // We need to detect if the format has changed.
                    bool haveNewFormat = imageFrame != null;

                    if (haveNewFormat)
                    {
                        pixelDataD = new short[imageFrame.PixelDataLength];
                        imageFrame.CopyPixelDataTo(pixelDataD);
                        this.depthFrame32 = new byte[imageFrame.Width * imageFrame.Height * Bgr32BytesPerPixel];
                    }


                    imageFrame.CopyPixelDataTo(pixelDataD);

                    byte[] convertedDepthBits = this.ConvertDepthFrame(pixelDataD, ((KinectSensor)sender).DepthStream);

                    // A WriteableBitmap is a WPF construct that enables resetting the Bits of the image.
                    // This is more efficient than creating a new Bitmap every frame.
                    if (haveNewFormat)
                    {
                        this.outputBitmap = new WriteableBitmap(
                            imageFrame.Width,
                            imageFrame.Height,
                            96,  // DpiX
                            96,  // DpiY
                            PixelFormats.Bgr32,
                            null);

                        //image2.Source = this.outputBitmap;
                        //camaraObscura.image1.Source = this.outputBitmap;
                        normalDeep.Source = this.outputBitmap;
                        //Repintado e instrucciones subsecuentes
                        //Teclado_Bioloid();
                        reloj();
                    }

                    this.outputBitmap.WritePixels(
                        new Int32Rect(0, 0, imageFrame.Width, imageFrame.Height),
                        convertedDepthBits,
                        imageFrame.Width * Bgr32BytesPerPixel,
                        0);

                    //this.lastImageFormat = imageFrame.Format;

                    //UpdateFrameRate();
                }
            }
        }

        private byte[] ConvertDepthFrame(short[] depthFrame, DepthImageStream depthStream)
        {
            int tooNearDepth = depthStream.TooNearDepth;
            int tooFarDepth = depthStream.TooFarDepth;
            int unknownDepth = depthStream.UnknownDepth;

            for (int i16 = 0, i32 = 0; i16 < depthFrame.Length && i32 < this.depthFrame32.Length; i16++, i32 += 4)
            {
                int player = depthFrame[i16] & DepthImageFrame.PlayerIndexBitmask;
                int realDepth = depthFrame[i16] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                byte intensity = (byte)(~(realDepth >> 4));

                if (player == 0 && realDepth == 0)
                {
                    // white 
                    this.depthFrame32[i32 + RedIndex] = 255;
                    this.depthFrame32[i32 + GreenIndex] = 255;
                    this.depthFrame32[i32 + BlueIndex] = 255;
                    //mensaje.Text = "Blanco";
                }
                else if (player == 0 && realDepth == tooFarDepth)
                {
                    // dark purple
                    this.depthFrame32[i32 + RedIndex] = 66;
                    this.depthFrame32[i32 + GreenIndex] = 0;
                    this.depthFrame32[i32 + BlueIndex] = 66;
                    //mensaje.Text = "Morado";
                }
                else if (player == 0 && realDepth == unknownDepth)
                {
                    // dark brown
                    this.depthFrame32[i32 + RedIndex] = 66;
                    this.depthFrame32[i32 + GreenIndex] = 66;
                    this.depthFrame32[i32 + BlueIndex] = 33;
                    //mensaje.Text = "Cafe";
                }
                else
                {
                    // tint the intensity by dividing by per-player values
                    this.depthFrame32[i32 + RedIndex] = (byte)(intensity >> IntensityShiftByPlayerR[player]);
                    this.depthFrame32[i32 + GreenIndex] = (byte)(intensity >> IntensityShiftByPlayerG[player]);
                    this.depthFrame32[i32 + BlueIndex] = (byte)(intensity >> IntensityShiftByPlayerB[player]);
                }
            }

            return this.depthFrame32;
        }

        private void nui_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            bool receivedData = false;
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if (skeletons == null) //allocate the first time
                    {
                        skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }
                    receivedData = true;
                }
                else
                {
                    // apps processing of skeleton data took too long; it got more than 2 frames behind.
                    // thedata is no longer avabilable.
                }
            }
            if (receivedData)
            {

                // DISPLAYOR PROCESS IMAGE DATA IN skeletons HERE
            }
        }

        //Convertidores
        BitmapSource IntToBitmap(int[] array, int w, int h)
        {
            BitmapSource bmap = BitmapSource.Create(w, h, 96, 96, PixelFormats.Bgr32, null, array, w * 4);
            return bmap;
        }

        BitmapSource DepthToBitmapSource(DepthImageFrame imageFrame)
        {
            short[] pixelData = new short[imageFrame.PixelDataLength];
            imageFrame.CopyPixelDataTo(pixelData);

            BitmapSource bmap = BitmapSource.Create(imageFrame.Width, imageFrame.Height, 96, 96, PixelFormats.Gray16, null, pixelData, imageFrame.Width * imageFrame.BytesPerPixel);
            return bmap;
        }

        Point getJoint(JointType j, Skeleton S)
        {
            SkeletonPoint Sloc = S.Joints[j].Position;
            ColorImagePoint Cloc = kinectS.MapSkeletonPointToColor(Sloc, ColorImageFormat.RgbResolution640x480Fps30);
            return new Point(Cloc.X, Cloc.Y);
        }

        void FramesReady(object sender, AllFramesReadyEventArgs e)
        {
            ColorImageFrame VFrame = e.OpenColorImageFrame();
            if (VFrame == null) return;
            byte[] pixelS = new byte[VFrame.PixelDataLength];
            //BitmapSource bmap = ImageToBitmap(VFrame);
            /*
            Point p1, p2, p3, p4;
            
            SkeletonFrame SFrame = e.OpenSkeletonFrame();
            if (SFrame == null) return;

            Skeleton[] Skeletons = new Skeleton[SFrame.SkeletonArrayLength];
            SFrame.CopySkeletonDataTo(Skeletons);
            Joint p5;
            foreach (Skeleton S in Skeletons)
            {
                if (S.TrackingState == SkeletonTrackingState.Tracked)
                {
                    p1 = getJoint(JointType.HandLeft, S);

                    HandLeftX = (int)p1.X;
                    HandLeftY = (int)p1.Y;
                    cIx.Content = HandLeftX;
                    cIy.Content = HandLeftY;
                    
                    
                    p2 = getJoint(JointType.HandRight, S);

                    HandRightX = (int)p2.X;
                    HandRightY = (int)p2.Y;
                    
                    cDx.Content = HandRightX;
                    cDy.Content = HandRightY;

                    p3 = getJoint(JointType.ElbowLeft, S);

                    ElbowLeftX = (int)p3.X;
                    ElbowLeftY = (int)p3.Y;
                    //cIx.Content = ElbowLeftX;
                    //cIy.Content = ElbowLeftY;

                    p4 = getJoint(JointType.ElbowRight, S);

                    ElbowRightX = (int)p4.X;
                    ElbowRightY = (int)p4.Y;
                    //cDx.Content = ElbowRightX;
                    //cDy.Content = ElbowRightY;
                    
                    //Este codigo de la App se ejecuta solo cuando el Kinect lee alguna posicion del esqueleto

                    colocaRobot();
                    
                }
            }
            */
            
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    if (this.skeletons == null)
                    {
                        // Allocate array of skeletons
                        this.skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    // Copy skeletons from this frame
                    skeletonFrame.CopySkeletonDataTo(this.skeletons);

                    // Find first tracked skeleton, if any
                    Skeleton skeleton = this.skeletons.Where(s => s.TrackingState == SkeletonTrackingState.Tracked).FirstOrDefault();
                    
                    if (skeleton != null)
                    {
                        // Obtain the left knee joint; if tracked, print its position
                        Joint j = skeleton.Joints[JointType.KneeLeft];
                        if(skeleton.TrackingState == SkeletonTrackingState.Tracked){
                            leer_cuerpo(skeleton);
                        }
                        /*
                        if (j.TrackingState == JointTrackingState.Tracked)
                        {
                            cIx.Content = j.Position.X;
                            cIy.Content = j.Position.Y;
                            cDx.Content = j.Position.Z;
                        }
                        */
                        
                    }
                }
            }
        }

        void Teclado_Bioloid() {
            
            if (Keyboard.IsKeyDown(Key.Right))
            {
                //mensaje.Text = "Adelanta";
                //Humanoide = new Thread(new ThreadStart(adelanta));
                //adelanta();
            }
            if (Keyboard.IsKeyDown(Key.Left))
            {
                //mensaje.Text = "Retrocede";
                //Humanoide = new Thread(new ThreadStart(retrocede));
                //retrocede();
            }
        }



        public void colocaRobot() {
            if (HandRightY <= 300 && HandLeftY <= 300 && !rev && HandLeftY >= 100 && HandRightY >= 100 && HandRightX > 400 && HandLeftX < 180)
            {
                robot.estatico();
                //msjRobot.Text = "Estatico!";
            }
            else if (HandRightY <= 100 && HandLeftY <= 100 && !ava && HandRightX <= 500 && HandLeftX >= 200)
            {

                robot.arriba();
                //msjRobot.Text = "Arriba!";
            }
            else if (HandRightY >= 330 && HandLeftY >= 330 && !stop) {
                robot.abajo();
                //msjRobot.Text = "Abajo!";
            }
        }

        public void cerrar() {
            if (HandLeftX >= 150 && HandLeftX <= 170 && HandLeftY >= 320 && HandLeftY <= 360 && HandRightX >= 350 && HandRightX <= 400 && HandRightY >= 380 && HandRightY <= 400)
            {
                cuenta_cierre = true;
                //mensaje.Text = ""+cuenta_cierre;
            }
            else {
                cuenta_cierre = false;
                //mensaje.Text = ""+cuenta_cierre;
            }
        }

        public void reloj() {
            
        }

        public void leer_cuerpo(Skeleton skeleton) {
            manoIzq = skeleton.Joints[JointType.HandLeft];
            manoDer = skeleton.Joints[JointType.HandRight];
            cabeza = skeleton.Joints[JointType.Head];
            hombroIzq = skeleton.Joints[JointType.ShoulderLeft];
            hombroDer = skeleton.Joints[JointType.ShoulderRight];
            hombroCentro = skeleton.Joints[JointType.ShoulderCenter];
            codoIzq = skeleton.Joints[JointType.ElbowLeft];
            codoDer = skeleton.Joints[JointType.ElbowRight];
            /*cIx.Content = manoIzq.Position.X;
            cIy.Content = manoIzq.Position.Y;
            cDx.Content = manoDer.Position.X;
            cDy.Content = manoDer.Position.Y;
             */
            /*
            cIx.Content = hombroIzq.Position.Z;
            cIy.Content = codoIzq.Position.Z;
            cDx.Content = manoIzq.Position.Z;
             */

            //Leyendo Codo
            
                
                labelParte.Content = "Codo";
            
                cIx.Content = codoIzq.Position.X;
                cIy.Content = codoIzq.Position.Y;
                cIz.Content = codoIzq.Position.Z;
                cDx.Content = codoIzq.Position.X;
                cDy.Content = codoIzq.Position.Y;
                cDz.Content = codoIzq.Position.Z;
             
            /*
                cIx.Content = manoIzq.Position.X;
                cIy.Content = manoIzq.Position.Y;
                cIz.Content = manoIzq.Position.Z;
                cDx.Content = manoDer.Position.X;
                cDy.Content = manoDer.Position.Y;
                cDz.Content = manoDer.Position.Z;
             */
            movimiento2();
        }

        public double distancia_entre_puntos(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow((x2 - x1),2) + Math.Pow((y2 - y1),2));
        }

        public double aGrados(double angulo)
        {
            return angulo * (180 / Math.PI);
        }

        public void movimiento() {
            double anguloIzq, anguloDer;
            //Mano izquierda
            anguloIzq = Math.Atan2(manoIzq.Position.X, manoIzq.Position.Y);
            anguloIzq = anguloIzq * (180 / Math.PI);

            double anzibio;
            anzibio = ((anguloIzq + 90) * 300) / (-90);
            anzibio = 512 + (int)anzibio;

            if (anzibio <= 212)
            {
                anzibio = 212;
            }
            else if (anzibio >= 812)
            {
                anzibio = 812;
            }

            //msjConexion.Text = "" + anzibio;
            //Mano derecha
            anguloDer = Math.Atan2(manoDer.Position.X, manoDer.Position.Y);
            anguloDer = anguloDer * (180 / Math.PI);
            double andibio;
            andibio = ((anguloDer - 90) * 300) / (90);
            andibio = 512 - (int)andibio;
            //mensaje.Text = "" + andibio;
            if (andibio <= 212)
            {
                andibio = 212;
            }
            else if (andibio >= 812)
            {
                andibio = 812;
            }
            //robot.escribir((int)anzibio, 004);
            //robot.escribir((int)andibio, 003);
        }

        public void movimiento2() {
            double ang1Izq, ang2Izq, ang3Izq;
            double ang1Der, ang2Der, ang3Der;
            double l1Izq, l2Izq, l1Der, l2Der, B;
            double q1, q2;
            double numerador, denominador;

            //Mano izquierda
            l1Izq = this.distancia_entre_puntos(hombroIzq.Position.X, hombroIzq.Position.Y, codoIzq.Position.X, codoIzq.Position.Y);
            l2Izq = this.distancia_entre_puntos(codoIzq.Position.X, codoIzq.Position.Y, manoIzq.Position.X, manoIzq.Position.Y);
            B = this.distancia_entre_puntos(hombroIzq.Position.X, hombroIzq.Position.Y, manoIzq.Position.X, manoIzq.Position.Y);

            q1 = Math.Atan2(manoIzq.Position.X, manoIzq.Position.Y);
            numerador = Math.Pow(l1Izq, 2) - Math.Pow(l2Izq, 2) + Math.Pow(B, 2);
            denominador = 2 * l1Izq * B;
            q2 = Math.Acos((numerador/denominador));
            ang1Izq = q1 + q2;
            numerador = Math.Pow(l1Izq, 2) + Math.Pow(l2Izq, 2) - Math.Pow(B, 2);
            denominador = 2 * l1Izq * l2Izq;
            ang2Izq = Math.Acos((numerador/denominador));

            ang1Izq = this.aGrados(ang1Izq);
            ang2Izq = this.aGrados(ang2Izq);

            //Desplegar angulos brazo izquierdo
            anguloBrazoI.Content = ang1Izq;
            anguloCodoI.Content = ang2Izq;
            


            double an1zibio;
            /*an1zibio = ((ang1Izq + 90) * 300) / (-90);
            an1zibio = 512 + (int)an1zibio;
            */
            an1zibio = ((ang1Izq*-1)+90) * 3.14;
            // an1zibio += 512;

            //Restringir movimiento Bioloid
            if (an1zibio <= 205)
            {
                an1zibio = 205;
            }
            else if (an1zibio >= 820)
            {
                an1zibio = 820;
            }
            //Angulo del codo
            //Angulo complementario
            //ang2Izq = 360 - ang2Izq;
            double an2zibio;

            //msjRobot.Text = "" + ang2Izq;
            /*
            an2zibio = ((ang2Izq + 80) * 300) / (-80);
            an2zibio = Math.Abs(512 + (int)an2zibio);
            an2zibio = 1000 - an2zibio;
            */
            an2zibio = ((180 - (ang2Izq)) +180 ) * 3.14;
            //msjConexion.Text = "" + an2zibio;
            
            if (an2zibio <= 512)
            {
                an2zibio = 512;
            }
            else if (an2zibio >= 820)
            {
                an2zibio = 820;
            }
            
            //Desplegar angulos bioloid izquierdo
            anguloBBrazoI.Content = an1zibio;
            anguloBCodoI.Content = an2zibio;
            

            //Mano derecha
            l1Der = this.distancia_entre_puntos(hombroDer.Position.X, hombroDer.Position.Y, codoDer.Position.X, codoDer.Position.Y);
            l2Der = this.distancia_entre_puntos(codoDer.Position.X, codoDer.Position.Y, manoDer.Position.X, manoDer.Position.Y);
            B = this.distancia_entre_puntos(hombroDer.Position.X, hombroDer.Position.Y, manoDer.Position.X, manoDer.Position.Y);

            q1 = Math.Atan2(manoDer.Position.X, manoDer.Position.Y);
            numerador = Math.Pow(l1Der, 2) - Math.Pow(l2Der, 2) + Math.Pow(B, 2);
            denominador = 2 * l1Der * B;
            q2 = Math.Acos((numerador / denominador));
            ang1Der = q1 + q2;
            numerador = Math.Pow(l1Der, 2) + Math.Pow(l2Der, 2) - Math.Pow(B, 2);
            denominador = 2 * l1Der * l2Der;
            ang2Der = Math.Acos((numerador / denominador));

            ang1Der = this.aGrados(ang1Der);
            ang2Der = this.aGrados(ang2Der);

            //Desplegar angulos brazo derecho
            anguloBrazoD.Content = ang1Der;
            anguloCodoD.Content = ang2Der;

            double an1dibio;
            /*
            an1dibio = ((ang1Der - 90) * 300) / (90);
            an1dibio = 512 - (int)an1dibio;
            */
            //an1dibio =((ang1Der + 90) * 3.41);
            an1dibio = Math.Abs(((ang1Der - 240) * 3.41));
            //an1zibio = ((ang1Izq * -1) + 90) * 3.14;
            // an1zibio += 512;
            
            if (an1dibio <= 205)
            {
                an1dibio = 205;
            }
            else if (an1dibio >= 820)
            {
                an1dibio = 820;
            }
            
            //Angulo del codo
            //Angulo complementario
            //ang2Izq = 360 - ang2Izq;
            double an2dibio;
            //an2zibio = ((ang2Izq + 80) * 300) / (-80);
            //an2zibio = Math.Abs(512 + (int)an2zibio);
            //an2zibio = 1000 - an2zibio;
            /*
            an2dibio = ((ang2Der + 80) * 300) / (-80);
            //100 grados de error
            an2dibio = (Math.Abs((int)an2dibio))-450;
            */
            an2dibio = ((ang2Der - 30) * 3.41);
            if (an2dibio <= 205)
            {
                an2dibio = 205;
            }
            else if (an2dibio >= 512)
            {
                an2dibio = 512;
            }
            //Desplegar angulos bioloid izquierdo
            anguloBBrazoD.Content = an1dibio;
            anguloBCodoD.Content = an2dibio;
            

            robot.escribir((int)an1dibio, 003);
            robot.escribir((int)an2dibio, 005);
            robot.escribir((int)an1zibio, 004);
            robot.escribir((int)an2zibio, 006);
        }

        
    }
}

