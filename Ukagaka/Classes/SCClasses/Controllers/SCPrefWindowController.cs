using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Win32;


namespace Ukagaka
{
    public class SCPrefWindowController : Window, INotifyPropertyChanged
    {
        private static SCPrefWindowController _sharedPrefWindow = null;

        // UI Controls
        private CheckBox _bootingCheckGhostThumbnail;
        private CheckBox _bootingCheckInverseType;
        private CheckBox _bootingCheckOldestType;
        private CheckBox _bootingCheckZeropointBalloon;

        private CheckBox _mailcheckCheckEnable;
        private TextBox _mailcheckFldServer;
        private TextBox _mailcheckFldUsername;
        private PasswordBox _mailcheckFldPassword;
        private CheckBox _mailcheckCheckAutocheck;
        private TextBox _mailcheckFldAutocheckInterval;

        private TextBox _sstpLogview;
        private RadioButton _sstpPortDefault;
        private RadioButton _sstpPortAlternate;
        private CheckBox _sstpSwitch;

        private DataGrid _pluginList;

        private RadioButton _displaySbLevelNormal;
        private RadioButton _displaySbLevelFloating;
        private RadioButton _displaySbLevelModal;
        private TextBox _displaySbFontnameField;
        private TextBox _displaySbFontsizeField;
        private Slider _displaySbSliderTransparency;
        private TextBox _displaySbFldTransparency;
        private CheckBox _displaySbBalloonFadeout;
        private CheckBox _displaySbBalloonClickthrough;
        private Slider _displaySbSliderWaitrate;
        private TextBox _displaySbFldWaitrate;

        private TextBox _displayGmPreviewFpath;

        private CheckBox _miscAlwaysShowVanish;
        private CheckBox _miscDeleteAfterOnlineInstall;
        private CheckBox _miscLoadWholeSurfaces;
        private CheckBox _miscDisableSerikoAnimation;
        private CheckBox _miscShowDevInterfaces;
        private CheckBox _miscLightmodeOnSstp;
        private CheckBox _miscResetSurfaceOnSstp;

        private SCPluginListDataSource _pluginListDatasource;

        public event PropertyChangedEventHandler PropertyChanged;

        public static SCPrefWindowController SharedPrefWindowController()
        {
            if (_sharedPrefWindow == null)
            {
                _sharedPrefWindow = new SCPrefWindowController();
            }
            return _sharedPrefWindow;
        }

        public SCPrefWindowController()
        {
            Title = "Preferences";
            Width = 800;
            Height = 600;

            // Initialize UI components
            InitializeComponent();

            _pluginListDatasource = new SCPluginListDataSource();
            _pluginList.ItemsSource = _pluginListDatasource.Plugins;

            Closing += (sender, e) =>
            {
                e.Cancel = !WindowShouldClose();
                if (!e.Cancel)
                {
                    _sharedPrefWindow = null;
                }
            };
        }

        private void InitializeComponent()
        {
            // This would normally be done in XAML, but showing code-behind for conversion

            var mainGrid = new Grid();

            var tabControl = new TabControl();

            // Boot Tab
            var bootTab = new TabItem { Header = "Boot" };
            var bootStack = new StackPanel { Orientation = Orientation.Vertical };
            _bootingCheckGhostThumbnail = new CheckBox { Content = "Show ghost thumbnails" };
            _bootingCheckInverseType = new CheckBox { Content = "Inverse type conversion" };
            _bootingCheckOldestType = new CheckBox { Content = "Use oldest type" };
            _bootingCheckZeropointBalloon = new CheckBox { Content = "Zero point balloon" };
            bootStack.Children.Add(_bootingCheckGhostThumbnail);
            bootStack.Children.Add(_bootingCheckInverseType);
            bootStack.Children.Add(_bootingCheckOldestType);
            bootStack.Children.Add(_bootingCheckZeropointBalloon);
            bootTab.Content = bootStack;

            // Mail Tab
            var mailTab = new TabItem { Header = "Mail" };
            var mailGrid = new Grid();
            mailGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            mailGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            _mailcheckCheckEnable = new CheckBox { Content = "Enable mail check" };
            _mailcheckCheckEnable.Checked += (s, e) => MailcheckCheckEnable();
            _mailcheckCheckEnable.Unchecked += (s, e) => MailcheckCheckEnable();
            Grid.SetColumnSpan(_mailcheckCheckEnable, 2);

            mailGrid.Children.Add(_mailcheckCheckEnable);

            // Add other mail controls similarly...

            mailTab.Content = mailGrid;

            // Add other tabs...

            tabControl.Items.Add(bootTab);
            tabControl.Items.Add(mailTab);

            mainGrid.Children.Add(tabControl);
            Content = mainGrid;
        }

