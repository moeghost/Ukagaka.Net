using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using Cocoa.AppKit;
namespace Ukagaka
{




    public class SCGhostManagerWindowController : Window
    {
        private SCGhostManager ghostManager;

        // UI 控件
        private TabControl tabViewInfo;
        private ListView tableInstalled;
        private ListView tableShell;
        private ListView tableBalloon;
        private TextBox findBox;
        private Button btnThumbView;
        private Image imageThumbnail;
        SCGhostPreviewView view_preview;
        private TextBox fldScale;
        private Slider sliderScale;
        private Button btnNetworkUpdate;
        private Button btnTateKesi;
        private Button btnVanish;
        private TextBox fldIdentification;
        private TextBox fldMasterShiori;
        private TextBox fldNShells;
        private TextBox fldMakoto;

        public SCGhostManagerWindowController(SCGhostManager manager)
        {
            ghostManager = manager;
            view_preview = new SCGhostPreviewView();
            Title = "GhostManager";
            Width = 800;
            Height = 600;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            InitializeUI();
            LoadDefaults();

            Closing += OnWindowClosing;
        }

        private void InitializeUI()
        {
            // 主容器
            DockPanel rootPanel = new DockPanel();
            Content = rootPanel;

            // 查找框和按钮行
            StackPanel topPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
            findBox = new TextBox { Width = 200, Margin = new Thickness(5) };
            findBox.TextChanged += FindBoxUpdated;
            btnThumbView = new Button { Content = "Thumbnail View", Margin = new Thickness(5) };
            btnThumbView.Click += (s, e) => ToggleThumbnailDrawer();

            topPanel.Children.Add(findBox);
            topPanel.Children.Add(btnThumbView);
            DockPanel.SetDock(topPanel, Dock.Top);
            rootPanel.Children.Add(topPanel);

            // TabControl
            tabViewInfo = new TabControl();
            DockPanel.SetDock(tabViewInfo, Dock.Top);

            // --- Tab: Control/Preview ---
            TabItem tabControlPreview = new TabItem { Header = "CONTROL/PREVIEW" };
            StackPanel controlPanel = new StackPanel { Margin = new Thickness(10) };

            sliderScale = new Slider { Minimum = 0.5, Maximum = 2.0, Value = 1.0, Width = 200, Margin = new Thickness(5) };
            sliderScale.ValueChanged += SliderScaleChanged;
            fldScale = new TextBox { Width = 60, IsReadOnly = true, Margin = new Thickness(5) };
            UpdateScaleIndicator();

            btnNetworkUpdate = new Button { Content = "Network Update", Margin = new Thickness(5) };
            btnNetworkUpdate.Click += NetworkUpdate;
            btnTateKesi = new Button { Content = "Boot/Quit", Margin = new Thickness(5) };
            btnTateKesi.Click += TateKesi;
            btnVanish = new Button { Content = "Vanish", Margin = new Thickness(5) };
            btnVanish.Click += Vanish;

            controlPanel.Children.Add(sliderScale);
            controlPanel.Children.Add(fldScale);
            controlPanel.Children.Add(btnNetworkUpdate);
            controlPanel.Children.Add(btnTateKesi);
            controlPanel.Children.Add(btnVanish);

            tabControlPreview.Content = controlPanel;
            tabViewInfo.Items.Add(tabControlPreview);

            // --- Tab: Shell ---
            TabItem tabShell = new TabItem { Header = "SHELL" };
            tableShell = new ListView { Margin = new Thickness(5) };
            tabShell.Content = tableShell;
            tabViewInfo.Items.Add(tabShell);

            // --- Tab: Balloon ---
            TabItem tabBalloon = new TabItem { Header = "BALLOON" };
            tableBalloon = new ListView { Margin = new Thickness(5) };
            tabBalloon.Content = tableBalloon;
            tabViewInfo.Items.Add(tabBalloon);




            TabItem tabInstalled = new TabItem { Header = "GHOST" };
            tableInstalled = new ListView { Margin = new Thickness(5) };
            tabInstalled.Content = tableInstalled;
            tabViewInfo.Items.Add(tabInstalled);




            rootPanel.Children.Add(tabViewInfo);

            // Thumbnail 显示
            imageThumbnail = new Image
            {
                Width = 120,
                Height = 120,
                Margin = new Thickness(10),
                Stretch = Stretch.Uniform
            };
            DockPanel.SetDock(imageThumbnail, Dock.Right);
            rootPanel.Children.Add(imageThumbnail);

            // 下方详情信息
            StackPanel bottomPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(5) };
            fldIdentification = new TextBox { Margin = new Thickness(2), IsReadOnly = true };
            fldMasterShiori = new TextBox { Margin = new Thickness(2), IsReadOnly = true };
            fldNShells = new TextBox { Margin = new Thickness(2), IsReadOnly = true };
            fldMakoto = new TextBox { Margin = new Thickness(2), IsReadOnly = true };

