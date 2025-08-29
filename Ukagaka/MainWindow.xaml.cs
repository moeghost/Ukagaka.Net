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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Utils;
using Ukagaka.Classes.ToolKit;
//using System.Text.Encoding.CodePages;


namespace Ukagaka
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {


       static MainWindow shared;


        private static string MenuName_Exit = "EXIT";


        //需添加System.Windows.Forms引用
        private NotifyIcon notifyIcon = null;

        private System.Windows.Forms.ContextMenuStrip notifyContextMenuStrip;
        
        private System.Windows.Forms.ToolStripMenuItem toolStripSubMenu;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator;  //定义菜单分隔符
        private ToolStripMenuItem shellSubMenu;


        public static MainWindow SharedMainWindow()
        {
            return shared;
        }






        public MainWindow()
        {
            shared = this;
            InitializeComponent();
            Loaded += Window_Loaded;
           
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // Init();
        }






        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void Init()
        {

            notifyIcon = new NotifyIcon();
            notifyContextMenuStrip = new ContextMenuStrip();
            InitTray();
            SetupTrayMenu();


            SCFoundation.SharedFoundation().Bootcode();

            //  this.Close();
            //  kero.Show();
            //this.ShowDialog(sakura);
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //SaveConfig();

            notifyIcon.Dispose();
        }


        protected override void OnClosed(EventArgs e)
        {
            SCFoundation.SharedFoundation().PerformQuit();

            //  CloseDevice();
            System.Windows.Application.Current.Shutdown();
            base.OnClosed(e);
            Environment.Exit(0);
        }


        private void InitWithTrayIcon()
        {

            this.notifyContextMenuStrip.Items.Clear();

            //将快捷的文件添加到托盘右键菜单中
          //  this.FlushTrayChildMenuInLove();

            //添加分隔符
            toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.notifyContextMenuStrip.Items.Add(toolStripSeparator);
            /*
            List<IconFileInfo> groups = GetBoxGroup();
            if (groups != null && groups.Count > 0)
            {
                foreach (IconFileInfo group in groups)
                {
                    //创建菜单
                    toolStripSubMenu = new System.Windows.Forms.ToolStripMenuItem();
                    toolStripSubMenu.Text = group.GroupName;
                    toolStripSubMenu.Image = ShortStartup.Properties.Resources.Group;

                    //添加到托盘菜单
                    this.notifyContextMenuStrip.Items.Add(toolStripSubMenu);

                    //刷新该分组下的图标
                    this.FlushTrayChildMenuInGroup(toolStripSubMenu, group.GroupName);
                }
                groups = null;
            }
            */

            shellSubMenu = new ToolStripMenuItem();


            toolStripSubMenu = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSubMenu.Text = "外壳";
            toolStripSubMenu.ToolTipText = "外壳";
            toolStripSubMenu.Tag = "外壳";
            //     toolStripMenuItem.Image = this.smallIconImageList[i];
            toolStripSubMenu.DropDownItems.Add(shellSubMenu);
            toolStripSubMenu.Click += new EventHandler(OnShellClick);





            this.notifyContextMenuStrip.Items.Add(toolStripSubMenu);

             
            //添加分隔符
            toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.notifyContextMenuStrip.Items.Add(toolStripSeparator);







            //添加“退出”
            toolStripSubMenu = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSubMenu.Text = "退出(&E)";
            toolStripSubMenu.Name = MenuName_Exit;
          //  toolStripSubMenu.Image = ShortStartup.Properties.Resources.Exit;
            toolStripSubMenu.Click += new EventHandler(toolStripSubMenu_Click);
            this.notifyContextMenuStrip.Items.Add(toolStripSubMenu);

            toolStripSubMenu = null;





        }


    

        public void SetMenuItemForShellSubMenu(System.Windows.Controls.ContextMenu menu)
        {
            var menuItem = MenuHelper.ConvertWpfMenuToWinFormsToolStripMenu(menu);

            shellSubMenu.DropDownItems.Add(menuItem);

        }


        private void toolStripSubMenu_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStripMenuItem toolStripMenuItem = sender as System.Windows.Forms.ToolStripMenuItem;
            if (toolStripMenuItem.Name == MenuName_Exit)
            {
                //取消注册热键
                // HotKey.UnregisterHotKey(this.Handle, _winQKey);
                // HotKey.GlobalDeleteAtom(_winQKey);

                //退出

                SCFoundation.SharedFoundation().PerformQuit();



                this.Close();

                //(Exit)

                System.Environment.Exit(System.Environment.ExitCode);

            }
            else
            {
                //打开文件
                
               // selectBoxFileItem = null;
            }
            toolStripMenuItem = null;
        }


        // <summary>
        // 将选项添加到托盘右键菜单中
        // </summary>
        private void SetupTrayMenu()
        {




        }


        private bool InitTray()
        {

            //   notifyIcon.Icon = new System.Drawing.Icon("ShortStartUp.ico");

            notifyIcon.Icon = Ukagaka.Properties.Resources.kikka;
            notifyIcon.Text = "Ukagaka";
            notifyIcon.BalloonTipText = "Ukagaka";
            notifyIcon.ShowBalloonTip(1000);
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = this.notifyContextMenuStrip;

            //   notifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler((s, e) => Visibility = (Visibility == Visibility.Hidden) ? Visibility.Visible : Visibility.Hidden);

            ////点击托盘 展示窗体
            this.notifyIcon.Click += (o, e1) =>
            {
                this.Show(); //this.Visible=true;
                this.WindowState = WindowState.Normal;
                this.Activate();
            };

            InitWithTrayIcon();

            return true;
        }


        private void OnShellClick(object sender, EventArgs e)
        {
             
        }


        /*
        private void Init()
        {

            Shell sakura = new Shell(0);
            sakura.Owner = Window.GetWindow(this);
            sakura.Show();
            //sakura.Show();


            Shell kero = new Shell(1);
            kero.Owner = Window.GetWindow(this);
            kero.Show();


            //  this.Close();
            //  kero.Show();
            //this.ShowDialog(sakura);
        }
        */



    }
}
