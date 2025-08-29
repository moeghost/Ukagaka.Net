using Cocoa.AppKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Controls;

namespace Ukagaka
{
    public class SCInstalledBalloonsList: NSTableView.DataSource
    {
        private readonly List<ListElement> _list = new List<ListElement>();
        private int _selectedItem = 0; // 0 is built-in
        private readonly SCGhostManager _ghostManager;

        public SCInstalledBalloonsList(SCGhostManager ghostManager)
        {
            _ghostManager = ghostManager;
            ReloadList();
        }

        public void SetEnabled(bool enabled)
        {
            // In WPF, we would enable/Disable the entire ListView
            // This would need to be implemented in the view layer
        }

        public void SelectBalloon(string balloonPath)
        {
            int foundId = 0; // 0 = built-in
            for (int i = 0; i < _list.Count; i++)
            {
                if (balloonPath.Equals(_list[i].Path))
                {
                    foundId = i + 1;
                    break;
                }
            }
            _selectedItem = foundId;

            // In WPF, we would update the selection in the ListView
            // and scroll to the selected item
        }

        public void ReloadList()
        {
            _list.Clear();

            string bundleDir = SCFoundation.GetParentDirOfBundle();
            string shellDir = Path.Combine(bundleDir, "home/balloon");

            if (Directory.Exists(shellDir))
            {
                foreach (var dir in Directory.GetDirectories(shellDir))
                {
                    string descriptPath = Path.Combine(dir, "descript.txt");
                    if (File.Exists(descriptPath))
                    {
                        _list.Add(new ListElement($"home/balloon/{Path.GetFileName(dir)}"));
                    }
                }
            }

            int maxHeight = GetMaxHeightOfThumbnails();
            // In WPF, we would set the row height in the view
        }

        private int GetMaxHeightOfThumbnails()
        {
            int result = 0;
            foreach (var element in _list)
            {
                if (element.Thumbnail != null && element.Thumbnail.Height > result)
                {
                    result = (int)element.Thumbnail.Height;
                }
            }
            return result;
        }

        public NSMenu MakeBalloonMenu(SCSession self)
        {
            var menu = new NSMenu();

            // Built-in item
            var builtInItem = new MenuItem
            {
                Header = "[built-in]",
                Tag = "",
                IsChecked = string.IsNullOrEmpty(self.GetBalloonServer().GetPath()),
                IsEnabled = !string.IsNullOrEmpty(self.GetBalloonServer().GetPath())
            };
            builtInItem.Click += (sender, e) => ChangeBalloonTo(self, "");
            menu.Items.Add(builtInItem);

            // Balloon items
            foreach (var element in _list)
            {
                var item = new MenuItem
                {
                    Header = element.Name,
                    Tag = element.Path,
                    IsChecked = self.GetBalloonServer().GetPath().Equals(element.Path),
                    IsEnabled = !self.GetBalloonServer().GetPath().Equals(element.Path)
                };

                if (element.Thumbnail != null)
                {
                    element.Thumbnail.LockFocus();

                    item.Icon = new Image { Source = element.Thumbnail.GetBitmapImage(), Width = 16, Height = 16 };
                }

                item.Click += (sender, e) => ChangeBalloonTo(self, element.Path);
                menu.Items.Add(item);
            }

            return menu;
        }

        private void ChangeBalloonTo(SCSession self, string balloonPath)
        {
            _ghostManager.GetInstalledList().ChangeBalloonOfGhost(
                self.GetGhostPath(), balloonPath);
        }

        // WPF Data Model for the ListView
        public class BalloonItem
        {
            public bool IsSelected { get; set; }
            public NSImage Thumbnail { get; set; }
            public string Name { get; set; }
            public string Path { get; set; }
        }

        public IEnumerable<BalloonItem> GetBalloonItems()
        {
            // Built-in item
            yield return new BalloonItem
            {
                IsSelected = _selectedItem == 0,
                Name = "[built-in]",
                Path = ""
            };

            // Balloon items
            for (int i = 0; i < _list.Count; i++)
            {
                yield return new BalloonItem
                {
                    IsSelected = _selectedItem == i + 1,
                    Thumbnail = _list[i].Thumbnail,
                    Name = _list[i].Name,
                    Path = _list[i].Path
                };
            }
        }

        public void HandleSelectionChanged(int selectedIndex)
        {
            _selectedItem = selectedIndex;
            string balloonPath = selectedIndex == 0 ? "" : _list[selectedIndex - 1].Path;
            _ghostManager.GetInstalledList().ChangeBalloonOfCurrentGhost(balloonPath);
        }

        private class ListElement
        {
            public string Name { get; }
            public string Path { get; }
            public NSImage Thumbnail { get; }

            public ListElement(string path)
            {
                Path = path;

                string bundleDir = SCFoundation.GetParentDirOfBundle();
                var desc = new SCDescription(new Utils.File(bundleDir, path, "descript.txt"));
                Name = desc.GetStrValue("name") ?? "Undefined Balloon";

                // Load thumbnail
                Utils.File thumbnail = new  Utils.File(bundleDir, path, "thumbnail.png");
                if (thumbnail.Exists())
                {
                    Thumbnail = LoadImage(thumbnail);
                }
                else
                {
                    thumbnail = new Utils.File(bundleDir, path, "thumbnail.pnr");
                    if (thumbnail.Exists())
                    {
                        Thumbnail = LoadImage(thumbnail);
                        // Note: In original code, SCAlphaConverter would process the image
                    }
                }
            }

            private NSImage LoadImage(Utils.File file)
            {
                NSImage image = null;
                // 在后台线程中调用：
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    image = new NSImage(file.GetFullName()); ;
                });
                return image;
            }

            private NSImage LoadImage(string path)
            {
                try
                {
                    NSImage image = null;
                    // 在后台线程中调用：
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        image = new NSImage(path); ;
                    });
                    return image;
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}
