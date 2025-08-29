using Cocoa.AppKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Ukagaka;

namespace Ukagaka
{


    public class SCShellContextMenuMaker
    {
        private readonly SCSession _session;

        private ContextMenu _hontaiMenu;
        private ContextMenu _unyuuMenu;

        private MenuItem _sakuraRecommended;
        private MenuItem _portal;
        private MenuItem _devInterfaces;
        private MenuItem _changeGhost;
        private MenuItem _changeShell;
        private MenuItem _changeBalloon;
        private MenuItem _networkUpdate;
        private MenuItem _vanish;
        private MenuItem _headlineSensor;
        private MenuItem _aboutGhost;
        private MenuItem _aboutShell;
        private MenuItem _disappear;

        private MenuItem _unyuuRecommended;

        public SCShellContextMenuMaker(SCSession session)
        {
            _session = session;

            InitializeHontaiMenu();
            InitializeUnyuuMenu();
        }

        private void InitializeHontaiMenu()
        {
            _hontaiMenu = new ContextMenu();

            // さくらのおすすめ
            _sakuraRecommended = new MenuItem();
            _hontaiMenu.Items.Add(_sakuraRecommended);

            // ポータル
            _portal = new MenuItem();
            _hontaiMenu.Items.Add(_portal);

            // 区切り線
            AddSeparator(_hontaiMenu);

            // ゴースト変更
            _changeGhost = new MenuItem { Header = SCStringsServer.GetStrFromMainDic("contextmenu.changeghost") };
            _hontaiMenu.Items.Add(_changeGhost);

            // シェル
            _changeShell = new MenuItem { Header = SCStringsServer.GetStrFromMainDic("contextmenu.shell") };
            _hontaiMenu.Items.Add(_changeShell);

            // バルーン
            _changeBalloon = new MenuItem { Header = SCStringsServer.GetStrFromMainDic("contextmenu.balloon") };
            _hontaiMenu.Items.Add(_changeBalloon);

            // 区切り線
            AddSeparator(_hontaiMenu);

            // ネットワーク更新
            _networkUpdate = new MenuItem();
            _networkUpdate.Click += (sender, e) => NetworkUpdateSelected();
            _hontaiMenu.Items.Add(_networkUpdate);

            // 消滅通告
            var defaults = NSUserDefaults.StandardUserDefaults();
            if (defaults.IntegerForKey("misc.always_show_vanish") == 1 ||
                _session.GetStringFromShiori("vanishbuttonvisible") == null ||
                !_session.GetStringFromShiori("vanishbuttonvisible").Equals("0"))
            {
                _vanish = new MenuItem();
                _vanish.Click += (sender, e) => VanishSelected();
                _hontaiMenu.Items.Add(_vanish);
            }

            // 区切り線
            AddSeparator(_hontaiMenu);

            // 開発用インターフェイス
            if (defaults.IntegerForKey("misc.show_dev_interfaces") == 1)
            {
                _devInterfaces = new MenuItem { Header = SCStringsServer.GetStrFromMainDic("contextmenu.development") };
                _hontaiMenu.Items.Add(_devInterfaces);

                var submenu = MakeDevInterfacesSubMenu();
                _devInterfaces.Items.Add(submenu);

                // 区切り線
                AddSeparator(_hontaiMenu);
            }

            // ヘッドラインセンサ
            _headlineSensor = new MenuItem { Header = SCStringsServer.GetStrFromMainDic("contextmenu.hlsensor") };
            _hontaiMenu.Items.Add(_headlineSensor);

            // 区切り線
            AddSeparator(_hontaiMenu);

            // このゴーストについて...
            _aboutGhost = new MenuItem
            {
                Header = SCStringsServer.GetStrFromMainDic("contextmenu.aboutghost"),
                IsEnabled = _session.GetReadmeFile().Exists(),
            };
            _aboutGhost.Click += (sender, e) => AboutThisGhost();
            _hontaiMenu.Items.Add(_aboutGhost);

            // このシェルについて...
            _aboutShell = new MenuItem
            {
                Header = SCStringsServer.GetStrFromMainDic("contextmenu.aboutshell")
            };
            _aboutShell.Click += (sender, e) => AboutThisShell();
            _hontaiMenu.Items.Add(_aboutShell);

            // 区切り線
            AddSeparator(_hontaiMenu);

            // 消す
            _disappear = new MenuItem
            {
                Header = SCStringsServer.GetStrFromMainDic("contextmenu.close")
            };
            _disappear.Click += (sender, e) => CloseSelected();
            _hontaiMenu.Items.Add(_disappear);
        }

