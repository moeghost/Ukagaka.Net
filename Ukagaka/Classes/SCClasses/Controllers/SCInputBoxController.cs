using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Windows;

namespace Ukagaka
{
    public class SCInputBoxController : Window
    {
        private readonly SCInputBoxSession inputboxSession;
        private readonly SCSession session;
        private readonly TextBox textField;
        private readonly Button sendButton;

        public SCInputBoxController(SCInputBoxSession inputboxSession)
        {
            this.inputboxSession = inputboxSession;
            this.session = inputboxSession.GetSession();

            Title = "INPUT : " + session.GetSelfName();
            Width = 300;
            Height = 150;
            WindowStartupLocation = WindowStartupLocation.Manual;
            ResizeMode = ResizeMode.NoResize;

            // === UI构造 ===
            var grid = new Grid
            {
                Margin = new Thickness(10)
            };

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            textField = new TextBox
            {
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            Grid.SetRow(textField, 0);
            grid.Children.Add(textField);

            sendButton = new Button
            {
                Content = "OK",
                Height = 30,
                Margin = new Thickness(0, 10, 0, 0)
            };
            sendButton.Click += Send;
            Grid.SetRow(sendButton, 1);
            grid.Children.Add(sendButton);

            Content = grid;
        }

        /// <summary>
        /// Java 的 showWindow(sender)
        /// </summary>
        public void ShowWindow(object sender = null)
        {
            // === 位置计算 ===
            int screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            int shellLeft = (int)session.GetHontai().X();
            int shellTop = (int)session.GetHontai().Height();

            int inputBoxLeft = shellLeft;
            int inputBoxBottom = shellTop;

            if (inputBoxLeft < 0)
            {
                inputBoxLeft = 0;
            }
            else if (inputBoxLeft + (int)Width > screenWidth)
            {
                inputBoxLeft = screenWidth - (int)Width;
            }

            Left = inputBoxLeft;
            Top = inputBoxBottom;

            Show();
            Activate();
        }

        /// <summary>
        /// Java 的 show()
        /// </summary>
        public void ShowInputBox()
        {
            if (IsVisible) return;

            // WPF的DispatcherTimer相当于NSTimer
            var openTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(0)
            };
            openTimer.Tick += (s, e) =>
            {
                openTimer.Stop();
                ShowWindow();
            };
            openTimer.Start();

            if (!Application.Current.Dispatcher.CheckAccess())
            {
                // 等待窗口显示
                while (!IsVisible) { }
            }
        }

        /// <summary>
        /// Java 的 close(sender)
        /// </summary>
        public void CloseInputBox(object sender = null)
        {
            Dispatcher.Invoke(Close);
        }

        /// <summary>
        /// Java 的 send(sender)
        /// </summary>
        private void Send(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textField.Text)) return;

            inputboxSession.MessageFromUser(textField.Text);
            textField.Clear();
        }
    }
}
