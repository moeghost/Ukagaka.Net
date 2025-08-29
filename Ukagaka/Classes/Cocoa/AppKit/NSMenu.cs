using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Ukagaka;

namespace Cocoa.AppKit
{
    public class NSMenu:Menu
    {
        private bool _autoenablesItems = true;
        private object _menuRepresentation;

        public NSMenu()
        {
            // 使用 ItemsSource 替代直接访问 Items
            var items = new ObservableCollection<NSMenuItem>();
            items.CollectionChanged += (s, e) => UpdateItemsState();
            Items.Add(items);
            IsPortrait = true;
        }

        private bool _IsPortrait = false;
        public bool IsPortrait
        {
            get
            {
                return _IsPortrait;
            }

            set
            {
                _IsPortrait = value;
                if (value)
                {
                    SetVerticalMenuPanel(this);
                }

                else
                {
                    SetHorizontalMenuPanel(this);
                }

            }

        }





        // 设置 Menu 为垂直排列
        private void SetVerticalMenuPanel(NSMenu menu)
        {
            // 方式1：直接使用代码构建 ItemsPanelTemplate
            var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Vertical);

            var itemsPanelTemplate = new ItemsPanelTemplate();
            itemsPanelTemplate.VisualTree = stackPanelFactory;

            menu.ItemsPanel = itemsPanelTemplate;

            // 方式2：通过 XAML 字符串加载（更灵活）
            /*
            string xaml = @"<ItemsPanelTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                                <StackPanel Orientation='Vertical' />
                            </ItemsPanelTemplate>";
            menu.ItemsPanel = (ItemsPanelTemplate)XamlReader.Parse(xaml);
            */
        }

        private void SetHorizontalMenuPanel(NSMenu menu)
        {
            // 方式1：直接使用代码构建 ItemsPanelTemplate
            var stackPanelFactory = new FrameworkElementFactory(typeof(StackPanel));
            stackPanelFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);

            var itemsPanelTemplate = new ItemsPanelTemplate();
            itemsPanelTemplate.VisualTree = stackPanelFactory;

            menu.ItemsPanel = itemsPanelTemplate;

            // 方式2：通过 XAML 字符串加载（更灵活）
            /*
            string xaml = @"<ItemsPanelTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                                <StackPanel Orientation='Vertical' />
                            </ItemsPanelTemplate>";
            menu.ItemsPanel = (ItemsPanelTemplate)XamlReader.Parse(xaml);
            */
        }





        public NSMenu(NSMenu menu) : this()
        {
            var items = new ObservableCollection<NSMenuItem>();
            items.CollectionChanged += (s, e) => UpdateItemsState();
            foreach (var item in menu.Items)
            {
                if (item is ItemCollection itemCollection)
                {
                    var menuItem = new NSMenuItem(itemCollection);

                    items.Add(menuItem);

                }
                if (item is NSMenuItem nSMenuItem)
                {

                    var menuItem = new NSMenuItem(nSMenuItem);

                    items.Add(nSMenuItem);

                }
            }
             
            Items.Add(items);
        }

        public NSMenu(string title) : this()
        {
        }

        internal void AddItem(NSMenuItem item)
        {
            Items.Add(item);
            UpdateItemsState();
        }

        internal NSMenuItem ItemAtIndex(int index)
        {
            if (index >= 0 && index < Items.Count)
            {
                return Items[index] as NSMenuItem;
            }
            return null;
        }

        internal int NumberOfItems()
        {
            return Items.Count;
        }

        internal void SetAutoenablesItems(bool autoenable)
        {
            _autoenablesItems = autoenable;
            UpdateItemsState();
        }

        internal void SetMenuRepresentation(object representation)
        {
            _menuRepresentation = representation;
        }

        private void UpdateItemsState()
        {
            if (!_autoenablesItems) return;

            foreach (var item in Items)
            {
                if (item is NSMenuItem menuItem)
                {
                    menuItem.SetEnabled(menuItem.GetAction() != null || menuItem.HasSubmenu());
                }
            }
        }

        public Menu ConvertToMenu()
        {
            Menu menu = new Menu();
            foreach (var item in Items)
            {
                if (item is NSMenuItem)
                {
                    NSMenuItem _item = new NSMenuItem((NSMenuItem)item);


                    menu.Items.Add(_item);
                }

                if (item is NSMenu)
                {
                    NSMenu _menu = new NSMenu((NSMenu)item);
                    menu.Items.Add(_menu);
                }
            }

            return menu;
        }

    }
}