        private void InitializeUnyuuMenu()
        {
            _unyuuMenu = new ContextMenu();

            // うにゅうのおすすめ
            _unyuuRecommended = new MenuItem();
            _unyuuMenu.Items.Add(_unyuuRecommended);
        }

        public void ReconstructMenu(int type)
        {
            if (type == SCFoundation.HONTAI)
            {
                // おすすめ
                string title = _session.GetStringFromShiori("sakura.recommendbuttoncaption");
                if (string.IsNullOrEmpty(title))
                {
                    title = SCStringsServer.GetStrFromMainDic("contextmenu.close", new[] { _session.GetSelfName() });
                }
                _sakuraRecommended.Header = title;
                SetTable(_sakuraRecommended, "sakura.recommendsites", type);

                // ポータル
                title = _session.GetStringFromShiori("sakura.portalbuttoncaption");
                if (string.IsNullOrEmpty(title))
                {
                    title = SCStringsServer.GetStrFromMainDic("contextmenu.portal");
                }
                _portal.Header = title;
                SetTable(_portal, "sakura.portalsites", type);

                // ゴースト変更
                if (_session.IsStatusClosing())
                {
                    _changeGhost.IsEnabled = false;
                }
                else
                {
                    var submenu = SCGhostManager.SharedGhostManager().MakeGhostMenu(_session);
                    _changeGhost.Items.Clear();
                    _changeGhost.Items.Add(submenu);
                }

                // シェル
                if (_session.IsShellChangingSessionRunningNow() ||
                    _session.IsStatusClosing() ||
                    _session.IsNetworkUpdaterRunningNow())
                {
                    _changeShell.IsEnabled = false;
                }
                else
                {
                    var submenu = SCGhostManager.SharedGhostManager().MakeShellMenu(_session);
                    _changeShell.Items.Clear();
                    _changeShell.Items.Add(submenu);
                    _changeShell.IsEnabled = true;
                }

                // バルーン
                var balloonSubmenu = SCGhostManager.SharedGhostManager().MakeBalloonMenu(_session);
                _changeBalloon.Items.Clear();
                _changeBalloon.Items.Add(balloonSubmenu);

                // ネットワーク更新
                title = _session.GetStringFromShiori("updatebuttoncaption");
                if (string.IsNullOrEmpty(title))
                {
                    title = SCStringsServer.GetStrFromMainDic("contextmenu.networkupdate");
                }
                _networkUpdate.Header = title;

                if (_session.GetStringFromShiori("homeurl") != null &&
                    !_session.IsNetworkUpdaterRunningNow() &&
                    !_session.IsStatusClosing())
                {
                    _networkUpdate.IsEnabled = true;
                }
                else
                {
                    _networkUpdate.IsEnabled = false;
                }

                // 消滅通告
                if (_vanish != null)
                {
                    title = _session.GetStringFromShiori("vanishbuttoncaption");
                    if (string.IsNullOrEmpty(title))
                    {
                        title = SCStringsServer.GetStrFromMainDic("contextmenu.vanish");
                    }
                    _vanish.Header = title;

                    if (!_session.IsNetworkUpdaterRunningNow() &&
                        !_session.IsStatusClosing())
                    {
                        _vanish.IsEnabled = true;
                    }
                    else
                    {
                        _vanish.IsEnabled = false;
                    }
                }

                // ヘッドラインセンサ
                SetHeadlineSensorTable(_headlineSensor, type);

                // このシェルについて...
                _aboutShell.IsEnabled = _session.GetCurrentShell().GetReadmeFile().Exists();

                // 消す
                if (!_session.IsStatusClosing() &&
                    !_session.IsNetworkUpdaterRunningNow())
                {
                    _disappear.IsEnabled = true;
                }
                else
                {
                    _disappear.IsEnabled = false;
                }
            }
            else if (type == SCFoundation.UNYUU)
            {
                // うにゅうのおすすめ
                string title = _session.GetStringFromShiori("kero.recommendbuttoncaption");
                if (title == null)
                {
                    title = SCStringsServer.GetStrFromMainDic("contextmenu.close", new[] { _session.GetKeroName() });
                }
                _unyuuRecommended.Header = title;
                SetTable(_unyuuRecommended, "kero.recommendsites", type);
            }
        }

