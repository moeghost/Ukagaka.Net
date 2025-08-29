using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;

namespace Ukagaka
{
    public class SCTeachBoxController
    {
        private readonly Window _window;
        private readonly TextBox _textField;
        private readonly SCTeachSession _teach;
        private readonly SCSession _session;
        private readonly Dispatcher _dispatcher;

        public SCTeachBoxController(SCTeachSession teach)
        {
            _teach = teach ?? throw new ArgumentNullException(nameof(teach));
            _session = teach.GetSession();
            _dispatcher = Dispatcher.CurrentDispatcher;

            // Create the window
            _window = new Window
            {
                Title = $"TEACH : {_session.GetSelfName()}",
                Width = 300,
                Height = 150,
                WindowStyle = WindowStyle.ToolWindow,
                ResizeMode = ResizeMode.NoResize
            };

            // Create UI elements
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            _textField = new TextBox { Margin = new Thickness(5) };
            Grid.SetRow(_textField, 0);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(5)
            };

            var sendButton = new Button
            {
                Content = "Send",
                Width = 80,
                Margin = new Thickness(5, 0, 0, 0)
            };
            sendButton.Click += SendButton_Click;

            buttonPanel.Children.Add(sendButton);
            Grid.SetRow(buttonPanel, 1);

            grid.Children.Add(_textField);
            grid.Children.Add(buttonPanel);

            _window.Content = grid;
        }

        public void Show()
        {
            if (_window.IsVisible) return;

            // Calculate position
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var shellLeft = _session.GetHontai().X();
            var shellTop = _session.GetHontai().Height();

            var teachboxLeft = (double)shellLeft;
            var teachboxBottom = (double)shellTop;

            if (teachboxLeft < 0)
            {
                teachboxLeft = 0;
            }
            else if (teachboxLeft + _window.Width > screenWidth)
            {
                teachboxLeft = screenWidth - _window.Width;
            }

            _window.Left = teachboxLeft;
            _window.Top = teachboxBottom - _window.Height; // Convert bottom to top position

            // Show the window on the UI thread
            if (_dispatcher.CheckAccess())
            {
                _window.Show();
            }
            else
            {
                _dispatcher.Invoke(() => _window.Show());
            }

            // Wait for window to be shown if not on UI thread
            if (Thread.CurrentThread.Name != "main")
            {
                while (!_window.IsVisible)
                {
                    Thread.Sleep(10);
                }
            }
        }

        public void Close()
        {
            if (_dispatcher.CheckAccess())
            {
                _window.Close();
            }
            else
            {
                _dispatcher.Invoke(() => _window.Close());
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_textField.Text)) return;

            _teach.MessageFromUser(_textField.Text);
            _textField.Clear();
        }
    }
}