            bottomPanel.Children.Add(new Label { Content = "Identification:" });
            bottomPanel.Children.Add(fldIdentification);
            bottomPanel.Children.Add(new Label { Content = "Master Shiori:" });
            bottomPanel.Children.Add(fldMasterShiori);
            bottomPanel.Children.Add(new Label { Content = "Number of Shells:" });
            bottomPanel.Children.Add(fldNShells);
            bottomPanel.Children.Add(new Label { Content = "Makoto:" });
            bottomPanel.Children.Add(fldMakoto);

            DockPanel.SetDock(bottomPanel, Dock.Bottom);
            rootPanel.Children.Add(bottomPanel);
        }

        private void LoadDefaults()
        {
            // 模拟NSUserDefaults，使用 Properties.Settings 或其他持久化方案
            // 这里简化直接赋值
            UpdateScaleIndicator();
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // 保存用户设置（例如 tabViewInfo.SelectedIndex 等）
        }

        public void ShowWindow(object sender)
        {
            this.Show();
        }

        // --- UI 操作方法 ---

        private void FindBoxUpdated(object sender, TextChangedEventArgs e)
        {
            ghostManager.GetInstalledList().FindBoxUpdated();
        }

        private void TateKesi(object sender, RoutedEventArgs e)
        {
            ghostManager.GetInstalledList().BootOrQuit();
        }

        private void NetworkUpdate(object sender, RoutedEventArgs e)
        {
            ghostManager.GetInstalledList().NetworkUpdate();
        }

        private void Vanish(object sender, RoutedEventArgs e)
        {
            ghostManager.GetInstalledList().Vanish();
        }