        private void AddSeparator(ItemsControl menu)
        {
            menu.Items.Add(new Separator());
        }

        private void SetTable(MenuItem target, string resourceType, int type)
        {
            string rcmTable = _session.GetStringFromShiori(resourceType);
            if (string.IsNullOrEmpty(rcmTable))
            {
                target.IsEnabled = false;
                return;
            }

            try
            {
                var submenu = new ContextMenu();

                // Split by 0x02
                var rows = rcmTable.Split('\u0002');
                foreach (var row in rows)
                {
                    // Split by 0x01
                    var columns = row.Split('\u0001');

                    string name = columns[0];
                    if (name == "-")
                    {
                        submenu.Items.Add(new Separator());
                    }
                    else
                    {
                        string url = columns[1];
                        string banner = columns.Length > 2 ? columns[2] : null;

                        var item = new MenuItem
                        {
                            Header = name,
                            Tag = new UrlMenuItemData { Url = url, Banner = banner }
                        };
                        item.Click += (sender, e) => UrlMenuItemSelected(sender);

                        if (!string.IsNullOrEmpty(banner))
                        {
                            var image = new Image
                            {
                                Source = SCFoundation.SharedFoundation().GetBannerServer().GetBanner(banner),
                                Width = 16,
                                Height = 16
                            };
                            item.Icon = image;
                        }

                        submenu.Items.Add(item);
                    }
                }

                target.Items.Add(submenu);
            }
            catch
            {
                target.IsEnabled = false;
            }
        }

        private void SetHeadlineSensorTable(MenuItem target, int type)
        {
            var submenu = new ContextMenu();

            var sensors = SCFoundation.SharedFoundation().GetPluginManager().GetPlugins("hlsensor");
            if (sensors.Length == 0)
            {
                target.IsEnabled = false;
                return;
            }

            target.IsEnabled = true;

            foreach (var sensor in sensors.OfType<SCHLSensorPlugin>())
            {
                var resp = sensor.Request($"GET Description HEADLINE/1.0\r\nSender: {SCFoundation.STRING_FOR_SENDER}\r\n\r\n");
                string title = resp.GetHeader("Name");

                var item = new MenuItem
                {
                    Header = title,
                    Tag = new HLSensorMenuItemData { Plugin = sensor }
                };
                item.Click += (sender, e) => HLSensorMenuItemSelected(sender);
                submenu.Items.Add(item);
            }

            target.Items.Add(submenu);
        }

        private ContextMenu MakeDevInterfacesSubMenu()
        {
            var submenu = new ContextMenu();

            // SHIORI/MAKOTOをリロード
            var item = new MenuItem
            {
                Header = SCStringsServer.GetStrFromMainDic("contextmenu.development.reload")
            };
            item.Click += (sender, e) => ReloadShioriAndMakoto();
            submenu.Items.Add(item);

            return submenu;
        }

        public void ReloadShioriAndMakoto()
        {
            _session.GetMasterSpirit().ReloadAll();
        }

        public ContextMenu GetMenu(int type)
        {
            return type == SCFoundation.HONTAI ? _hontaiMenu : _unyuuMenu;
        }

        public void NetworkUpdateSelected()
        {
            _session.DoNetworkUpdate();
        }

        public void CloseSelected()
        {
            SCGhostManager.SharedGhostManager().CloseGhost(_session);
        }

        public void VanishSelected()
        {
            _session.OpenVanishDialog();
        }

