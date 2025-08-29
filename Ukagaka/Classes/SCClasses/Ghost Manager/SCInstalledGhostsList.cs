using Cocoa.AppKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using Ukagaka;
using System.Windows.Documents;
using Utils;
using static System.Collections.Specialized.BitVector32;
using System.Windows.Markup;
using System.Security.Cryptography.X509Certificates;
using static Ukagaka.SCInstalledGhostsList;
using System.Windows.Media.Imaging;
namespace Ukagaka
{
    public class SCInstalledGhostsList : List<ListsElement>
    {
        public static int DETERMINATOR_WAIT = 300; // Determinatorスレッドが一件を讀む毎に待つミリ秒。

        private static NSColor BOOTED_COLOR = NSColor.ColorWithCalibratedRGB(1.0f, 0.0f, 0.0f, 1.0f);
        private static NSSelector selector_changeGhostTo = new NSSelector("ChangeGhostTo", new object());
        public static SCInstalledGhostsList Shared;


        List<ListsElement> list; // 中身はSCInstalledGhostsList.ListsElement。
        List<ListsElement> sublist; // 中身はlistと同じだが、こちらは絞り込み検索がかかっている可能性がある。
        SCGhostManager ghostManager;
        Determiner last_started_determiner; // 最後に起動したDeterminer。起動した事が無ければnull。





        public SCInstalledGhostsList(SCGhostManager ghostManager)
        {
            Shared = this;
            this.ghostManager = ghostManager;
            this.last_started_determiner = null;

            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            System.Windows.Controls.ListView tableView = windowController.GetTableInstalled();

            if (tableView != null)
            {
                tableView.ItemsSource = this;

                //tableView.SetDataSource(this);
                //tableView.SetDelegate(this);
            }
            list = new List<ListsElement>();
            sublist = new List<ListsElement>();
            ReloadList();

            TableViewSelectionDidChange(null);

            NSNotificationCenter.DefaultCenter().AddObserver(
                this,
                new NSSelector("onShellChangingBeginAndEnd", new NSNotification()),
                "shellchanger",
                null);
            NSNotificationCenter.DefaultCenter().AddObserver(
                this,
                new NSSelector("onNetworkUpdaterBeginAndEnd", new NSNotification()),
                    "networkupdater",
                    null);
            NSNotificationCenter.DefaultCenter().AddObserver(
                this,
                new NSSelector("onSessionClosingAndOpeningBeginAndEnd", new NSNotification()),
                    "sessionstarter",
                    null);
            NSNotificationCenter.DefaultCenter().AddObserver(
                this,
                new NSSelector("onSessionClosingAndOpeningBeginAndEnd", new NSNotification()),
                    "sessionchanger",
                    null);
            NSNotificationCenter.DefaultCenter().AddObserver(
                this,
                new NSSelector("onSessionClosingAndOpeningBeginAndEnd", new NSNotification()),
                    "sessioncloser",
                    null);
            NSNotificationCenter.DefaultCenter().AddObserver(
                this,
                new NSSelector("onSessionClosingAndOpeningBeginAndEnd", new NSNotification()),
                    "vanisher",
                    null);
            NSNotificationCenter.DefaultCenter().AddObserver(
                this,
                new NSSelector("onSessionEnteringAndLeavingPassiveMode", new NSNotification()),
                    "passivemode",
                    null);
            Shared = this;
        }