        private void SliderScaleChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateScaleIndicator();
            ghostManager.GetInstalledList().ChangeScale(sliderScale.Value);
        }

        public void UpdateScaleIndicator()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                fldScale.Text = $"{(int)(sliderScale.Value * 100)}%";

            });
        }

        private void ToggleThumbnailDrawer()
        {
            // 模拟 Drawer，用右侧 ImageView 代替
            Application.Current.Dispatcher.Invoke(() =>
            {
                imageThumbnail.Visibility = imageThumbnail.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;

            });
        }

        // --- 可供外部调用的方法 ---
        public void SetThumbnail(BitmapImage img)
        {
            imageThumbnail.Source = img;
        }


        public void SetMasterShioriKernelName(string name)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                fldMasterShiori.Text = name;
            });
        }

        public void SetMakotoName(string name)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                fldMakoto.Text = name;
            });
        }

        public void SetIdentification(string id)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                fldIdentification.Text = id;
            });
        }

        public void SetNumberOfShells(int n)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (n == -1)
                {
                    fldNShells.Text = "";
                    fldNShells.Foreground = Brushes.Black;
                }
                else
                {
                    fldNShells.Text = n.ToString();
                    fldNShells.Foreground = (n == 1 ? Brushes.Black : Brushes.Red);
                }
            });
        }

        public void SetEnabledOfNetworkUpdateButton(bool enabled)
        {
            btnNetworkUpdate.IsEnabled = enabled;
        }

        public void SetEnabledOfVanishButton(bool enabled)
        {
            btnVanish.IsEnabled = enabled;
        }

        public Button GetBootAndQuitButton()
        {
            return btnTateKesi;
        }

        public ListView GetTableInstalled()
        {
            return tableInstalled;
        }

        public ListView GetTableShells()
        {
            return tableShell;
        }

        public TextBox GetFindBox()
        {
            return findBox;
        }

        public SCGhostPreviewView GetPreviewView()
        {
            return view_preview;

        }

        public Slider GetScaleSlider()
        {
            return sliderScale;

        }

        public TextBox GetScaleIndicator()
        {
            return fldScale;
        }

    }
















    public class SCGhostManagerWindowController1 : NSWindowController
    {
        SCGhostManager ghostManager;

        // root
        NSTableView table_installed;
        NSTabView tabview_info;
        NSTextField find_box;
        NSButton btn_thumb_view;
        NSDrawer drawer_thumbnail;

        // tab CONTROL/PREVIEW
        SCGhostPreviewView view_preview;
        NSTextField fld_scale;
        NSSlider slider_scale;
        NSButton btn_networkupdate;
        NSButton btn_tate_kesi;
        NSButton btn_vanish;

        // tab SHELL
        NSTableView table_shell;

        // tab BALLOON
        NSTableView table_balloon;

        // view DETAIL
        NSTextField fld_identification;
        NSTextField fld_mastershiori;
        NSTextField fld_n_shells;
        NSTextField fld_makoto;

        // view THUMBNAIL
        NSImageView image_thumbnail;

        public SCGhostManagerWindowController1(SCGhostManager s):base()
        {

            //super("GhostManager");
           // window();


            Window.SetFrameAutoSaveName("ghostmanager.frame");


            table_balloon = new NSTableView();

            table_balloon.SetAutoSaveName("ghostmanager.balloontable.columns");
            table_balloon.SetAutoSaveTableColumns(true);


            table_shell = new NSTableView();


            find_box = new NSTextField();


            view_preview = new SCGhostPreviewView();


            table_installed = new NSTableView();

            fld_mastershiori = new NSTextField();

            btn_tate_kesi = new NSButton();


            btn_networkupdate = new NSButton();

            btn_vanish = new NSButton();



            fld_scale = new NSTextField();

            slider_scale = new NSSlider();

            fld_identification= new NSTextField();

            fld_makoto = new NSTextField();

            fld_n_shells = new NSTextField();

            image_thumbnail = new NSImageView();

            
            // タブの復帰
            NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults();
            string tab_id = (string)defaults.ObjectForKey("ghostmanager.tabview_info.IdOflastSelectedTab");
            if (tab_id != null)
            {
                tabview_info.SelectTabViewItemWithIdentifier(tab_id);
            }

            // サムネイルドローワ表示状態の復歸
            if (defaults.IntegerForKey("ghostmanager.drawer.thumbnail.visible") != 0)
            {
                btn_thumb_view.SetIntValue(1);
                //drawer_thumbnail.open();
            }

            ghostManager = s;
        }

        public void ShowWindow(object sender)
        {
           // base.showWindow(sender);

            if (btn_thumb_view.IntValue() != 0)
            {
                drawer_thumbnail.Open(); // ドローワが開かない問題を回避するhack
                drawer_thumbnail.ContentView().Window().Display(); // ドローワが再描画されない問題を回避するhack
            }
        }

        /******** delegate methods ********/
        public bool WindowShouldClose(object sender)
        {
            // 状態をdefaultsに保存します。
            NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults();
            defaults.SetObjectForKey(tabview_info.SelectedTabViewItem().Identifier(), "ghostmanager.tabview_info.IdOflastSelectedTab");
            defaults.SetIntegerForKey(btn_thumb_view.IntValue(), "ghostmanager.drawer.thumbnail.visible");

            defaults.Synchronize();
            return true; // falseを返せばクローズが食い止められる。
        }

        public NSTextField GetFindBox()
        {
            return find_box;
        }

        public SCGhostPreviewView GetPreviewView()
        {
            return view_preview;
        }

        public NSTableView GetTableInstalled()
        {
            return table_installed;
        }

        public NSTableView GetTableBalloons()
        {
            return table_balloon;
        }

        public NSTextField GetScaleIndicator()
        {
            return fld_scale;
        }

        public NSSlider GetScaleSlider()
        {
            return slider_scale;
        }

        public void SetMasterShioriKernelName(String name)
        {
            fld_mastershiori.SetStringValue(name);
        }

        public void SetMakotoName(String name)
        {
            fld_makoto.SetStringValue(name);
        }

        public void SetIdentification(String id)
        {
            fld_identification.SetStringValue(id);
        }

        // Actions
        public void FindBoxUpdated(Object sender)
        {
            ghostManager.GetInstalledList().FindBoxUpdated();
        }

        public void Tate_kesi(Object sender)
        {
            ghostManager.GetInstalledList().BootOrQuit();
        }

        public void Networkupdate(Object sender)
        {
            ghostManager.GetInstalledList().NetworkUpdate();
        }

        public void Vanish(Object sender)
        {
            ghostManager.GetInstalledList().Vanish();
        }

        public void ReloadLists(Object sender)
        {
            /*
            SCOldTypeConverter.convertAll();
            SCOldBalloonConverter.convertAll();
            SCGhostThumbnailMover.moveAll();
            ghostManager.reloadLists();
            */
        }

        public void Slider_scale(Object sender)
        {
            UpdateScaleIndicator();
            ghostManager.GetInstalledList().ChangeScale(slider_scale.DoubleValue());
        }

        public void UpdateScaleIndicator()
        {
            String value = (int)(slider_scale.DoubleValue() * 100) + "%";
            fld_scale.SetStringValue(value);
        }

        public NSTableView GetTableShells()
        {
            return table_shell;
        }

        public void SetNumberOfShells(int n)
        {
            // -1なら消去します。
            if (n == -1)
            {
                fld_n_shells.SetStringValue("");
            }
            else
            {
                fld_n_shells.SetIntValue(n);
            }
            fld_n_shells.SetTextColor(n == 1 || n == -1 ? NSColor.BlackColor() : NSColor.RedColor());
        }

        public void SetThumbnail(NSImage img)
        {
            image_thumbnail.SetImage(img);
        }

        public void SetEnabledOfNetworkUpdateButton(bool value)
        {
            btn_networkupdate.SetEnabled(value);
        }

        public void SetEnabledOfVanishButton(bool value)
        {
            btn_vanish.SetEnabled(value);
        }

        public NSButton GetBootAndQuitButton()
        {
            return btn_tate_kesi;
        }
 
         
    }
}