        private void UrlMenuItemSelected(object sender)
        {
            if (!(sender is MenuItem menuItem) || !(menuItem.Tag is UrlMenuItemData data)) return;

            try
            {
                Process.Start(new ProcessStartInfo(data.Url) { UseShellExecute = true });
            }
            catch { }
        }

        private void HLSensorMenuItemSelected(object sender)
        {
            if (!(sender is MenuItem menuItem) || !(menuItem.Tag is HLSensorMenuItemData data)) return;

            _session.DoHLSensing(data.Plugin);
        }

        public void AboutThisGhost()
        {
            var readmeFile = _session.GetReadmeFile();
            if (readmeFile.Exists())
            {
                Process.Start(new ProcessStartInfo(readmeFile.GetFullName()) { UseShellExecute = true });
            }
        }

        public void AboutThisShell()
        {
            var readmeFile = _session.GetCurrentShell().GetReadmeFile();
            if (readmeFile.Exists())
            {
                Process.Start(new ProcessStartInfo(readmeFile.GetFullName()) { UseShellExecute = true });
            }
        }


        private class UrlMenuItemData
        {
            public string Url { get; set; }
            public string Banner { get; set; }
        }

        private class HLSensorMenuItemData
        {
            public SCHLSensorPlugin Plugin { get; set; }
        }









        public class SCShellContextMenuMaker1
        {
            SCSession session;

            NSMenu hontaiMenu;
            NSMenu unyuuMenu;
            SCContextMenuView hontaiMenuView;
            SCContextMenuView unyuuMenuView;

            NSMenuItem sakura_recommended;
            NSMenuItem portal;
            NSMenuItem dev_interfaces;
            NSMenuItem change_ghost;
            NSMenuItem change_shell;
            NSMenuItem change_balloon;
            NSMenuItem network_update;
            NSMenuItem vanish;
            NSMenuItem headline_sensor;
            NSMenuItem about_ghost;
            NSMenuItem about_shell;
            NSMenuItem disappear;

            NSMenuItem unyuu_recommended;

            private NSSelector selector_urlMenuItemSelected;
            private NSSelector selector_hlsensorMenuItemSelected;

