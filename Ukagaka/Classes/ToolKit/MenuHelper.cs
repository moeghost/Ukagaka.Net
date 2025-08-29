using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Ukagaka.Classes.ToolKit
{
    internal class MenuHelper
    {

        public static ContextMenuStrip ConvertWpfMenuToWinForms(System.Windows.Controls.Menu wpfMenu)
        {
            var contextMenuStrip = new ContextMenuStrip();

            foreach (var item in wpfMenu.Items)
            {
                if (item is System.Windows.Controls.MenuItem wpfMenuItem)
                {
                    var winFormsItem = ConvertMenuItem(wpfMenuItem);
                    contextMenuStrip.Items.Add(winFormsItem);
                }
                else if (item is Separator)
                {
                    contextMenuStrip.Items.Add(new ToolStripSeparator());
                }
            }

            return contextMenuStrip;
        }

        public static ToolStripMenuItem ConvertWpfMenuToWinFormsToolStripMenu(System.Windows.Controls.Menu wpfMenu)
        {
            var contextMenuStrip = new ToolStripMenuItem();

            foreach (var item in wpfMenu.Items)
            {
                if (item is System.Windows.Controls.MenuItem wpfMenuItem)
                {
                    var winFormsItem = ConvertMenuItem(wpfMenuItem);
                    contextMenuStrip.DropDownItems.Add(winFormsItem);
                }
                else if (item is Separator)
                {
                    contextMenuStrip.DropDownItems.Add(new ToolStripSeparator());
                }
            }

            return contextMenuStrip;
        }


        public static ToolStripMenuItem ConvertWpfMenuToWinFormsToolStripMenu(System.Windows.Controls.ContextMenu wpfMenu)
        {
            var contextMenuStrip = new ToolStripMenuItem();

            foreach (var item in wpfMenu.Items)
            {
                if (item is System.Windows.Controls.MenuItem wpfMenuItem)
                {
                    var winFormsItem = ConvertMenuItem(wpfMenuItem);
                    contextMenuStrip.DropDownItems.Add(winFormsItem);
                }
                else if (item is Separator)
                {
                    contextMenuStrip.DropDownItems.Add(new ToolStripSeparator());
                }
            }

            return contextMenuStrip;
        }

        private static ToolStripMenuItem ConvertMenuItem(System.Windows.Controls.MenuItem wpfMenuItem)
        {
            var winFormsItem = new ToolStripMenuItem
            {
                Text = wpfMenuItem.Header?.ToString(),
                Enabled = wpfMenuItem.IsEnabled,
                // 可以添加图标: Image = ConvertImage(wpfMenuItem.Icon)
            };

            // 添加点击事件
            if (wpfMenuItem.Command != null)
            {
                winFormsItem.Click += (s, e) =>
                {
                    if (wpfMenuItem.Command.CanExecute(wpfMenuItem.CommandParameter))
                        wpfMenuItem.Command.Execute(wpfMenuItem.CommandParameter);
                };
            }
            else
            {
                winFormsItem.Click += (s, e) => wpfMenuItem.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.MenuItem.ClickEvent));
            }

            // 递归处理子菜单
            foreach (var childItem in wpfMenuItem.Items)
            {
                if (childItem is System.Windows.Controls.MenuItem childWpfMenuItem)
                {
                    winFormsItem.DropDownItems.Add(ConvertMenuItem(childWpfMenuItem));
                }
                else if (childItem is Separator)
                {
                    winFormsItem.DropDownItems.Add(new ToolStripSeparator());
                }
            }

            return winFormsItem;
        }





    }
}
