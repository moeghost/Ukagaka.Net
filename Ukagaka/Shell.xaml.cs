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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using System.IO;
using AppSettings;
using Cocoa.AppKit;

namespace Ukagaka
{

    
    /// <summary>
    /// Ukagaka.xaml 的交互逻辑
    /// </summary>
    public partial class Shell : Window
    {
       

       // private NotifyIcon _icon;
        private ContextMenu _contextMenu;

        private List<ShellWpf> loadedForms = new List<ShellWpf>();
        private Floater _floater;
        double ratio = 1.0;
        float baseHorizLoc;   
        int type = 0; //  

        public Shell()
        {
            InitializeComponent();
            Init();
            //InitializeNotifyIcon();
            // InitializeContextMenu();
        }


        public Shell(int type)
        {
            InitializeComponent();
            Init(type);
            //InitializeNotifyIcon();
            // InitializeContextMenu();
        }





        private void Window_Load(object sender, EventArgs e)
        {
         //   SetBackgroundTransparent();
            InitializeControls();
       //     InitializeMenuTable();
    //        LoadMenu(MenuEnum.MainMenu);
    //        HookManager.KeyDown += new KeyEventHandler(HookManager_KeyDown);

            //Floater

    //        _floater = new Floater();
        //    if (AppSettings.Settings.Instance.Redmine_IsFloaterShown)
       //         _floater.Show();
       //     else
       //         _floater.Hide();
        }

      

        #region key hook

        private bool _shell_ctrlKeyDown = false;



        #endregion

        #region initialize shell 


        private void Init(int type)
        {

            this.type = type;
            InitializeControls();

        }
        private void Init()
        {

            InitializeControls();
            this.ShowInTaskbar = false;

        }


        private void InitializeControls()
        {
            Settings settings = Settings.Instance;
            //set default location
            // image_Sakura.poin = new Point(settings.Shell_SakuraLocationX, settings.Shell_SakuraLocationY);
            // image_Sakura.location = new Point(settings.Shell_KeroLocationX, settings.Shell_KeroLocationY);
            //    dialogPanelSakura.location = new Point(settings.Shell_SakuraDialogPanelLocationX, settings.Shell_SakuraDialogPanelLocationY);
            //   dialogPanelKero.location = new Point(settings.Shell_KeroDialogPanelLocationX, settings.Shell_KeroDialogPanelLocationY);

            //set size

            // this.picKero.Size = new Size(settings.Shell_KeroWidth, settings.Shell_KeroHeight);
            //    this.dialogPanelSakura.Size = new Size(settings.Shell_SakuraDialogPanelWidth, settings.Shell_SakuraDialogPanelHeight);
            //     this.dialogPanelKero.Size = new Size(settings.Shell_KeroDialogPanelWidth, settings.Shell_KeroDialogPanelHeight);

            //set pic source


       //     BitmapImage image = new BitmapImage(new Uri("Resources/Images/surface0000.png", UriKind.Relative));//打开图片
            
             BitmapImage image = new BitmapImage(new Uri("pack://SiteOfOrigin:,,,/ghost/Taromati2/shell/Remilia Scarlet/surface0000.png"));//打开图片
              BitmapImage mask = new BitmapImage(new Uri("pack://SiteOfOrigin:,,,/ghost/Taromati2/shell/Remilia Scarlet/surface0000.pna"));//打开图片
          
            if (type == 1)
            {
                image = new BitmapImage(new Uri("pack://SiteOfOrigin:,,,/ghost/Taromati2/shell/Remilia Scarlet/surface0010.png"));//打开图片
                mask = new BitmapImage(new Uri("pack://SiteOfOrigin:,,,/ghost/Taromati2/shell/Remilia Scarlet/surface0010.pna"));//打开图片

            }



            this.image_Sakura.Source = CGImage.CreateImageMask(image, mask);


            this.image_Sakura.Width = image.Width;


            this.image_Sakura.Height = image.Height;
            this.Width = this.image_Sakura.Width;
            this.Height = this.image_Sakura.Height;
          
            //   this.picKero.Image = Image.FromFile("Resources/Images/surface1101.png");

            //set Dialog Panel
            //   dialogPanelSakura.BackColor = System.Drawing.ColorTranslator.FromHtml(settings.Shell_DialogPanelBackColor);
            //    dialogPanelKero.BackColor = System.Drawing.ColorTranslator.FromHtml(settings.Shell_DialogPanelBackColor);
            //    dialogPanelSakura.FlowDirection = FlowDirection.TopDown;
            //    dialogPanelKero.FlowDirection = FlowDirection.TopDown;
            //     dialogPanelSakura.Padding = new Padding(10, 10, 10, 10);
            //    dialogPanelKero.Padding = new Padding(10, 10, 10, 10);
            //     dialogPanelKero.Hide();
        }

        #endregion


 


















        #region shell event handling


         



        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void picSakura_Click(object sender, EventArgs e)
        {
          //  dialogPanelSakura.Visible = (dialogPanelSakura.Visible == true) ? false : true;
        }

        private void picKero_Click(object sender, EventArgs e)
        {
       //     dialogPanelKero.Visible = (dialogPanelKero.Visible == true) ? false : true;
        }

        private void SetControlLocationToCenter_Click(object sender, EventArgs e)
        {
            SetControlLocationToCenter();
        }

        private void SetControlLocationToCenter()
        {
         //   picSakura.location = new Point(ClientSize.Width / 2, ClientSize.Height / 2);
         //   picKero.location = new Point(ClientSize.Width / 2 - 100, ClientSize.Height / 2 + 200);
         //   dialogPanelSakura.location = new Point(ClientSize.Width / 2 - 250, ClientSize.Height / 2 - 50);
         //   dialogPanelKero.location = new Point(ClientSize.Width / 2 - 300, ClientSize.Height / 2 + 200);
        }

        public void ExitMenu_Click(object sender, EventArgs e)
        {
          //  SakuraSay("下次再见~");
        ///    Settings.Instance.Shell_WriteLocationSettings(Settings.ControlEnum.Sakura, picSakura.Location().X, picSakura.Location().Y);
         //   Settings.Instance.Shell_WriteLocationSettings(Settings.ControlEnum.SakuraDialogPanel, dialogPanelSakura.Location().X, dialogPanelSakura.Location().Y);
         //   Settings.Instance.Shell_WriteLocationSettings(Settings.ControlEnum.Kero, picKero.Location().X, picKero.Location().Y);
         //   Settings.Instance.Shell_WriteLocationSettings(Settings.ControlEnum.KeroDialogPanel, dialogPanelKero.Location().X, dialogPanelKero.Location().Y);
        //    Settings.Instance.Shell_SaveSettings();
        //    Close();
        }

        #endregion


    }
}