            public SCShellContextMenuMaker1(SCSession s)
            {
                session = s;

                selector_urlMenuItemSelected = new NSSelector("urlMenuItemSelected", new object());
                selector_hlsensorMenuItemSelected = new NSSelector("hlsensorMenuItemSelected", new object());

                hontaiMenu = new NSMenu();
                hontaiMenu.SetAutoenablesItems(false);
                hontaiMenuView = new SCContextMenuView(session, SCFoundation.HONTAI, true);
                hontaiMenu.SetMenuRepresentation(hontaiMenuView);

                // さくらのおすすめ
                sakura_recommended = new NSMenuItem("", null, "");
                hontaiMenu.AddItem(sakura_recommended);

                // ポータル
                portal = new NSMenuItem("", null, "");
                hontaiMenu.AddItem(portal);

                // 区切り線
                AddSeparator(hontaiMenu);

                // ゴースト変更
                change_ghost = new NSMenuItem(SCStringsServer.GetStrFromMainDic("contextmenu.changeghost"), null, "");
                hontaiMenu.AddItem(change_ghost);

                // シェル
                change_shell = new NSMenuItem(SCStringsServer.GetStrFromMainDic("contextmenu.Shell"), null, "");
                hontaiMenu.AddItem(change_shell);

                // バルーン
                change_balloon = new NSMenuItem(SCStringsServer.GetStrFromMainDic("contextmenu.balloon"), null, "");
                hontaiMenu.AddItem(change_balloon);

                // 区切り線
                AddSeparator(hontaiMenu);

                // ネットワーク更新
                network_update = new NSMenuItem("", null, "");
                network_update.SetAction(new NSSelector("networkUpdateSelected", new object()));
                network_update.SetTarget(this);
                hontaiMenu.AddItem(network_update);

                // 消滅通告
                NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults();
                if (defaults.IntegerForKey("misc.always_show_vanish") == 1 ||
                    session.GetStringFromShiori("vanishbuttonvisible") == null ||
                    !session.GetStringFromShiori("vanishbuttonvisible").Equals("0"))
                {
                    vanish = new NSMenuItem("", null, "");
                    vanish.SetAction(new NSSelector("vanishSelected", new object()));
                    vanish.SetTarget(this);
                    hontaiMenu.AddItem(vanish);
                }

                else
                {
                    vanish = null;
                }

                // 区切り線
                AddSeparator(hontaiMenu);

                // 開発用インターフェイス
                if (defaults.IntegerForKey("misc.Show_dev_interfaces") == 1)
                {
                    dev_interfaces = new NSMenuItem(SCStringsServer.GetStrFromMainDic("contextmenu.development"), null, "");
                    hontaiMenu.AddItem(dev_interfaces);

                    NSMenu submenu = MakeDevInterfacesSubMenu();
                    SCContextMenuView submenuView = new SCContextMenuView(session, SCFoundation.HONTAI);
                    submenu.SetMenuRepresentation(submenuView);
                    dev_interfaces.SetSubmenu(submenu);
                    SetCustumCells(submenuView, SCFoundation.HONTAI);

                    // 区切り線
                    AddSeparator(hontaiMenu);
                }

                // ヘッドラインセンサ
                headline_sensor = new NSMenuItem(SCStringsServer.GetStrFromMainDic("contextmenu.Hlsensor"), null, "");
                hontaiMenu.AddItem(headline_sensor);

                // 区切り線
                AddSeparator(hontaiMenu);

                // このゴーストについて...
                about_ghost = new NSMenuItem(SCStringsServer.GetStrFromMainDic("contextmenu.aboutghost"), null, "");
                about_ghost.SetEnabled(session.GetReadmeFile().Exists());
                about_ghost.SetAction(new NSSelector("aboutThisGhost", new object()));
                about_ghost.SetTarget(this);
                hontaiMenu.AddItem(about_ghost);

                // このシェルについて...
                about_shell = new NSMenuItem(SCStringsServer.GetStrFromMainDic("contextmenu.aboutshell"), null, "");
                about_shell.SetAction(new NSSelector("aboutThisShell", new object()));
                about_shell.SetTarget(this);
                hontaiMenu.AddItem(about_shell);

                // 区切り線
                AddSeparator(hontaiMenu);

                // 消す
                disappear = new NSMenuItem(SCStringsServer.GetStrFromMainDic("contextmenu.close"), null, "");
                disappear.SetAction(new NSSelector("closeSelected", new object()));
                disappear.SetTarget(this);
                hontaiMenu.AddItem(disappear);

                // 以上全ての項目にカスタムセルを設定。
                SetCustumCells(hontaiMenuView, SCFoundation.HONTAI);


                unyuuMenu = new NSMenu();
                unyuuMenu.SetAutoenablesItems(false);
                unyuuMenuView = new SCContextMenuView(session, SCFoundation.UNYUU, true);
                unyuuMenu.SetMenuRepresentation(unyuuMenuView);

                // うにゅうのおすすめ
                unyuu_recommended = new NSMenuItem("", null, "");
                unyuuMenu.AddItem(unyuu_recommended);

                // 以上全ての項目にカスタムセルを設定。
                SetCustumCells(unyuuMenuView, SCFoundation.UNYUU);
            }