        public new void Show()
        {
            // Load settings from application properties
            var settings = Properties.Settings.Default;

            // Boot settings
            _bootingCheckInverseType.IsChecked = settings.BootingConverterInverseType;
            _bootingCheckGhostThumbnail.IsChecked = settings.BootingConverterThumbnail;
            _bootingCheckZeropointBalloon.IsChecked = settings.BootingConverterZeropointBalloon;

            // Mail settings
            _mailcheckFldServer.Text = settings.MailcheckServer ?? "";
            _mailcheckFldUsername.Text = settings.MailcheckUsername ?? "";
            _mailcheckFldPassword.Password = settings.MailcheckPassword ?? "";
            _mailcheckCheckEnable.IsChecked = settings.MailcheckCheckEnable;
            MailcheckCheckEnable();

            _mailcheckCheckAutocheck.IsChecked = settings.MailcheckCheckAutocheck;
            MailcheckCheckAutocheck();

            _mailcheckFldAutocheckInterval.Text = settings.MailcheckAutocheckInterval.ToString();

            // SSTP settings
            _sstpSwitch.IsChecked = settings.SstpSwitch;
            SstpSwitch();

            if (settings.SstpPort == 11000)
            {
                _sstpPortDefault.IsChecked = true;
            }
            else
            {
                _sstpPortAlternate.IsChecked = true;
            }

            // Display settings
            switch (settings.DisplayLevelselect)
            {
                case (int)WindowLevel.Normal:
                    _displaySbLevelNormal.IsChecked = true;
                    break;
                case (int)WindowLevel.Floating:
                    _displaySbLevelFloating.IsChecked = true;
                    break;
                case (int)WindowLevel.Modal:
                    _displaySbLevelModal.IsChecked = true;
                    break;
            }

            _displaySbFontnameField.Text = settings.DisplayFontname ?? "Segoe UI";
            _displaySbFontsizeField.Text = settings.DisplayFontsize.ToString("0");

            _displaySbSliderTransparency.Value = settings.DisplaySliderTransparency;
            DisplaySbSliderTransparency();

            _displaySbSliderWaitrate.Value = settings.DisplayBalloonWaitRate;
            DisplaySbSliderWaitrate();

            _displaySbBalloonFadeout.IsChecked = settings.DisplayBalloonFadeout;
            _displaySbBalloonClickthrough.IsChecked = settings.DisplayBalloonClickthrough;
            SCFoundation.BALLOON_USES_CLICK_THROUGH = _displaySbBalloonClickthrough.IsChecked == true;

            _displayGmPreviewFpath.Text = settings.DisplayGhostmanagerPreviewFilepath ?? "";

            // Misc settings
            _miscAlwaysShowVanish.IsChecked = settings.MiscAlwaysShowVanish;
            _miscDeleteAfterOnlineInstall.IsChecked = settings.MiscDeleteAfterOnlineInstall;
            _miscLoadWholeSurfaces.IsChecked = settings.MiscLoadWholeSurfacesOnBootingSurfaceServer;
            _miscDisableSerikoAnimation.IsChecked = settings.MiscDisableSerikoAnimation;
            _miscShowDevInterfaces.IsChecked = settings.MiscShowDevInterfaces;
            _miscLightmodeOnSstp.IsChecked = settings.MiscLightmodeOnSstp;
            _miscResetSurfaceOnSstp.IsChecked = settings.MiscResetSurfaceOnSstp;

            SCFoundation.LOAD_WHOLE_SURFACES_ON_BOOTING_SURFACE_SERVER = _miscLoadWholeSurfaces.IsChecked == true;
            SCFoundation.STOP_SERIKO = _miscDisableSerikoAnimation.IsChecked == true;

            base.Show();
        }

        public void ReloadPluginList()
        {
            _pluginListDatasource.ReloadData();
            _pluginList.Items.Refresh();
        }

        public void AddStringToSSTPLog(string str)
        {
            _sstpLogview.AppendText(str + Environment.NewLine);
            _sstpLogview.ScrollToEnd();
        }