        public void ReloadList()
        {
            // Determinerが動作中なら止める。
            if (last_started_determiner != null && last_started_determiner.IsAlive())
            {
                last_started_determiner.TerminateAndWait();
            }

            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            windowController.GetTableInstalled()?.UnselectAll();
            list.Clear();

            String bundleDir = SCFoundation.GetParentDirOfBundle();
            File ghostDir;
            File[] items;

            ghostDir = new File(bundleDir, "home/ghost");
            items = ghostDir.ListFiles();
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].IsDirectory())
                {
                    File ghost_dir = new File(items[i], "ghost");
                    if (!ghost_dir.Exists())
                    {
                        continue; // ghostディレクトリが無ければスキップ。
                    }
                    String path_from_home = "home/ghost/" + items[i].GetName();
                    NSDictionary ghostDefaults = ghostManager.GetGhostDefaults(path_from_home);

                    Object boot_flag_entry = ghostDefaults.ObjectForKey("boot_flag");
                    bool boot_flag =
                (boot_flag_entry != null &&
                 (boot_flag_entry is long ?

              ((long)boot_flag_entry) :
              ((Integer)boot_flag_entry).IntValue()) == 1 ? true : false);

                    String balloon = (String)ghostDefaults.ObjectForKey("balloon");
                    if (balloon == null)
                    {
                        balloon = SCFoundation.FIXED_BALLOON_TO_BOOT;
                    }
                    object scale_entry = ghostDefaults.ObjectForKey("scale");
                    double scale = (scale_entry is double ? (double)scale_entry : 1.0);

                    String shell_dirname = (String)ghostDefaults.ObjectForKey("shell_dirname");
                    if (shell_dirname == null)
                    {
                        shell_dirname = "master";
                    }
                    ListsElement le = new ListsElement(boot_flag, path_from_home, balloon, shell_dirname, scale);
                    list.Add(le);
                }
            }

            FindBoxUpdated();

            // Determinerを起動する。
            if (!SCFoundation.DO_NOT_DETERMINE_GHOST_DETAIL)
            {
                last_started_determiner = new Determiner(list);
                last_started_determiner.Start();
            }
        }

        private class ListsElementComparer : IComparer<ListsElement>
        {
            private readonly string key;
            private readonly string order;

            public ListsElementComparer(string key, string order)
            {
                this.key = key;
                this.order = order;
            }

            public int Compare(ListsElement le1, ListsElement le2)
            {
                string a, b;
                if (key == null || key.Equals("name"))
                {
                    a = le1.GetName();
                    b = le2.GetName();
                }
                else
                {
                    a = le1.GetMasterShioriKernelName() ?? "";
                    b = le2.GetMasterShioriKernelName() ?? "";
                }

                int sign = order == null || order.Equals("asc") ? 1 : -1;
                return string.Compare(a, b) * sign;
            }
        }
        public void SortList()
        {
            // defaults内に記録された順序でゴーストリストをソートする。
            NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults();

            String key = (String)defaults.ObjectForKey("ghostmanager.ghostlist.sort.key");
            String order = (String)defaults.ObjectForKey("ghostmanager.ghostlist.sort.order");

            sublist.Sort(new ListsElementComparer(key, order));



            //ghostManager.GetWindowController().GetTableInstalled().ReloadData();
        }









        public class Determiner
        {
            // バックグラウンドで少しずつ各ゴーストのSHIORIとMAKOTOを判別するスレッド。
            List<ListsElement> list;
            volatile bool flag_term;
            Thread thread;

            public Determiner(List<ListsElement> list)
            {
                this.list = list;
                thread = new Thread(Run);
            }

            public void Start()
            {
                thread.Start();
            }

            public bool IsAlive()
            {
                return thread.IsAlive; 
            }


            public void Sleep(int value)
            {
                Thread.Sleep(value);
            }
            public void Run()
            {
                //int pool = NSAutoreleasePool.push();

                //this.SetPriority(Thread.MIN_PRIORITY);
                flag_term = false;

                String bundleDir = SCFoundation.GetParentDirOfBundle();
                for (int i = 0; !flag_term && i < list.Count; i++)
                {
                    if (i > 0)
                    {
                        try
                        {
                            Sleep(SCInstalledGhostsList.DETERMINATOR_WAIT);
                        }
                        catch (Exception e) { }
                    }

                    ListsElement le = (ListsElement)list.ElementAt(i);
                    File master_ghost_dir = new File(bundleDir, le.GetPath() + "/ghost/master");

                    String shiori_name = SCShioriLoader.GetModuleName(master_ghost_dir);
                    if (shiori_name == null)
                    {
                        shiori_name = "";
                    }
                    le.SetMasterShioriKernelName(shiori_name);

                    ArrayList  makoto_names = SCMakotoLoader.GetModuleNames(master_ghost_dir);
                    le.SetMakotoNames(makoto_names);

                    if (i % 3 == 0)
                    {
                        // テーブルを更新
                        SCInstalledGhostsList.Shared.SortList();
                    }
                }

                // テーブルを更新
                SCInstalledGhostsList.Shared.SortList();

                // テーブル以外の部分を更新
                SCInstalledGhostsList.Shared.TableViewSelectionDidChange(null);

                // NSAutoreleasePool.pop(pool);
            }

            public void TerminateAndWait()
            {
                flag_term = true;
                while (IsAlive())
                {
                    try
                    {
                        Sleep(150);
                    }
                    catch (Exception e) { }
                }
            }
        }

        public void FindBoxUpdated()
        {
            // find_boxが空だった場合は、単にlistをsublistにコピー。
            // 空でなかった場合、ゴースト名に部分一致でfind_boxの内容を含んでいるものだけをsublistへ。
            sublist.Clear();

            String to_find = ghostManager.GetWindowController().GetFindBox().Text;
            if (to_find.Length == 0)
            {
                sublist.AddRange(list);
            }
            else
            {
                foreach (var item in list)
                {
                    ListsElement le = (ListsElement)item;

                    if (le.GetName().IndexOf(to_find) != -1)
                    {
                        sublist.Add(le);
                    }
                }
            }

            SortList();
        }

        public class ListsElement
        {
            // あくまでSCInstalledGhostsListの内部クラスです。他のクラス内にある同名の物とは関係ありません。
            bool boot_flag;
            String name;
            String path;
            String balloon_path;
            double scale;
            String masterShioriKernelName; // これと
            ArrayList makotoNames; // これがnullなら、未判別である。
            String identification;
            String shell_dirname; // 最後に選ばれたシェルのディレクトリ名。
            String selfname;
            String keroname;

            bool thumb_checked; // サムネイルの有無を未判定ならfalse。
            File thumb_file; // png又はpnr。無ければnull。

            // IDがghost/masterのものと異なるシェル一覧。
            bool indep_shells_checked; // 有無を未判定ならfalse。
            Hashtable independent_shells; // {ID => シェルディレクトリ名} 無ければnull

            public ListsElement(
            bool boot_flag, String path, String balloon_path,
            String shell_dirname, double scale)
            {
                // pathは、そのシェル又はゴーストのルートディレクトリです。
                this.boot_flag = boot_flag;
                this.path = path; // home/ghost/[ghostname]
                this.balloon_path = balloon_path;
                this.scale = scale;
                this.shell_dirname = shell_dirname;

                String bundleDir = SCFoundation.GetParentDirOfBundle();
                // 無駄な処理ではあるが、専用のコードは面倒で書きたくない。
                SCDescription descm = new SCDescription(new File(bundleDir, path + "/ghost/master/descript.txt"));
                name = descm.GetStrValue("name");
                selfname = descm.GetStrValue("sakura.name");
                keroname = descm.GetStrValue("kero.name");
                if (name == null)
                {
                    if (selfname != null)
                    {
                        if (keroname != null)
                        {
                            name = selfname + ',' + keroname;
                        }
                        else
                        {
                            name = selfname;
                        }
                    }
                    else
                    {
                        name = SCStringsServer.GetStrFromMainDic("ghostmanager.ghostname.undefined");
                    }
                }
            }

            public bool GetBootFlag()
            {
                return boot_flag;
            }

            public void SetBootFlag(bool value)
            {
                boot_flag = value;
            }

            public String GetName()
            {
                return name;
            }

            public String GetPath()
            {
                return path;
            }

            public String GetBalloonPath()
            {
                return balloon_path;
            }

            public void SetBalloonPath(String p)
            {
                balloon_path = p;
            }

            public double GetScale()
            {
                return scale;
            }

            public void SetScale(double scale)
            {
                this.scale = scale;
            }

            public String GetMasterShioriKernelName()
            {
                return masterShioriKernelName;
            }

            public void SetMasterShioriKernelName(String name)
            {
                masterShioriKernelName = name;
            }

            public ArrayList GetMakotoNames()
            {
                return makotoNames;
            }

            public void SetMakotoNames(ArrayList names)
            {
                makotoNames = names;
            }

            public String Make_identification(String sakura, String kero)
            {
                // これは本來staticだが、内部クラスなのでstaticに出來ない。
                StringBuffer buf = new StringBuffer(30);
                if (sakura != null)
                {
                    buf.Append(sakura);
                }
                buf.Append(',');
                if (kero != null)
                {
                    buf.Append(kero);
                }
                return buf.ToString();
            }

            public String GetIdentification()
            {
                // 必要になるまで作らない。
                if (identification == null)
                {
                    identification = Make_identification(selfname, keroname);
                }
                return identification;
            }

            public String GetShellDirName()
            {
                return shell_dirname;
            }

            public void SetShellDirName(String str)
            {
                shell_dirname = str;
            }

            public File GetThumbnailFile()
            {
                if (!thumb_checked)
                {
                    // サムネイルの有無を未判定。
                    String bundleDir =
                        SCFoundation.GetParentDirOfBundle();
                    File f = new File(bundleDir, path + "/thumbnail.png");
                    if (f.Exists())
                    {
                        thumb_file = f;
                    }
                    else
                    {
                        f = new File(bundleDir, path + "/thumbnail.pnr");
                        if (f.Exists())
                        {
                            thumb_file = f;
                        }
                    }
                    thumb_checked = true;
                }

                return thumb_file;
            }

            public String GetSelfName()
            {
                return selfname;
            }

            public String GetKeroName()
            {
                return keroname;
            }

            public Hashtable GetIndependentShells()
            {
                if (!indep_shells_checked)
                {
                    // ghost/masterとIDが違うシェルを探す。
                    File shell_dir = new File(
                        SCFoundation.GetParentDirOfBundle(),
                        path + "/shell");
                    File[] shells = shell_dir.ListFiles();
                    for (int i = 0; i < shells.Length; i++)
                    {
                        if (!shells[i].IsDirectory())
                        {
                            continue;
                        }
                        File desc_f = new File(shells[i], "descript.txt");
                        if (!desc_f.Exists())
                        {
                            continue;
                        }
                        SCDescription desc = new SCDescription(desc_f);
                        String sakura = desc.GetStrValue("sakura.name");
                        String kero = desc.GetStrValue("kero.name");
                        if ((sakura != null && !sakura.Equals(selfname)) ||
                        (kero != null && !kero.Equals(keroname)))
                        {
                            // 別の名前を持つてゐる。
                            if (independent_shells == null)
                            {
                                independent_shells = new Hashtable();
                                 
                            }
                            independent_shells[Make_identification(sakura, kero)] = shells[i].GetName();
                                
                        }
                    }
                    indep_shells_checked = true;
                }
                return independent_shells;
            }

            public String search_id_in_independent_shells(String id)
            {
                // 指定されたIDを持つ獨立シェルを探し、見付かればそのシェルディレクトリ名を返す。
                // IDは兩方の指定でも、sakura側のみの指定でも良い。
                if (!indep_shells_checked)
                {
                    GetIndependentShells();
                }
                if (independent_shells == null)
                {
                    return null;
                }

                bool both = (name.IndexOf(',') != -1);
                foreach (string key in independent_shells.Keys)
                {
                    String shell_id = (String)key;
                    if (both)
                    {
                        if (shell_id.Equals(id))
                        {
                            return (String)independent_shells[shell_id];
                        }
                    }
                    else
                    {
                        if (shell_id.StartsWith(id))
                        {
                            return (String)independent_shells[shell_id];
                        }
                    }
                }
                return null;
            }
        }

        public NSMenu MakeGhostMenu(SCSession self)
        {
            NSMenu submenu = new NSMenu();
            submenu.SetAutoenablesItems(false);

            int n_ghosts = list.Count();
            for (int i = 0; i < n_ghosts; i++)
            {
                ListsElement le = (ListsElement)list.ElementAt(i);
                SCSession sessionIfRunning = SCFoundation.SharedFoundation().GetSession(le.GetPath());

                GhostMenuItem item = new GhostMenuItem(self, le.GetName(), le.GetPath(), le.GetBalloonPath(), le.GetSelfName(), le.GetScale());
                item.SetAction(selector_changeGhostTo);
                item.SetTarget(this);
                item.SetState(self.GetGhostPath().Equals(le.GetPath()) ? NSCell.OnState : NSCell.OffState);
                item.SetEnabled(sessionIfRunning == null || self.GetGhostPath().Equals(le.GetPath()));

                submenu.AddItem(item);
            }

            return submenu;
        }

        public List<ListsElement> GetInstalledGhostList()
        {
            return list; // 内容を変更してはならない。
        }

        public void ChangeGhostTo(Object sender)
        {
            // メニューから呼ばれるActionです。
            if (!(sender is GhostMenuItem)) 
            {
                return;
            }
            GhostMenuItem gmi = (GhostMenuItem)sender;

            SCSessionChanger sc = new SCSessionChanger(gmi.GetSelf(), gmi.GetPath(), gmi.GetBalloonPath(), gmi.GetSelfName(), gmi.GetScale());
            sc.Start();
        }

        public void ChangeBalloonOfCurrentGhost(String balloon_dir)
        {
            // balloon_dirは、home/balloon/から始まるバルーンのパス。
            // 現在リストで選択されているゴーストのバルーンを変える。
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            int selected_row = windowController.GetTableInstalled().SelectedIndex;
            if (selected_row == -1) return;

            ListsElement elem = (ListsElement)sublist.ElementAt(selected_row);
            ChangeBalloonOfGhost(elem.GetPath(), balloon_dir);
        }

        public void ChangeShellOfCurrentGhost(String shell_dirname)
        {
            // 現在リストで選択されているゴーストのシェルを変える。
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            int selected_row = windowController.GetTableInstalled().SelectedIndex;
            if (selected_row == -1) return;

            ListsElement elem = (ListsElement)sublist.ElementAt(selected_row);
            ChangeShellOfGhost(elem.GetPath(), shell_dirname);
        }

        private ListsElement FindGhost(String ghost_path)
        {
            int n_elems = list.Count;
            for (int i = 0; i < n_elems; i++)
            {
                ListsElement elem = (ListsElement)list.ElementAt(i);
                if (elem.GetPath().Equals(ghost_path)) return elem;
            }

            return null;
        }

        public void ChangeShellOfGhost(String ghost_path, String shell_dirname)
        {
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            int selected_row = windowController.GetTableInstalled().SelectedIndex;
            if (selected_row != -1)
            {
                ListsElement list = (ListsElement)sublist.ElementAt(selected_row);
                if (list != null && list.GetPath().Equals(ghost_path))
                {
                    ghostManager.GetShellsList().SelectShell(shell_dirname);
                }
            }

            ListsElement le = FindGhost(ghost_path);
            if (le != null)
            {
                le.SetShellDirName(shell_dirname);
            }
            NSMutableDictionary ghostDefaults = ghostManager.GetGhostDefaults(ghost_path);
            ghostDefaults.SetObjectForKey(shell_dirname, "shell_dirname");
            ghostManager.SetGhostDefaults(ghost_path, ghostDefaults);

            // このゴーストが既に起動していたら、即座にシェルの切り替えを行う。
            SCSession session = SCFoundation.SharedFoundation().GetSession(ghost_path);
            if (session != null)
            {
                session.ChangeShell();
            }
            session = SCFoundation.SharedFoundation().
                GetTemporarySession(le.GetIdentification(), false);
            if (session != null)
            {
                session.ChangeShell();
            }

            TableViewSelectionDidChange(null); // プレビューを更新する。
        }

        public void ChangeBalloonOfGhost(String ghost_path, String balloon_dir)
        {
            // balloon_dirは、home/balloon/から始まるバルーンのパス。
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            int selected_row = windowController.GetTableInstalled().SelectedIndex;
            if (selected_row != -1)
            {
                ListsElement le = (ListsElement)sublist.ElementAt(selected_row);
                if (le != null && le.GetPath().Equals(ghost_path))
                {
                    ghostManager.GetBalloonsList().SelectBalloon(balloon_dir);
                }
            }

            ListsElement elem = FindGhost(ghost_path);
            if (elem != null) elem.SetBalloonPath(balloon_dir);

            NSMutableDictionary ghostDefaults = ghostManager.GetGhostDefaults(ghost_path);
            ghostDefaults.SetObjectForKey(balloon_dir, "balloon");
            ghostManager.SetGhostDefaults(ghost_path, ghostDefaults);

            // このゴーストが既に起動していたら、即座にバルーンの切り替えを行う。
            SCSession session = SCFoundation.SharedFoundation().GetSession(ghost_path);
            if (session != null)
            {
                // サブスレッドからRestartBalloonSkinServerを呼ぶとステータスウインドウが出る。
                new BalloonSkinChangerThread(session, balloon_dir).Start();
            }
            session = SCFoundation.SharedFoundation().
                GetTemporarySession(elem.GetIdentification(), false);
            if (session != null)
            {
                session.RestartBalloonSkinServer(balloon_dir); // ステータスウインドウなど要らない。
            }
        }
        protected class BalloonSkinChangerThread
        {
            SCSession session;
            String balloon_dir;
            Thread thread;
            public BalloonSkinChangerThread(SCSession session, String balloon_dir)
            {
                this.session = session;
                this.balloon_dir = balloon_dir;
                this.thread = new Thread(Run);
            }
            public void Run()
            {
                // int pool = NSAutoreleasePool.push();
                session.RestartBalloonSkinServer(balloon_dir);
               // NSAutoreleasePool.pop(pool);
            }

            public void Start()
            {
                thread.Start();
            }

           

        }

        public void ChangeScale(double scale)
        {
            // 現在リストで選択されているゴーストのスケールを変える。

            if (scale == 0)
            {
                // 0にすると戻らなくなる恐れがあるので、しない。
                return;
            }

            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            int selected_row = windowController.GetTableInstalled().SelectedIndex;
            if (selected_row == -1) return;

            ListsElement elem = (ListsElement)sublist.ElementAt(selected_row);
            elem.SetScale(scale);

            // defaultsに保存
            NSMutableDictionary ghostDefaults = ghostManager.GetGhostDefaults(elem.GetPath());
            ghostDefaults.SetObjectForKey(scale, "scale");
            ghostManager.SetGhostDefaults(elem.GetPath(), ghostDefaults);

            // そのゴーストが起動していたら、ここでスケール変更。
            SCSession session = SCFoundation.SharedFoundation().GetSession(elem.GetPath());
            if (session != null)
            {
                session.ResizeShell(scale);
            }
        }

        public void NetworkUpdate()
        {
            // 現在リストで選択されているゴーストをネットワーク更新。
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            int selected_row = windowController.GetTableInstalled().SelectedIndex;
            if (selected_row == -1) return;

            ListsElement elem = (ListsElement)sublist.ElementAt(selected_row);

            SCSession session = SCFoundation.SharedFoundation().GetSession(elem.GetPath());
            if (session == null) return;

            session.DoNetworkUpdate();
        }

        public void BootOrQuit()
        {
            // リストで選択されているゴーストの起動/終了をトグル
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            int selected_row = windowController.GetTableInstalled().SelectedIndex;
            if (selected_row == -1) return;

            ListsElement elem = (ListsElement)sublist.ElementAt(selected_row);

            SCSession session = SCFoundation.SharedFoundation().GetSession(elem.GetPath());
            if (session == null)
            {
                // 起動
                elem.SetBootFlag(true);
                SCSessionStarter.SharedStarter().Start(elem.GetPath(), elem.GetBalloonPath(), elem.GetScale());
            }
            else
            {
                // 終了
                elem.SetBootFlag(false);
                session.PerformClose();
            }
            ghostManager.SetBootFlagInDefaults(elem.GetPath(), elem.GetBootFlag());

            //windowController.GetTableInstalled().ReloadData();
            TableViewSelectionDidChange(null);
        }

        public void Vanish()
        {
            // リストで選択されているゴーストのVANISHウインドウを開く
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            int selected_row = windowController.GetTableInstalled().SelectedIndex;
            if (selected_row == -1) return;

            ListsElement elem = (ListsElement)sublist.ElementAt(selected_row);

            SCSession session = SCFoundation.SharedFoundation().GetSession(elem.GetPath());
            if (session == null) return;

            session.OpenVanishDialog();
        }

        public void SetBootFlag(String ghost_path, bool value)
        {
            // メモリ上のデータのbootフラグを変化させるだけです。
            // 他には何の動作も行いません。
            ListsElement le = FindGhost(ghost_path);
            if (le == null) return;

            le.SetBootFlag(value);
        }

        public void BootAllGhostsToBoot()
        {
            // ブートフラグが立っている全てのゴーストをブートさせます。
            // 起動時に一度だけ読んでください。
            // 二回も三回も呼ぶとどうなるかは．．．お察しください。
            SCFoundation foundation = SCFoundation.SharedFoundation();
            int n_shells = list.Count();
            bool isBoot = false;
            for (int i = 0; i < n_shells; i++)
            {
                ListsElement elem = (ListsElement)list.ElementAt(i);

                if (elem.GetBootFlag())
                {
                    SCSessionStarter.SharedStarter().Start(elem.GetPath(), elem.GetBalloonPath(), elem.GetScale());
                    isBoot = true;
                }
                  
            }

            if (!isBoot)
            {
                ListsElement elem = (ListsElement)list.ElementAt(list.Count - 1);

                if (elem != null)
                {
                    SCSessionStarter.SharedStarter().Start(elem.GetPath(), elem.GetBalloonPath(), elem.GetScale());
                }
                 
            }

        }

        public void ClearBootFlags()
        {
            foreach (ListsElement elem in list)
            { 
                if (elem.GetBootFlag())
                {
                    elem.SetBootFlag(false);
                    ghostManager.SetBootFlagInDefaults(elem.GetPath(), elem.GetBootFlag());
                }
            }
        }

        public ArrayList GetAllGhostPathsInAsleep()
        {
            // ブートフラグが立っていない全てのゴーストのパスを返します。
            // 一つも無ければ空のVectorを返します。
            ArrayList result = new ArrayList();
            int n_shells = list.Count;
            for (int i = 0; i < n_shells; i++)
            {
                ListsElement elem = (ListsElement)list.ElementAt(i);

                if (!elem.GetBootFlag())
                {
                    result.Add(elem.GetPath());
                }
            }
            return result;
        }

        public String GetBalloonPathOfGhost(String ghostPath)
        {
            ListsElement le = FindGhost(ghostPath);
            if (le == null) return "";

            return (le.GetBalloonPath() == null ? "" : le.GetBalloonPath());
        }

        public double GetScaleOfGhost(String ghostPath)
        {
            ListsElement le = FindGhost(ghostPath);
            if (le == null) return 1.0;

            return le.GetScale();
        }

        public String GetNameOfGhost(String ghostPath)
        {
            ListsElement le = FindGhost(ghostPath);
            if (le == null) return "";

            return le.GetName();
        }

        // Notification Callback
        public void onShellChangingBeginAndEnd(NSNotification notification)
        {
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            int selected_row = windowController.GetTableInstalled().SelectedIndex;
            if (selected_row == -1) return;
            ListsElement elem = (ListsElement)sublist.ElementAt(selected_row);

            SCSession session = (SCSession)notification.UserInfo().ObjectForKey("session");
            if (session.GetGhostPath().Equals(elem.GetPath()))
            {
                // まさにこれだった
                bool beginning = ((String)notification.Object()).Equals("begin");
                windowController.SetEnabledOfNetworkUpdateButton(!beginning);
                windowController.SetEnabledOfVanishButton(!beginning);
                windowController.GetBootAndQuitButton().IsEnabled = !beginning;
                ghostManager.GetShellsList().SetEnabled(!beginning);
            }
        }

        public void onSessionEnteringAndLeavingPassiveMode(NSNotification notification)
        {
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            int selected_row = windowController.GetTableInstalled().SelectedIndex;
            if (selected_row == -1) return;
            ListsElement elem = (ListsElement)sublist.ElementAt(selected_row);

            SCSession session = (SCSession)notification.UserInfo().ObjectForKey("session");
            if (session.GetGhostPath().Equals(elem.GetPath()))
            {
                // まさにこれだった
                bool entering = ((String)notification.Object()).Equals("enter");
                windowController.SetEnabledOfNetworkUpdateButton(!entering);
                windowController.SetEnabledOfVanishButton(!entering);
                windowController.GetBootAndQuitButton().IsEnabled = (!entering);
                ghostManager.GetShellsList().SetEnabled(!entering);
            }
        }

        public void OnNetworkUpdaterBeginAndEnd(NSNotification notification)
        {
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            int selected_row = windowController.GetTableInstalled().SelectedIndex;
            if (selected_row == -1) return;
            ListsElement elem = (ListsElement)sublist.ElementAt(selected_row);

            SCSession session = (SCSession)notification.UserInfo().ObjectForKey("session");
            if (session.GetGhostPath().Equals(elem.GetPath()))
            {
                // まさにこれだった
                bool beginning = ((String)notification.Object()).Equals("begin");
                windowController.SetEnabledOfNetworkUpdateButton(!beginning);
                windowController.SetEnabledOfVanishButton(!beginning);
                windowController.GetBootAndQuitButton().IsEnabled = (!beginning);
                ghostManager.GetShellsList().SetEnabled(!beginning);
            }
        }

        public void OnSessionClosingAndOpeningBeginAndEnd(NSNotification notification)
        {
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            if (notification.Name.Equals("vanisher") &&
                ((String)notification.Object()).Equals("vanished")) {
                // ゴーストが一つ消えているので、リストの中身ごとリロードさせる。
                ghostManager.ReloadLists();
            }

    else
            {
              //  windowController.GetTableInstalled().ReloadData();
            }
            TableViewSelectionDidChange(null);
        }

        // implementaion of NSTableView.DataSource
        public int NumberOfRowsInTableView(NSTableView aTableView)
        {
            if (sublist == null) return 0;
            return sublist.Count();
        }

        public bool TableViewAcceptDrop(
            NSTableView tableView,
            NSDraggingInfo info,
            int row,
            int operation)
        {
            // optional
            return false;
        }

        public Object TableViewObjectValueForLocation(
            NSTableView aTableView,
            NSTableColumn aTableColumn,
            int rowIndex)
        {
            String column_id = (String)aTableColumn.Identifier;
            ListsElement elem = (ListsElement)sublist.ElementAt(rowIndex);

            if (column_id.Equals("name"))
            {
                if (SCFoundation.SharedFoundation().GetSession(elem.GetPath()) != null)
                {
                    NSMutableAttributedString mas = new NSMutableAttributedString(elem.GetName());
                    mas.AddAttributeInRange(NSAttributedString.ForegroundColorAttributeName, BOOTED_COLOR, new NSRange(0, elem.GetName().Length));
                    return mas;
                }
                else
                {
                    return elem.GetName();
                }
            }
            else if (column_id.Equals("shiori"))
            {
                if (elem.GetMasterShioriKernelName() == null)
                {
                    return SCStringsServer.GetStrFromMainDic("ghostmanager.undeterminated");
                }
                else
                {
                    return elem.GetMasterShioriKernelName();
                }
            }

            return null;
        }

        public void TableViewSetObjectValueForLocation(
            NSTableView aTableView,
            Object anObject,
            NSTableColumn aTableColumn,
            int rowIndex)
        {

        }

        public int TableViewValidateDrop(
            NSTableView tableView,
            NSDraggingInfo info,
            int row,
            int operation)
        {
            // optional
            return 0;
        }

        public bool TableViewWriteRowsToPasteboard(
            NSTableView tableView,
            NSArray rows,
            NSPasteboard pboard)
        {
            // optional
            return false;
        }

        public void TableViewSortDescriptorsDidChange(
            NSTableView tableView,
            NSArray rows)
        {
            // optional
        }

        // DeleGete of NSTableView
        public bool TableViewShouldSelectTableColumn(NSTableView aTableView, NSTableColumn aTableColumn)
        {
            NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults();

            String old_key = (String)defaults.ObjectForKey("ghostmanager.ghostlist.sort.key");
            String new_key = (String)aTableColumn.Identifier;
            if (new_key.Equals(old_key))
            {
                // キーが同じなので順序を逆に。
                String old_order = (String)defaults.ObjectForKey("ghostmanager.ghostlist.sort.order");
                String new_order = old_order.Equals("asc") ? "desc" : "asc";
                defaults.SetObjectForKey(new_order, "ghostmanager.ghostlist.sort.order");
            }
            else
            {
                // キーが違ふ。新しい順序は常に昇順。
                defaults.SetObjectForKey(new_key, "ghostmanager.ghostlist.sort.key");
                defaults.SetObjectForKey("asc", "ghostmanager.ghostlist.sort.order");
            }
            defaults.Synchronize();
            SortList();
            return false;
        }

        public void TableViewSelectionDidChange(NSNotification notification)
        {
            // notificationは無視
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();

            int selected_row = -1;

            Application.Current.Dispatcher.Invoke(() =>
            {
                selected_row = windowController.GetTableInstalled().SelectedIndex;

            if (selected_row == -1)
            {
                windowController.GetPreviewView().SetEmpty();

                windowController.SetMasterShioriKernelName("-");
                windowController.SetMakotoName("-");
                windowController.SetIdentification("-");

                ghostManager.GetBalloonsList().SetEnabled(false);
                ghostManager.GetShellsList().SetEnabled(false);
                windowController.GetScaleSlider().IsEnabled = (false);
                windowController.GetScaleIndicator().IsEnabled = (false);
                windowController.SetNumberOfShells(-1);

                // 初めて呼ばれた時のためにインジケータを初期化
                windowController.UpdateScaleIndicator();

                windowController.SetEnabledOfNetworkUpdateButton(false);
                windowController.SetEnabledOfVanishButton(false);
                windowController.GetBootAndQuitButton().Content = (SCStringsServer.GetStrFromMainDic("ghostmanager.bootandquit.boot")); // 立てる
                windowController.GetBootAndQuitButton().IsEnabled = (false);

                windowController.SetThumbnail(null);
            }
            else
            {
                ListsElement le = (ListsElement)sublist.ElementAt(selected_row);
                SCSession sessionIfRunning = SCFoundation.SharedFoundation().GetSession(le.GetPath()); // 起動していなければnull。
                bool session_is_now_booting = SCSessionStarter.SharedStarter().IsThisGhostStartingNow(le.GetPath());

                File shell_dir = new File(SCFoundation.GetParentDirOfBundle(), le.GetPath() + "/shell/" + le.GetShellDirName());
                windowController.GetPreviewView().SetImage(shell_dir);

                String masterShioriType = le.GetMasterShioriKernelName();
                if (masterShioriType == null)
                {
                    masterShioriType = SCStringsServer.GetStrFromMainDic("ghostmanager.undeterminated");
                }
                else if (masterShioriType.Length == 0)
                {
                    masterShioriType = SCStringsServer.GetStrFromMainDic("ghostmanager.shioritype.none");
                }
                windowController.SetMasterShioriKernelName(masterShioriType);
                windowController.SetIdentification(le.GetIdentification());

                ArrayList makoto_mod_names = le.GetMakotoNames();
                String makotoName;
                if (makoto_mod_names == null)
                {
                    makotoName = SCStringsServer.GetStrFromMainDic("ghostmanager.undeterminated");
                }
                else if (makoto_mod_names.Count == 0)
                {
                    makotoName = "\u7121\u3057"; // 無し
                }
                else
                {
                    StringBuffer buf = new StringBuffer();
                    int n_makoto = makoto_mod_names.Count;
                    for (int i = 0; i < n_makoto; i++)
                    {
                        String name = (String)makoto_mod_names.ToArray().ElementAt(i);
                        buf.Append(name.Length == 0 ? SCStringsServer.GetStrFromMainDic("ghostmanager.makototype.unknown") : name + ", ");
                    }
                    makotoName = buf.ToString();
                }
                windowController.SetMakotoName(makotoName);

                // バルーン
                SCInstalledBalloonsList bl = ghostManager.GetBalloonsList();
                bl.SetEnabled(true);
                bl.SelectBalloon(le.GetBalloonPath());

                // スケール
                Slider scaleSlider = ghostManager.GetWindowController().GetScaleSlider();
                scaleSlider.IsEnabled =(true);
                scaleSlider.Value = (le.GetScale());

                // インジケータに反映させる。
                ghostManager.GetWindowController().UpdateScaleIndicator();

                // シェル
                if (sessionIfRunning != null)
                {
                    bool onoff = (!sessionIfRunning.IsShellChangingSessionRunningNow() &&
                             !sessionIfRunning.IsStatusClosing() &&
                             !sessionIfRunning.IsNetworkUpdaterRunningNow() &&
                             !sessionIfRunning.IsInPassiveMode());
                    ghostManager.GetShellsList().SetEnabled(onoff);
                }
                else
                {
                    ghostManager.GetShellsList().SetEnabled(!session_is_now_booting);
                }
                ghostManager.GetShellsList().SetContent(le.GetPath());
                ghostManager.GetShellsList().SelectShell(le.GetShellDirName());
                windowController.SetNumberOfShells(windowController.GetTableShells().Items.Count);

                // サムネイル
                File thumbnailFile = le.GetThumbnailFile();
                if (thumbnailFile != null)
                {
                    BitmapImage thumbnail = null;
                    String lowercase_fname = thumbnailFile.GetName().ToLower();
                    if (lowercase_fname.EndsWith(".png"))
                    {
                        thumbnail = new BitmapImage(new Uri(thumbnailFile.GetPath()));
                    }
                    else if (lowercase_fname.EndsWith(".pnr"))
                    {
                       // thumbnail = SCAlphaConverter.ConvertImage(
                     //   new NSImage(thumbnailFile.GetPath(), true));
                    }
                    windowController.SetThumbnail(thumbnail);
                }
                else
                {
                    windowController.SetThumbnail(null);
                }

                // ネットワーク更新,立て/消し
                if (sessionIfRunning == null)
                {
                    windowController.SetEnabledOfNetworkUpdateButton(false);
                    windowController.SetEnabledOfVanishButton(false);

                    windowController.GetBootAndQuitButton().Content = (SCStringsServer.GetStrFromMainDic("ghostmanager.bootandquit.boot"));
                    windowController.GetBootAndQuitButton().IsEnabled = (!session_is_now_booting);
                }
                else
                {
                    windowController.GetBootAndQuitButton().Content = (SCStringsServer.GetStrFromMainDic("ghostmanager.bootandquit.quit"));
                    windowController.GetBootAndQuitButton().IsEnabled = (
                        !sessionIfRunning.IsStatusClosing() &&
                        !sessionIfRunning.IsNetworkUpdaterRunningNow() &&
                        !sessionIfRunning.IsInPassiveMode());
                    windowController.SetEnabledOfNetworkUpdateButton(
                        sessionIfRunning.GetStringFromShiori("homeurl") != null &&
                        !sessionIfRunning.IsNetworkUpdaterRunningNow() &&
                        !sessionIfRunning.IsStatusClosing() &&
                        !sessionIfRunning.IsInPassiveMode());
                    NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults();
                    windowController.SetEnabledOfVanishButton(
                        (defaults.IntegerForKey("misc.always_show_vanish") == 1 ||
                         sessionIfRunning.GetStringFromShiori("vanishbuttonvisible") == null ||
                         !sessionIfRunning.GetStringFromShiori("vanishbuttonvisible").Equals("0")) &&
                        !sessionIfRunning.IsStatusClosing() &&
                        !sessionIfRunning.IsNetworkUpdaterRunningNow() &&
                        !sessionIfRunning.IsInPassiveMode());
                }
            }

            });

        }

        private class GhostMenuItem : NSMenuItem
        {
            SCSession self;
            String path;
            String balloonPath;
            String selfname;

            double scale;


        public GhostMenuItem(SCSession self, String name, String path, String balloonPath, String selfname, double scale):base(name, null, "")
        {
            //super(name, null, "");
            this.self = self;
            this.path = path;
            this.balloonPath = balloonPath;
            this.selfname = selfname;
            this.scale = scale;
        }

        public SCSession GetSelf()
        {
            return self;
        }

        public String GetPath()
        {
            return path;
        }

        public String GetBalloonPath()
        {
            return balloonPath;
        }

        public String GetSelfName()
        {
            return selfname;
        }

        public double GetScale()
        {
            return scale;
        }
 
        }
}
}