            public void ReconstructMenu(int type)
            {
                // メニューを表示している間に再構成しないこと。誤動作します。
                if (type == SCFoundation.HONTAI)
                {
                    // おすすめ
                    {
                        String title = session.GetStringFromShiori("sakura.recommendbuttoncaption");
                        // 取得出来なければ「%selfnameのおすすめ」とする。
                        if (title == null || title.Length == 0)
                        {
                            title = SCStringsServer.GetStrFromMainDic("contextmenu.close", new String[] { session.GetSelfName() });
                        }
                        sakura_recommended.SetTitle(title);
                        setTable(sakura_recommended, "sakura.recommendsites", type);
                    }

                    // ポータル
                    {
                        String title = session.GetStringFromShiori("sakura.portalbuttoncaption");
                        // 取得出来なければ「ポータル」とする。
                        if (title == null || title.Length == 0)
                        {
                            title = SCStringsServer.GetStrFromMainDic("contextmenu.portal");
                        }
                        portal.SetTitle(title);
                        setTable(portal, "sakura.portalsites", type);
                    }

                    // ゴースト変更
                    {
                        if (session.IsStatusClosing())
                        {
                            change_ghost.SetEnabled(false);
                        }
                        else
                        {
                            NSMenu submenu = SCGhostManager.SharedGhostManager().MakeGhostMenu(session);
                            SCContextMenuView submenuView = new SCContextMenuView(session, type);
                            submenu.SetMenuRepresentation(submenuView);
                            change_ghost.SetSubmenu(submenu);

                            SetCustumCells(submenuView, SCFoundation.HONTAI);
                        }
                    }

                    // シェル
                    {
                        if (session.IsShellChangingSessionRunningNow() ||
                            session.IsStatusClosing() ||
                            session.IsNetworkUpdaterRunningNow())
                        {
                            change_shell.SetEnabled(false);
                        }
                        else
                        {
                            NSMenu submenu = SCGhostManager.SharedGhostManager().MakeShellMenu(session);
                            SCContextMenuView submenuView = new SCContextMenuView(session, SCFoundation.HONTAI);
                            submenu.SetMenuRepresentation(submenuView);
                            change_shell.SetSubmenu(submenu);

                            SetCustumCells(submenuView, SCFoundation.HONTAI);

                            change_shell.SetEnabled(true);
                        }
                    }

                    // バルーン
                    {
                        NSMenu submenu = SCGhostManager.SharedGhostManager().MakeBalloonMenu(session);
                        SCContextMenuView submenuView = new SCContextMenuView(session, type);
                        submenu.SetMenuRepresentation(submenuView);
                        change_balloon.SetSubmenu(submenu);

                        SetCustumCells(submenuView, SCFoundation.HONTAI);
                    }

                    // 着せ替え
                    /*NSMenuItem item_kisekae_submenu = new NSMenuItem("\u7740\u305b\u66ff\u3048",null,"");
                    NSMenu kisekae_submenu = session.GetCurrentShell().GetSeriko().GetBindCenter().GetBindMenu();
                    if (kisekae_submenu == null)
                    {
                        item_kisekae_submenu.SetEnabled(false);
                    }
                    else
                    {
                        item_kisekae_submenu.SetSubmenu(kisekae_submenu);
                    }
                    hontaiMenu.AddItem(item_kisekae_submenu);*/

                    // ネットワーク更新
                    {
                        String title = session.GetStringFromShiori("updatebuttoncaption");
                        if (title == null || title.Length == 0)
                        {
                            title = SCStringsServer.GetStrFromMainDic("contextmenu.Networkupdate");
                        }
                        network_update.SetTitle(title);

                        if (session.GetStringFromShiori("homeurl") != null &&
                            !session.IsNetworkUpdaterRunningNow() &&
                            !session.IsStatusClosing())
                        {
                            network_update.SetEnabled(true);
                        }
                        else
                        {
                            network_update.SetEnabled(false);
                        }
                    }

                    // 消滅通告
                    if (vanish != null)
                    {
                        String title = session.GetStringFromShiori("vanishbuttoncaption");
                        if (title == null || title.Length == 0)
                        {
                            title = SCStringsServer.GetStrFromMainDic("contextmenu.vanish");
                        }
                        vanish.SetTitle(title);

                        if (!session.IsNetworkUpdaterRunningNow() &&
                            !session.IsStatusClosing())
                        {
                            vanish.SetEnabled(true);
                        }
                        else
                        {
                            vanish.SetEnabled(false);
                        }
                    }

                    // ヘッドラインセンサ
                    {
                        setHeadlineSensorTable(headline_sensor, type);
                    }

                    // このシェルについて...
                    {
                        about_shell.SetEnabled(session.GetCurrentShell().GetReadmeFile().Exists());
                    }

                    // 消す
                    {
                        if (!session.IsStatusClosing() &&
                            !session.IsNetworkUpdaterRunningNow())
                        {
                            disappear.SetEnabled(true);
                        }
                        else
                        {
                            disappear.SetEnabled(false);
                        }
                    }
                }
                else if (type == SCFoundation.UNYUU)
                {
                    // うにゅうのおすすめ
                    {
                        String title = session.GetStringFromShiori("kero.recommendbuttoncaption");
                        // 取得出来なければ「%keronameのおすすめ」とする。
                        if (title == null)
                        {
                            title = SCStringsServer.GetStrFromMainDic("contextmenu.close", new String[] { session.GetKeroName() });
                        }
                        unyuu_recommended.SetTitle(title);
                        setTable(unyuu_recommended, "kero.recommendsites", type);
                    }
                }
            }

