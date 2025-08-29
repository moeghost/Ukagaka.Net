using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ukagaka
{
    public class SCMenuController
    {
        private static SCMenuController _sharedInstance = null;

        private MenuItem _showPrefWindow;
        private MenuItem _quit;
        private MenuItem _mailcheck;
        private Menu _mainMenu;

        public static SCMenuController SharedMenuController()
        {
            return _sharedInstance;
        }

        public SCMenuController(Menu mainMenu)
        {
            _sharedInstance = this;
            _mainMenu = mainMenu;
            InitializeMenuItems();
        }

        private void InitializeMenuItems()
        {
            // You'll need to create and wire up your menu items here
            // This is a basic structure - you'll need to adapt it to your actual menu structure

            _showPrefWindow = new MenuItem { Header = "Preferences" };
            _showPrefWindow.Click += (sender, e) => ShowPrefWindow(sender);

            _quit = new MenuItem { Header = "Quit" };
            _quit.Click += (sender, e) => Quit(sender);

            _mailcheck = new MenuItem { Header = "Check Mail" };
            _mailcheck.Click += (sender, e) => DoMailCheck(sender);

            // Add more menu items as needed
            // _mainMenu.Items.Add(_showPrefWindow);
            // etc.
        }

        public void Quit(object sender)
        {
            SCFoundation.SharedFoundation().PerformQuit();
        }

        public void PseudoAI(object sender)
        {
            // Debug use - makes AI speak random phrases
            SCSession anySession = SCFoundation.SharedFoundation().GetSession();
            if (anySession != null)
            {
                anySession.DoShioriRandomTalk();
            }
        }

        public void EventOnBoot(object sender)
        {
            // Debug use - sends OnBoot event
            SCSession anySession = SCFoundation.SharedFoundation().GetSession();
            if (anySession != null)
            {
                anySession.DoShioriEvent("OnBoot", new string[] { anySession.GetCurrentShell().GetShellName() });
            }
        }

        public void ShowPrefWindow(object sender)
        {
            SCPrefWindowController pwc = SCPrefWindowController.SharedPrefWindowController();
            pwc.ShowWindow();
        }

        public void ShowShellManager(object sender)
        {
            SCGhostManager shellm = SCGhostManager.SharedGhostManager();
            shellm.ShowManagerWindow();
        }

        public async Task DoMailCheck(object sender)
        {
           await SCFoundation.SharedFoundation().DoMailCheckAsync();
        }

        public void ShowUpdateChecker(object sender)
        {
            //SCUpdateChecker.SharedUpdateChecker().ShowWindow();
        }

        public void ToggleShioriLog(object sender)
        {
            SCFoundation.LOG_SHIORI_SESSION = !SCFoundation.LOG_SHIORI_SESSION;
        }

        public void ToggleShioriLocked(object sender)
        {
            SCFoundation.LOCK_SHIORI_EVENTS = !SCFoundation.LOCK_SHIORI_EVENTS;
            ((MenuItem)sender).IsChecked = SCFoundation.LOCK_SHIORI_EVENTS;
        }

        public bool ValidateMenuItem(MenuItem menuItem)
        {
            if (menuItem == _showPrefWindow)
            {
                SCPrefWindowController pref = SCPrefWindowController.SharedPrefWindowController();
                return !pref.IsVisible;
            }
            else if (menuItem == _mailcheck)
            {
                SCSession anySession = SCFoundation.SharedFoundation().GetSession();
                if (anySession == null) return false;

                var defaults = Properties.Settings.Default;

                if (defaults.MailcheckCheckEnable != true) return false;

                if (string.IsNullOrEmpty(defaults.MailcheckServer)) return false;
                if (string.IsNullOrEmpty(defaults.MailcheckUsername)) return false;
                if (string.IsNullOrEmpty(defaults.MailcheckPassword)) return false;

                return true;
            }
            else if (menuItem == _quit)
            {
                // Returns false if any session is in passive mode
                List<SCSession> sessions = SCFoundation.SharedFoundation().GetSessionsList();
                foreach (SCSession session in sessions)
                {
                    if (session.IsInPassiveMode()) return false;
                }
                return true;
            }

            return true;
        }

        public ContextMenu MakeNewBindGroupSubMenu(string title)
        {
            // Creates and returns a new bind group submenu for "costume change"
            // Inserts it under the ID 1 item ("Costume Change")

            // In WPF, we typically use ContextMenu or MenuItem with subitems
            var itemSubmenu = new MenuItem { Header = title, Tag = 2 };

            // Find the "Costume Change" menu item (tag=1) and insert after it
            int insertIndex = FindMenuItemIndexByTag(1) + 1;
            if (insertIndex > 0 && insertIndex < _mainMenu.Items.Count)
            {
                _mainMenu.Items.Insert(insertIndex, itemSubmenu);
            }

            var submenu = new ContextMenu();
            submenu.IsEnabled = true; // Equivalent to setAutoenablesItems(false)
            itemSubmenu.ItemsSource = submenu.Items;

            return submenu;
        }

        public void RemoveBindGroupSubMenu(string title)
        {
            // Finds and removes a bind group submenu
            // Searches among items with ID 2
            for (int i = 0; i < _mainMenu.Items.Count; i++)
            {
                if (_mainMenu.Items[i] is MenuItem item &&
                    item.Tag is int tag && tag == 2 &&
                    item.Header.ToString() == title)
                {
                    _mainMenu.Items.RemoveAt(i);
                    break;
                }
            }
        }

        private int FindMenuItemIndexByTag(int tag)
        {
            for (int i = 0; i < _mainMenu.Items.Count; i++)
            {
                if (_mainMenu.Items[i] is MenuItem item && item.Tag is int itemTag && itemTag == tag)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
