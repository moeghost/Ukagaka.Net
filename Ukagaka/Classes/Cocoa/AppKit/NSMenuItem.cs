using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ukagaka;

namespace Cocoa.AppKit
{
    public class NSMenuItem: MenuItem
    {

        private string _title;
        private bool _enabled = true;
        private int _state;
        private NSMenuItem _submenu;
        private object _target;
        private NSSelector _action;
        private bool _isProtocolSeparator = false;

        public NSMenuItem() : this("", null, null) { }

        public NSMenuItem(ItemCollection item)
        {
           
            Items.Add(item);
             
        }
        public NSMenuItem(NSMenuItem item)
        {
            Header = item.Header.ToString();
             foreach (var _item in item.Items)
            {

                Items.Add(_item);

            }

        }


        public NSMenuItem(string title) : this(title, null, null) { }

        public NSMenuItem(string title, NSSelector action, string keyEquivalent)
        {
            _title = title;
            _action = action;
            Header = title;
            
            if (!string.IsNullOrEmpty(keyEquivalent))
            {
                InputGestureText = keyEquivalent;
            }
        }

        internal void SetAction(NSSelector selector)
        {
            _action = selector;
            if (selector != null)
            {
                Click += (s, e) => selector.Invoke();
            }
        }

        internal NSSelector GetAction()
        {
            return _action;

        }


        internal void SetTarget(object target)
        {
            _target = target;
        }

        internal void SetState(int state)
        {
            _state = state;
            // WPF 中没有直接对应的状态，可以用CheckMark来表示
            IsChecked = state != 0;
        }

        internal void SetEnabled(bool enabled)
        {
            _enabled = enabled;
            base.IsEnabled = enabled;
        }

        internal static NSMenuItem ProtocolSeparatorItem()
        {
            var separator = new NSMenuItem
            {
                _isProtocolSeparator = true,
                 
            };
            separator.SetEnabled(false);
             
            // 设置特殊样式以区别于普通分隔符
            var border = new Border
            {
                Height = 8,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0, 1, 0, 0),
                BorderBrush = Brushes.Gray,
                Margin = new Thickness(0, 4, 0, 4)
            };

            separator.Template = new ControlTemplate(typeof(MenuItem))
            {
                VisualTree = new FrameworkElementFactory(typeof(ContentPresenter))
            };
            separator.Header = border;

            return separator;
        }
        internal bool IsProtocolSeparatorItem() => _isProtocolSeparator;


        // 修改现有的SeparatorItem方法以区分两种分隔符
        internal static NSMenuItem SeparatorItem()
        {
            var separator = new NSMenuItem
            {
                IsSeparatorItem = true
            };

            // 普通分隔符样式
            var border = new Border
            {
                Height = 1,
                Background = Brushes.LightGray,
                Margin = new Thickness(2, 0, 2, 0)
            };

            separator.Template = new ControlTemplate(typeof(MenuItem))
            {
                VisualTree = new FrameworkElementFactory(typeof(ContentPresenter))
            };
            separator.Header = border;

            return separator;
        }


        internal void SetSubmenu(NSMenu submenu)
        {

             
            Items.Clear();
            if (submenu != null)
            {

                var menu = new NSMenu(submenu);

                foreach (var item in menu.Items)
                {

                    Items.Add(item); // 现在可以安全添加
                }
            }
        }

        internal void SetSubmenu(NSMenuItem submenu)
        {
             
            _submenu = submenu;


            Items.Clear();
            if (submenu != null)
            {

                var menu = new NSMenuItem(submenu);
                 
                foreach (var item in menu.Items)
                {
                    
                    Items.Add(item); // 现在可以安全添加
                }
            }
        }

        internal void SetTitle(string title)
        {
            _title = title;
            Header = title;
        }

        internal bool IsSeparatorItem { get; private set; }

        internal bool HasSubmenu()
        {
            return _submenu != null || Items.Count > 0;
        }


        internal bool IsEnabled()
        {
            return _enabled;
        }
    }
}