            protected void SetCustumCells(NSMenuView menuView, int type)
            {
                NSMenu menu = menuView.Menu();
                int n_items = menu.NumberOfItems();
                for (int i = 0; i < n_items; i++)
                {
                    SCContextMenuItemCell mic = new SCContextMenuItemCell(session, type);
                    mic.SetMenuItem(menu.ItemAtIndex(i));
                    menuView.SetMenuItemCellForItemAtIndex(mic, i);
                }
            }

            protected void AddSeparator(NSMenu menu)
            {
                NSMenuItem item = (NSMenuItem)NSMenuItem.ProtocolSeparatorItem();
                hontaiMenu.AddItem(item);
            }

            private void setTable(NSMenuItem target, String resourceType, int type)
            {
                String rcmTable = session.GetStringFromShiori(resourceType);
                // 取得出来なければdisable。
                if (rcmTable == null || rcmTable.Length == 0)
                {
                    target.SetEnabled(false);
                }
                else
                {
                    try
                    {
                        NSMenu submenu = new NSMenu();
                        submenu.SetAutoenablesItems(false);
                        SCContextMenuView submenuView = new SCContextMenuView(session, type);
                        submenu.SetMenuRepresentation(submenuView);

                        // まずは0x02で分割
                        StringTokenizer rows = new StringTokenizer(rcmTable, "\u0002");
                        while (rows.HasMoreTokens())
                        {
                            // 次に0x01で分割
                            StringTokenizer columns = new StringTokenizer(rows.NextToken(), "\u0001");

                            String name = columns.NextToken();
                            if (name.Equals("-"))
                            {
                                submenu.AddItem((NSMenuItem)NSMenuItem.ProtocolSeparatorItem());
                            }
                            else
                            {
                                String url = columns.NextToken();
                                String banner = (columns.HasMoreTokens() ? columns.NextToken() : null);
                                URLMenuItem item = new URLMenuItem(name, url, banner);
                                item.SetAction(selector_urlMenuItemSelected);
                                item.SetTarget(this);
                                item.SetImage(SCFoundation.SharedFoundation().GetBannerServer().GetBanner(banner));
                                submenu.AddItem(item);
                                SCContextMenuItemCell mic = new SCContextMenuItemCell(session, type, SCContextMenuItemCell.ALIGN_RIGHT);
                                mic.SetMenuItem(item);
                                submenuView.SetMenuItemCellForItemAtIndex(mic, submenu.NumberOfItems() - 1);
                            }
                        }

                        target.SetSubmenu(submenu);
                    }
                    catch (Exception e)
                    {
                        target.SetEnabled(false);
                    }
                }
            }

            private void setHeadlineSensorTable(NSMenuItem target, int type)
            {
                NSMenu submenu = new NSMenu();
                submenu.SetAutoenablesItems(false);
                SCContextMenuView submenuView = new SCContextMenuView(session, type);
                submenu.SetMenuRepresentation(submenuView);

                SCPlugin[] sensors = SCFoundation.SharedFoundation().GetPluginManager().GetPlugins("hlsensor");
                if (sensors.Length == 0)
                {
                    target.SetEnabled(false);
                }
                else
                {
                    target.SetEnabled(true);

                    for (int i = 0; i < sensors.Length; i++)
                    {
                        SCHLSensorPlugin sensor = (SCHLSensorPlugin)sensors[i];

                        SCHLSensorResponse resp = sensor.Request("GET Description HEADLINE/1.0\r\nSender: " + SCFoundation.STRING_FOR_SENDER + "\r\n\r\n");
                        String title = resp.GetHeader("Name");

                        HLSensorMenuItem item = new HLSensorMenuItem(title, sensor);
                        item.SetAction(selector_hlsensorMenuItemSelected);
                        item.SetTarget(this);
                        submenu.AddItem(item);
                        SCContextMenuItemCell mic = new SCContextMenuItemCell(session, type, SCContextMenuItemCell.ALIGN_RIGHT);
                        mic.SetMenuItem(item);
                        submenuView.SetMenuItemCellForItemAtIndex(mic, submenu.NumberOfItems() - 1);
                    }

                    target.SetSubmenu(submenu);
                }
            }