        private bool WindowShouldClose()
        {
            var settings = Properties.Settings.Default;

            // Boot settings
            settings.BootingConverterInverseType = _bootingCheckInverseType.IsChecked == true;
            settings.BootingConverterThumbnail = _bootingCheckGhostThumbnail.IsChecked == true;
            settings.BootingConverterZeropointBalloon = _bootingCheckZeropointBalloon.IsChecked == true;

            // Mail settings
            MailcheckUpdate();

            // SSTP settings
            settings.SstpSwitch = _sstpSwitch.IsChecked == true;
            settings.SstpPort = _sstpPortDefault.IsChecked == true ? 11000 : 9801;

            // Display settings
            settings.DisplayLevelselect = _displaySbLevelNormal.IsChecked == true ? (int) WindowLevel.Normal :
                                         _displaySbLevelFloating.IsChecked == true ? (int)WindowLevel.Floating :
                                         (int)WindowLevel.Modal;

            settings.DisplayFontname = _displaySbFontnameField.Text;
            if (float.TryParse(_displaySbFontsizeField.Text, out float fontSize))
            {
                settings.DisplayFontsize = (int)fontSize;
            }

            settings.DisplaySliderTransparency = _displaySbSliderTransparency.Value;
            settings.DisplayBalloonWaitRate = _displaySbSliderWaitrate.Value;
            settings.DisplayBalloonFadeout = _displaySbBalloonFadeout.IsChecked == true;
            settings.DisplayBalloonClickthrough = _displaySbBalloonClickthrough.IsChecked == true;
            SCFoundation.BALLOON_USES_CLICK_THROUGH = settings.DisplayBalloonClickthrough;

            settings.DisplayGhostmanagerPreviewFilepath = _displayGmPreviewFpath.Text;

            // Misc settings
            settings.MiscAlwaysShowVanish = _miscAlwaysShowVanish.IsChecked == true;
            settings.MiscDeleteAfterOnlineInstall = _miscDeleteAfterOnlineInstall.IsChecked == true;
            settings.MiscLoadWholeSurfacesOnBootingSurfaceServer = _miscLoadWholeSurfaces.IsChecked == true;
            settings.MiscDisableSerikoAnimation = _miscDisableSerikoAnimation.IsChecked == true;
            settings.MiscShowDevInterfaces = _miscShowDevInterfaces.IsChecked == true;
            settings.MiscLightmodeOnSstp = _miscLightmodeOnSstp.IsChecked == true;
            settings.MiscResetSurfaceOnSstp = _miscResetSurfaceOnSstp.IsChecked == true;

            SCFoundation.LOAD_WHOLE_SURFACES_ON_BOOTING_SURFACE_SERVER = settings.MiscLoadWholeSurfacesOnBootingSurfaceServer;
            SCFoundation.STOP_SERIKO = settings.MiscDisableSerikoAnimation;

            settings.Save();

            return true;
        }

        private void FontChanged(object sender, EventArgs e)
        {
            // Handle font change from font dialog
            // Implementation would depend on how you implement font selection
        }

        // Action methods
        private void MailcheckCheckEnable()
        {
            bool isEnabled = _mailcheckCheckEnable.IsChecked == true;

            _mailcheckCheckAutocheck.IsEnabled = isEnabled;
            _mailcheckFldServer.IsEnabled = isEnabled;
            _mailcheckFldUsername.IsEnabled = isEnabled;
            _mailcheckFldPassword.IsEnabled = isEnabled;

            MailcheckCheckAutocheck();
        }

        private void MailcheckCheckAutocheck()
        {
            bool isEnabled = _mailcheckCheckAutocheck.IsChecked == true && _mailcheckCheckEnable.IsChecked == true;
            _mailcheckFldAutocheckInterval.IsEnabled = isEnabled;
        }

        private void MailcheckUpdate()
        {
            var settings = Properties.Settings.Default;

            settings.MailcheckCheckEnable = _mailcheckCheckEnable.IsChecked == true;
            settings.MailcheckCheckAutocheck = _mailcheckCheckAutocheck.IsChecked == true;
            settings.MailcheckServer = _mailcheckFldServer.Text;
            settings.MailcheckUsername = _mailcheckFldUsername.Text;
            settings.MailcheckPassword = _mailcheckFldPassword.Password;

            if (int.TryParse(_mailcheckFldAutocheckInterval.Text, out int interval))
            {
                settings.MailcheckAutocheckInterval = interval;
            }

            settings.Save();

            // Start or stop mail check daemon
            if (_mailcheckCheckEnable.IsChecked == true && _mailcheckCheckAutocheck.IsChecked == true)
            {
                SCFoundation.SharedFoundation().StartMailCheckDaemon();
            }
            else
            {
                SCFoundation.SharedFoundation().StopMailCheckDaemon();
            }
        }

        private void SstpPortSelect()
        {
            int port = _sstpPortDefault.IsChecked == true ? 11000 : 9801;

            SCFoundation.SharedFoundation().StopSSTPServer();
            SCFoundation.SharedFoundation().StartSSTPServer(port);
        }

        private void SstpSwitch()
        {
            if (_sstpSwitch.IsChecked == true)
            {
                SCFoundation.SharedFoundation().StartSSTPServer();
                _sstpPortDefault.IsEnabled = true;
                _sstpPortAlternate.IsEnabled = true;
            }
            else
            {
                SCFoundation.SharedFoundation().StopSSTPServer();
                _sstpPortDefault.IsEnabled = false;
                _sstpPortAlternate.IsEnabled = false;
            }
        }

