using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows;
using Cocoa.AppKit;

namespace Ukagaka
{
    public class SCCommunicateBoxController:NSWindow
    {
        private readonly SCSession _session;
        private NSTextField _textField;

        public SCCommunicateBoxController(SCSession session)
        {
            _session = session;
            InitializeComponent();
            Title = $"COMMUNICATE : {_session.GetSelfName()}";
        }

        private new void InitializeComponent()
        {
            Width = 300;
            Height = 100;
            WindowStartupLocation = WindowStartupLocation.Manual;

            var grid = new Grid();
            _textField = new NSTextField
            {
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Top
            };

            var sendButton = new NSButton
            {
                Content = "Send",
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 75
            };
            sendButton.Click += SendButton_Click;

            grid.Children.Add(_textField);
            grid.Children.Add(sendButton);
            Content = grid;
        }

        public new void Show()
        {
            if (IsVisible) return;

            // Calculate position (align with shell's top-left)
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            double shellLeft = _session.GetHontai().X();
            double shellTop = _session.GetHontai().Height();
            var comBoxLeft = shellLeft;
            var comBoxBottom = shellTop;

            if (comBoxLeft < 0)
            {
                comBoxLeft = 0;
            }
            else if (comBoxLeft + Width > screenWidth)
            {
                comBoxLeft = screenWidth - Width;
            }

            Left = comBoxLeft;
            Top = comBoxBottom - Height; // WPF origin is top-left

            // Show window on UI thread
            if (Thread.CurrentThread.Name == "main" || Thread.CurrentThread == Application.Current.Dispatcher.Thread)
            {
                base.Show();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => base.Show());
                while (!IsVisible) { Thread.Sleep(10); } // Wait for window to show
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_textField.Text)) return;

            _session.DoShioriCommunicateFromUser(_textField.Text);
            _textField.Text = "";
        }

        public new void Close()
        {
            if (Thread.CurrentThread.Name == "main" || Thread.CurrentThread == Application.Current.Dispatcher.Thread)
            {
                base.Close();
            }
            else
            {
                Application.Current.Dispatcher.Invoke(base.Close);
            }
        }
         
    }
}
