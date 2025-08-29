using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;
using Ukagaka;
using Cocoa.AppKit;
using System.Collections;
using Utils;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Data;
namespace Ukagaka
{

    public class SCShellsList
    {
        private readonly SCGhostManager _ghostManager;
        private readonly ObservableCollection<ListsElement> _list;
        private int _selectedItemIndex = -1;

        public SCShellsList(SCGhostManager ghostManager)
        {
            _list = new ObservableCollection<ListsElement>();
            _ghostManager = ghostManager ?? throw new ArgumentNullException(nameof(ghostManager));

            // Initialize the table view
            var windowController = ghostManager.GetWindowController();
            var tableView = windowController.GetTableShells();

            // 创建GridView作为ListView的View
            var gridView = new GridView();



            // Configure the table view columns
            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Select",
                CellTemplate = (DataTemplate)Application.Current.FindResource("RadioButtonColumnTemplate")
            });

            gridView.Columns.Add(new GridViewColumn
            {
                Header = "Name",
                DisplayMemberBinding = new Binding("DisplayName")
            });

         
            // 将GridView设置为ListView的View
            tableView.View = gridView;

            // 设置数据源
            tableView.ItemsSource = _list;

            // 启用单选模式
            tableView.SelectionMode = SelectionMode.Single;
        }

        public void SetEnabled(bool enabled)
        {
            var windowController = _ghostManager.GetWindowController();
            var tableView = windowController.GetTableShells();
            Application.Current.Dispatcher.Invoke(() =>
            {
                tableView.IsEnabled = enabled;
            });
        }

        public void SetContent(string ghostRoot)
        {
            _list.Clear();
            _selectedItemIndex = -1;

            // Read directory structure
            string bundleDir = SCFoundation.GetParentDirOfBundle();
            string shellDir = Path.Combine(bundleDir, ghostRoot, "shell");

            if (!Directory.Exists(shellDir))
                return;

            foreach (var shellPath in Directory.GetDirectories(shellDir))
            {
                Utils.File descript = new Utils.File(shellPath, "descript.txt");
                if (!descript.Exists())
                {
                    continue;
                }
                // Read descript.txt
                var desc = new SCDescription(descript);
                string shellName = desc.GetStrValue("name");
                string dirName = Path.GetFileName(shellPath);

                _list.Add(new ListsElement(shellName, dirName));
            }
        }

        public void SelectShell(string shellDir)
        {
            for (int i = 0; i < _list.Count; i++)
            {
                if (shellDir == _list[i].DirName)
                {
                    _selectedItemIndex = i;
                    var windowController = _ghostManager.GetWindowController();
                    var tableView = windowController.GetTableShells();
                    tableView.SelectedIndex = i;
                    tableView.ScrollIntoView(tableView.SelectedItem);
                    return;
                }
            }

            _selectedItemIndex = -1;
            var controller = _ghostManager.GetWindowController();
            controller.GetTableShells().SelectedIndex = -1;
        }

        public NSMenu MakeShellMenu(SCSession self)
        {
            var submenu = new NSMenu();

            string bundleDir = SCFoundation.GetParentDirOfBundle();
            string shellDir = Path.Combine(bundleDir, self.GetGhostPath(), "shell");

            if (!Directory.Exists(shellDir))
                return submenu;

            foreach (var shellPath in Directory.GetDirectories(shellDir))
            {
                Utils.File descript = new Utils.File(shellPath, "descript.txt");
                if (!descript.Exists())
                {
                    continue;
                }
                var desc = new SCDescription(descript);
                string shellName = desc.GetStrValue("name");
                string dirName = Path.GetFileName(shellPath);

                var item = new MenuItem
                {
                    Header = shellName ?? dirName,
                    Command = new ChangeShellCommand(this, self, dirName),
                    IsChecked = self.GetCurrentShell().GetDirName() == dirName,
                    IsEnabled = self.GetCurrentShell().GetDirName() != dirName,
                    
                };
                
                submenu.Items.Add(item);
            }

            return submenu;
        }

        public void ChangeShellTo(SCSession session, string dirName)
        {
            _ghostManager.GetInstalledList().ChangeShellOfGhost(session.GetGhostPath(), dirName);
        }

        private class ChangeShellCommand : ICommand
        {
            private readonly SCShellsList _parent;
            private readonly SCSession _session;
            private readonly string _dirName;

            public ChangeShellCommand(SCShellsList parent, SCSession session, string dirName)
            {
                _parent = parent;
                _session = session;
                _dirName = dirName;
            }

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                _parent.ChangeShellTo(_session, _dirName);
            }

            public event EventHandler CanExecuteChanged;
        }

        public class ListsElement
        {
            public string Name { get; }
            public string DirName { get; }
            public string DisplayName => Name ?? DirName;

            public ListsElement(string name, string dirName)
            {
                Name = name;
                DirName = dirName;
            }
        }
    }






    /*






    public class SCShellsList1 : NSTableView.DataSource
    {
        ArrayList list; // 中身はSCShellsList.ListsElement。
        int selected_item = 0; // ラジオボタンで選択されている項目。

        SCGhostManager ghostManager;

        private static NSSelector selector_changeShellTo = new NSSelector("changeShellTo", new object());


        public SCShellsList1(SCGhostManager ghostManager)
        {
            this.ghostManager = ghostManager;
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            NSTableView tableView = windowController.GetTableShells();

            tableView.SetDataSource(this);

            NSButtonCell bc = new NSButtonCell();
            bc.SetButtonType(NSButtonCell.RadioButton);
            bc.SetTitle("");
            list = new ArrayList();
            NSTableColumn tc = tableView.TableColumnWithIdentifier("select");
            if (tc == null)
            {
                System.Console.Write(
            "SCShellsList error : The table doesn\'t have a NSTableColumn named \"select\".");
                return;
            }
            tc.SetDataCell(bc);
          
        }

        public void SetEnabled(bool value)
        {
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();

            NSTableView tableView = windowController.GetTableShells();
            NSTableColumn tc;

            tc = tableView.TableColumnWithIdentifier("select");

            if (tc == null)
            {
                return;
            }

            tc.DataCell().SetEnabled(value);
            //tableView.display();
            tableView.SetNeedsDisplay(true);
        }

        public void SetContent(string ghost_root)
        {
            // ghost_root : home/ghost/[ghostname]
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
           // windowController.GetTableShells().DeSelectAll(this);
            list.Clear();

            // ディレクトリ構成を読んでリストを構成します。
            string bundleDir = SCFoundation.GetParentDirOfBundle();
            File shellDir = new File(bundleDir, ghost_root + "/shell");
            File[] shells = shellDir.ListFiles();
            for (int i = 0; i < shells.Length; i++)
            {
                if (!shells[i].IsDirectory())
                {
                    continue;
                }
                File descript_txt = new File(shells[i], "descript.txt");
                if (!descript_txt.Exists())
                {
                    continue; // descript.txtが無い。
                }
                // その中にあるdescript.txtを読む。
                SCDescription desc = new SCDescription(descript_txt);
                String shellname = desc.GetStrValue("name");
                String dirname = shells[i].GetName();

                list.Add(new ListsElement(shellname, dirname));
            }

            windowController.GetTableShells().ReloadData();
        }

        public void SelectShell(string shelldir)
        {
            // ディレクトリ名で指定されたシェルを選びます。
            // 見つからなかったら選択を解除します。
            SCGhostManagerWindowController windowController = ghostManager.GetWindowController();
            int n_items = list.Count;
            int found_id = -1;
            for (int i = 0; i < n_items; i++)
            {
                if (shelldir.Equals(((ListsElement)list.ToArray().ElementAt(i)).GetDirName()))
                {
                    // 見つけた
                    found_id = i;
                    break;
                }
            }
            if (found_id == -1)
            {
                selected_item = -1;
                windowController.GetTableShells().ReloadData();
                return;
            }
            selected_item = found_id;

            NSTableView tableView = windowController.GetTableShells();
            tableView.ReloadData();
            tableView.ScrollRowToVisible(found_id);
        }

        public NSMenu MakeShellMenu(SCSession self)
        {
            NSMenu submenu = new NSMenu();
            submenu.SetAutoenablesItems(false);

            // ディレクトリ構成を読んでリストを構成します。
            string bundleDir = SCFoundation.GetParentDirOfBundle();
            File shellDir = new File(bundleDir, self.GetGhostPath() + "/shell");
            File[] shells = shellDir.ListFiles();
            for (int i = 0; i < shells.Length; i++)
            {
                if (!shells[i].IsDirectory())
                {
                    continue;
                }
                // その中にあるdescript.txtを読む。
                SCDescription desc = new SCDescription(new Utils.File(shells[i], "descript.txt"));
                string shellname = desc.GetStrValue("name");
                string dirname = shells[i].GetName();

                ShellMenuItem item = new ShellMenuItem(self, shellname == null ? dirname : shellname, dirname);
                item.SetAction(selector_changeShellTo);
                item.SetTarget(this);
                item.SetState(self.GetCurrentShell().GetDirName().Equals(dirname) ? NSCell.OnState : NSCell.OffState);
                item.SetEnabled(item.State() != NSCell.OnState);

                submenu.AddItem(item);
            }

            return submenu;
        }

        public void ChangeShellTo(Object sender)
        {
            // メニューから呼ばれるActionです。
            if (!(sender is ShellMenuItem))
            {
                return;
            }
            ShellMenuItem smi = (ShellMenuItem)sender;

            ghostManager.GetInstalledList().ChangeShellOfGhost(smi.GetSelf().GetGhostPath(), smi.GetDirName());
        }

        private class ListsElement
        {
            string name;
            string dirname;

            public ListsElement(string name, string dirname)
            {
                this.name = name;
                this.dirname = dirname;
            }

            public string GetName()
            {
                return name;
            }

            public string GetDirName()
            {
                return dirname;
            }
        }

        private class ShellMenuItem : NSMenuItem
        {
            SCSession self;
            String dirname;


            public ShellMenuItem(SCSession self, string name, string dirname) : base(name, null, "")
            {
                // super(name, null, "");

                this.self = self;
                this.dirname = dirname;
            }

            public SCSession GetSelf()
            {
                return self;
            }

            public String GetDirName()
            {
                return dirname;
            }

            internal int State()
            {
                return NSCell.OffState;
            }
        }

        // implementaion of NSTableView.DataSource
        public int NumberOfRowsInTableView(NSTableView aTableView)
        {
            if (list == null)
            {
                return 0;
            }
            return list.Count;
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

        public void TableViewSortDescriptorsDidChange(
            NSTableView tableView,
            NSArray rows)
        {
            // optional
        }

        public Object TableViewObjectValueForLocation(
            NSTableView aTableView,
            NSTableColumn aTableColumn,
            int rowIndex)
        {

            string column_id = (string)aTableColumn.Identifier;
            if (column_id.Equals("select"))
            {
                return (selected_item == rowIndex ? 1 : 0);
            }

            // selectでなければname。
            ListsElement elem = (ListsElement)list.ToArray().ElementAt(rowIndex);
            return (elem.GetName() == null ? elem.GetDirName() : elem.GetName());
        }

        public void TableViewSetObjectValueForLocation(
            NSTableView aTableView,
            Object anObject,
            NSTableColumn aTableColumn,
            int rowIndex)
        {

            String column_id = (string)aTableColumn.Identifier;

            if (column_id.Equals("select"))
            {
                selected_item = rowIndex;
                aTableView.ReloadData();

                String dirname = ((ListsElement)list.ToArray().ElementAt(rowIndex)).GetDirName();
                ghostManager.GetInstalledList().ChangeShellOfCurrentGhost(dirname);
            }
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


    }
    */
}
