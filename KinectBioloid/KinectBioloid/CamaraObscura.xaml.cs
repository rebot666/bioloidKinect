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
    /// Interaction logic for CamaraObscura.xaml
    /// </summary>
    public partial class CamaraObscura : Window
    {
        public CamaraObscura()
        {
            /*this.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
            this.Left = this.Width - System.Windows.SystemParameters.WorkArea.Width;
            this.Top = this.Height - System.Windows.SystemParameters.WorkArea.Height;*/
            InitializeComponent();
        }

        public void MainWindow_Closed()
        {
            //Environment.Exit(0);
        }

        void DataWindow_Closing(object sender, CancelEventArgs e)
        {
            MainWindow_Closed();
        }
    }
}
