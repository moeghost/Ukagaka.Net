using Cocoa.AppKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

using Ukagaka;
using System.Collections;

namespace Ukagaka
{
    public class SCGhostManager
    {
        private static SCGhostManager sharedInstance = null;

        SCGhostManagerWindowController managerWindowController;
        SCInstalledGhostsList installedList;
        SCInstalledBalloonsList balloonsList;
        SCShellsList shellsList;

        public static SCGhostManager SharedGhostManager()
        {
            if (sharedInstance == null)
            {
                sharedInstance = new SCGhostManager();
            }
            return sharedInstance;
        }

        public SCGhostManager()
        {
            managerWindowController = new SCGhostManagerWindowController(this);

            balloonsList = new SCInstalledBalloonsList(this);
            shellsList = new SCShellsList(this);
            installedList = new SCInstalledGhostsList(this);
            /*
            NSNotificationCenter.defaultCenter().addObserver(
                this,
                new NSSelector("notification_reload", new Class[] { NSNotification.class}),
	    "installer.end",
	    null);
            */
        }

        public void ReloadLists()
        {
            installedList.ReloadList();
            balloonsList.ReloadList();
        }

        public void Notification_reload(NSNotification notification)
        {
            /*
    SCOldTypeConverter.ConvertAll();
    SCOldBalloonConverter.ConvertAll();
    SCGhostThumbnailMover.MoveAll();
    ReloadLists();
            */
        }

        public void ShowManagerWindow()
        {

            managerWindowController.ShowWindow(null);
        }

        public SCGhostManagerWindowController GetWindowController()
        {
            return managerWindowController;
        }

        public SCInstalledGhostsList GetInstalledList()
        {
            return installedList;
        }

        public SCInstalledBalloonsList GetBalloonsList()
        {
            return balloonsList;
        }

        public SCShellsList GetShellsList()
        {
            return shellsList;
        }
        public async Task BootAllGhostsToBootAsync()
        {
            BootAllGhostsToBoot();
        }

        public void BootAllGhostsToBoot()
        {
            // 解説はSCInstalledGhostsList.bootAllGhostsToBootにあります。
            installedList.BootAllGhostsToBoot();
            //(new _ZZZ()).Start();
        }

        public void ClearBootFlags()
        {
            installedList.ClearBootFlags();
        }

        /*private class _ZZZ extends Thread
        {
            public void Run()
            {
                int pool = NSAutoreleasePool.push();
                installedList.bootAllGhostsToBoot();
                NSAutoreleasePool.pop(pool);
            }
        }*/

        public ArrayList GetAllGhostPathsInAsleep()
        {
            return installedList.GetAllGhostPathsInAsleep();
        }

        public string GetBalloonPathOfGhost(String ghostPath)
        {
            return installedList.GetBalloonPathOfGhost(ghostPath);
        }

        public double GetScaleOfGhost(String ghostPath)
        {
            return installedList.GetScaleOfGhost(ghostPath);
        }

        public String GetNameOfGhost(String ghostPath)
        {
            return installedList.GetNameOfGhost(ghostPath);
        }

        public void forceHaltGhost(SCSession session)
        {
            // 状態をファイルに保存してセッションを閉じますが、
            // その際にOnCloseを発行しません。
            if (session.IsInPassiveMode())
            {
                return;
            }
            GetInstalledList().SetBootFlag(session.GetGhostPath(), false);
            SetBootFlagInDefaults(session.GetGhostPath(), false);
            session.Close();
        }

        public void CloseGhost(SCSession session)
        {
            // 状態をファイルに保存してセッションを閉じます。
            if (session.IsInPassiveMode())
            {
                return;
            }
            GetInstalledList().SetBootFlag(session.GetGhostPath(), false);
            SetBootFlagInDefaults(session.GetGhostPath(), false);
            session.PerformClose();
        }

        public NSMutableDictionary GetGhostDefaults(string path)
        {
            // path : home/ghost/[^/]+
            // なんて中途半端な正規表現で書いてみたりする。
            NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults();
            NSMutableDictionary dic = defaults.DictionaryForKey("ghost.pref." + path);
            return (dic == null ? new NSMutableDictionary() : dic);
        }

        public void SetGhostDefaults(string path, NSMutableDictionary dic)
        {
            // path : home/ghost/[^/]+
            NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults();
            defaults.SetObjectForKey(dic, "ghost.pref." + path);
            defaults.Synchronize();
        }

        public void SetBootFlagInDefaults(string path, bool value)
        {
            NSMutableDictionary ghostDefaults = GetGhostDefaults(path);
            ghostDefaults.SetObjectForKey(value ? 1 : 0, "boot_flag");
            SetGhostDefaults(path, ghostDefaults);
        }

        public List<SCInstalledGhostsList.ListsElement> GetInstalledGhostList()
        {
            return installedList.GetInstalledGhostList();
        }

        public NSMenu MakeGhostMenu(SCSession self)
        {
            // selfにチェックマークが付いた状態でメニューが作られます。
            // また、self以外の起動中のゴーストは灰色表示されます。
            return installedList.MakeGhostMenu(self);
        }

        public NSMenu MakeShellMenu(SCSession self)
        {
            // selfのカレントシェルにチェックマークが付いた状態でメニューが作られます。
            return shellsList.MakeShellMenu(self);
        }

       

        internal NSMenu MakeBalloonMenu(SCSession session)
        {
            return balloonsList.MakeBalloonMenu(session);
        }
    }
}
