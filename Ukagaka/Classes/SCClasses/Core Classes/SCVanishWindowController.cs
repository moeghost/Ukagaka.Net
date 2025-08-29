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
    public class SCVanishWindowController
    {
        private readonly Window _window;
        private readonly SCSession _session;
        private readonly Dispatcher _dispatcher;
        private readonly TextBlock _messageText;

        public SCVanishWindowController(SCSession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _dispatcher = Dispatcher.CurrentDispatcher;

            // Create the vanish window
            _window = new Window
            {
                Title = SCStringsServer.GetStrFromMainDic("vanishdialog.title", new[] { _session.GetSelfName() }),
                Width = 400,
                Height = 200,
                WindowStyle = WindowStyle.ToolWindow,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.Manual
            };

            // Create UI layout
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Message text
            _messageText = new TextBlock
            {
                Text = SCStringsServer.GetStrFromMainDic("vanishdialog.message", new[] { _session.GetSelfName() }),
                Margin = new Thickness(20),
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(_messageText, 0);

            // Button panel
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10)
            };

            // Cancel button
            var cancelButton = new Button
            {
                Content = "Cancel",
                Width = 100,
                Margin = new Thickness(5)
            };
            cancelButton.Click += (sender, e) => Cancel();

            // Vanish button
            var vanishButton = new Button
            {
                Content = "Vanish",
                Width = 100,
                Margin = new Thickness(5)
            };
            vanishButton.Click += (sender, e) => Vanish();

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(vanishButton);
            Grid.SetRow(buttonPanel, 1);

            grid.Children.Add(_messageText);
            grid.Children.Add(buttonPanel);

            _window.Content = grid;
        }

        public void Show()
        {
            if (_window.IsVisible) return;

            // Trigger OnVanishSelecting event
            _session.DoShioriEvent("OnVanishSelecting");

            // Center window on screen
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            var windowWidth = _window.Width;
            var windowHeight = _window.Height;

            _window.Left = (screenWidth - windowWidth) / 2;
            _window.Top = (screenHeight - windowHeight) / 2;

            // Show window on UI thread
            if (_dispatcher.CheckAccess())
            {
                _window.Show();
            }
            else
            {
                _dispatcher.Invoke(() => _window.Show());
            }

            // Wait for window to show if not on UI thread
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

        public Window Window()
        {

            return _window;
        }

        private void Cancel()
        {
            Close();
            _session.DoShioriEvent("OnVanishCancel");
        }

        private void Vanish()
        {
            Close();
            SCFoundation.SharedFoundation().StartVanishSession(_session);
        }
    }
}
