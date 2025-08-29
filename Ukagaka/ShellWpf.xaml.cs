using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ukagaka
{
    /// <summary>
    /// UkagakaWpf.xaml 的交互逻辑
    /// </summary>
    public partial class ShellWpf : Window
    {
        public ShellWpf()
        {

            InitializeComponent();
            Init();

        }






        public void Init()
        {
            ShowInTaskbar = false;
            Topmost = true;
            WindowState = WindowState.Maximized;
        }

        protected void ChangeWindowState()
        {
            if (WindowState == WindowState.Minimized)
            {
                //MyNotifyIcon.BalloonTipTitle = "Minimize Sucessful";
                //MyNotifyIcon.BalloonTipText = "Minimized the app ";
                //MyNotifyIcon.ShowBalloonTip(400);
                this.WindowState = WindowState.Maximized;
                Show();
            }
            else if (WindowState == WindowState.Normal || WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Minimized;
                Hide();
            }
        }




    }
}