        private void DisplaySbLevelSelect()
        {
            WindowLevel level = WindowLevel.Normal;
            if (_displaySbLevelFloating.IsChecked == true)
            {
                level = WindowLevel.Floating;
            }
            else if (_displaySbLevelModal.IsChecked == true)
            {
                level = WindowLevel.Modal;
            }

            foreach (var session in SCFoundation.SharedFoundation().GetSessionsList())
            {
                session.GetHontai().SetLevel((int)level);
                session.GetUnyuu().SetLevel((int)level);
                session.GetHontaiBalloon().SetLevel((int)level);
                session.GetUnyuuBalloon().SetLevel((int)level);
            }
        }

        private void DisplaySbBalloonFadeout()
        {
            bool fadeout = _displaySbBalloonFadeout.IsChecked == true;

            foreach (var session in SCFoundation.SharedFoundation().GetSessionsList())
            {
                session.GetHontaiBalloon().SetDoesFadeOut(fadeout);
                session.GetUnyuuBalloon().SetDoesFadeOut(fadeout);
            }
        }

        private void DisplaySbShowFontPanel()
        {
            // Implementation would use WPF's font dialog
            var fontDialog = new System.Windows.Forms.FontDialog();
            if (fontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Handle font selection
            }
        }

        private void DisplaySbSliderTransparency()
        {
            double val = _displaySbSliderTransparency.Value;
            _displaySbFldTransparency.Text = $"{(int)(val * 100)}%";

            foreach (var session in SCFoundation.SharedFoundation().GetSessionsList())
            {
                session.GetHontaiBalloon().SetTransparency(val);
                session.GetUnyuuBalloon().SetTransparency(val);
            }

            Properties.Settings.Default.DisplaySliderTransparency = val;
        }

        private void DisplaySbSliderWaitrate()
        {
            double val = _displaySbSliderWaitrate.Value;
            _displaySbFldWaitrate.Text = $"{(int)(val * 100)}%";

            SCScriptRunner.SetWaitRatio(val);

            Properties.Settings.Default.DisplayBalloonWaitRate = val;
        }

        private void DisplayGmPreviewSelect()
        {
            var openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog() == true)
            {
                _displayGmPreviewFpath.Text = openDialog.FileName;
            }
        }

        private void MiscAlwaysShowVanish()
        {
            Properties.Settings.Default.MiscAlwaysShowVanish = _miscAlwaysShowVanish.IsChecked == true;
            Properties.Settings.Default.Save();
        }

        private void MiscDeleteAfterOnlineInstall()
        {
            Properties.Settings.Default.MiscDeleteAfterOnlineInstall = _miscDeleteAfterOnlineInstall.IsChecked == true;
            Properties.Settings.Default.Save();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ShowWindow()
        {
            this.Show();

        }
    }

    public class SCPluginListDataSource : INotifyPropertyChanged
    {
        private ObservableCollection<SCPluginListDataElement> _plugins = new ObservableCollection<SCPluginListDataElement>();

        public ObservableCollection<SCPluginListDataElement> Plugins => _plugins;

        public event PropertyChangedEventHandler PropertyChanged;

        public SCPluginListDataSource()
        {
            ReloadData();
        }

        public void ReloadData()
        {
            _plugins.Clear();

            var plugins = SCFoundation.SharedFoundation().GetPluginManager().GetPlugins();
            foreach (var plugin in plugins)
            {
                string type = SCStringsServer.GetStrFromMainDic($"pref.plugin.type.{plugin.GetType()}");
                string name = plugin.GetName();
                string info;

                if (plugin.GetType() == "hlsensor")
                {
                    var resp = ((SCHLSensorPlugin)plugin).Request("GET Description HEADLINE/1.0\r\nSender: " +
                                                                  SCFoundation.STRING_FOR_SENDER + "\r\n\r\n");
                    info = resp.GetHeader("Name") + " @ " + resp.GetHeader("Data-Location");
                }
                else if (plugin.GetType() == "shiori")
                {
                    info = SCShioriLoader.GetModuleInfo(plugin.GetPluginClass());
                }
                else if (plugin.GetType() == "makoto")
                {
                    info = SCMakotoLoader.GetModuleInfo(plugin.GetPluginClass());
                }
                else
                {
                    info = "";
                }

                _plugins.Add(new SCPluginListDataElement(name, type, info));
            }

            OnPropertyChanged(nameof(Plugins));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

       


    }

    public class SCPluginListDataElement
    {
        public string Name { get; }
        public string Type { get; }
        public string Info { get; }

        public SCPluginListDataElement(string name, string type, string info)
        {
            Name = name;
            Type = type;
            Info = info;
        }
    }

    public enum WindowLevel
    {
        Normal,
        Floating,
        Modal
    }
}