            protected NSMenu MakeDevInterfacesSubMenu()
            {
                NSMenu submenu = new NSMenu();
                submenu.SetAutoenablesItems(false);

                URLMenuItem item;

                // SHIORI/MAKOTOをリロード
                item = new URLMenuItem(SCStringsServer.GetStrFromMainDic("contextmenu.development.reload"), null, "");
                item.SetAction(new NSSelector("reloadShioriAndMakoto", new object()));
                item.SetTarget(this);
                submenu.AddItem(item);

                return submenu;
            }
            public void ReloadShioriAndMakoto(Object sender)
            {
                session.GetMasterSpirit().ReloadAll();
            }

            public NSMenu GetMenu(int type)
            {
                // 一度も再構成せずに呼び出すとnullが返されます。
                return (type == SCFoundation.HONTAI ? hontaiMenu : unyuuMenu);
            }

            public void NetworkUpdateSelected(Object sender)
            {
                session.DoNetworkUpdate();
            }

            public void CloseSelected(Object sender)
            {
                SCGhostManager.SharedGhostManager().CloseGhost(session);
            }

            public void VanishSelected(Object sender)
            {
                session.OpenVanishDialog();
            }

            public void UrlMenuItemSelected(Object sender)
            {
                if (!(sender is URLMenuItem))
                {
                    return;
                }
                URLMenuItem mItem = (URLMenuItem)sender;
                // try { NSWorkspace.SharedWorkspace().openURL(new URL(mItem.GetSiteUrl())); } catch (Exception e) { }
            }

            public void HlsensorMenuItemSelected(Object sender)
            {
                if (!(sender is HLSensorMenuItem))
                {
                    return;
                }
                // session.doHLSensing(((HLSensorMenuItem)sender).GetPlugin());
            }

            public void AboutThisGhost(Object sender)
            {
                // NSWorkspace.SharedWorkspace().openFile(session.GetReadmeFile().GetPath(), null, true);
                //new SCDocumentWindowController(session.GetReadmeFile(),SCStringsServer.GetStrFromMainDic("contextmenu.aboutghost")).ShowWindow(null);
            }

            public void aboutThisShell(Object sender)
            {
                // NSWorkspace.SharedWorkspace().openFile(session.GetCurrentShell().GetReadmeFile().GetPath(), null, true);
                //new SCDocumentWindowController(session.GetCurrentShell().GetReadmeFile(),SCStringsServer.GetStrFromMainDic("contextmenu.aboutshell")).ShowWindow(null);
            }

            private class URLMenuItem : NSMenuItem
            {
                String site_name;
                String site_url;
                String banner_url;


                public URLMenuItem(String site_name, String site_url, String banner_url) : base(site_name, null, "")
                {
                    //super(site_name, null, "");

                    this.site_name = site_name;
                    this.site_url = site_url;
                    this.banner_url = banner_url;
                }

                public String getSiteName() { return site_name; }
                public String getSiteUrl() { return site_url; }
                public String getBannerUrl() { return banner_url; }

                internal void SetImage(object value)
                {
                    throw new NotImplementedException();
                }
            }

            private class HLSensorMenuItem : NSMenuItem
            {
                String site_name;
                SCHLSensorPlugin plugin;


                public HLSensorMenuItem(String site_name, SCHLSensorPlugin plugin) : base(site_name, null, "")
                {
                    // super(site_name, null, "");

                    this.site_name = site_name;
                    this.plugin = plugin;
                }

                public String getSiteName() { return site_name; }
                public SCHLSensorPlugin getPlugin() { return plugin; }
            }
        }
    }
}