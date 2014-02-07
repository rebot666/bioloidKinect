using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace KinectBioloid
{
    /// <summary>
    /// Lógica de interacción para Simulador.xaml
    /// </summary>
    public partial class CamaraNormal : Window
    {

        public CamaraNormal()
        {

            InitializeComponent();
        }

        public void MainWindow_Closed()
        {
            //Environment.Exit(0);
            this.Hide();
        }

        void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            MainWindow_Closed();
        }
    }
}
