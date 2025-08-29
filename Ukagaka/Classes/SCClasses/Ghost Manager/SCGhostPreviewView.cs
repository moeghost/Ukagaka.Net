using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using Utils;
using Cocoa.AppKit;

namespace Ukagaka
{
    public class SCGhostPreviewView : Canvas
    {
        private NSImage _background;
        private NSImage _sakura;
        private NSImage _kero;
        private bool _isEmpty = true;
        private readonly FontFamily _messageFont = new FontFamily("Arial");
        private readonly double _messageFontSize = 10.0;
        private readonly Brush _messageColor = Brushes.Black;
        private readonly Brush _messageBGColor = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));

        public SCGhostPreviewView()
        {
            // Load default background
            var defaults = Registry.CurrentUser.OpenSubKey(@"Software\GhostPreview");
            string bgImagePath = defaults?.GetValue("display.ghostmanager.preview.filepath") as string;

            if (string.IsNullOrEmpty(bgImagePath))
            {
                bgImagePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Aqua Blue.jpg");
            }

            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => { _background = new NSImage(bgImagePath); });
            }
            catch
            {
                // Fallback to a solid color if image can't be loaded
                _background = null;
            }

            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.InvalidateVisual();
        }

        public void SetImage(File shell)
        {
            SetImage(shell.GetFullName());

        }


        public void SetImage(string shellDirPath)
        {
            _isEmpty = false;

            if (!System.IO.Directory.Exists(shellDirPath))
            {
                _sakura = _kero = null;
                this.InvalidateVisual();
                return;
            }

            // Load sakura (surface0)
            string sakuraPath = FindSurfaceImage(shellDirPath, 0);
            if (!string.IsNullOrEmpty(sakuraPath))
            {
                SetImage(sakuraPath, SCFoundation.SAKURA);
            }
            else
            {
                _sakura = null;
            }

            // Load kero (surface10)
            string keroPath = FindSurfaceImage(shellDirPath, 10);
            if (!string.IsNullOrEmpty(keroPath))
            {
                SetImage(keroPath, SCFoundation.KERO);
            }
            else
            {
                _kero = null;
            }

            this.InvalidateVisual();
        }

        private string FindSurfaceImage(string shellDir, int surfaceId)
        {
            // First try alias.txt
            File alias = new File(shellDir, "alias.txt");

            if (alias.Exists())
            {
                try
                {
                    var aliasManager = new SCAliasManager(alias);
                    string surfaceName = aliasManager.ResolveAlias($"surface{surfaceId}");
                    File pngFile = new File(shellDir, $"{surfaceName}.png");
                    if (pngFile.Exists())
                    {
                        return pngFile.GetFullName();
                    }
                }
                catch { }
            }

            // Fallback to direct search
            string pattern = $"surface{surfaceId}*.png";
            var files =System.IO.Directory.GetFiles(shellDir, pattern);
            if (files.Length > 0)
            {
                return files[0];
            }

            return null;
        }

        public void SetEmpty()
        {
            _sakura = _kero = null;
            _isEmpty = true;
            this.InvalidateVisual();
        }

        public void SetImage(string imagePath, int type)
        {
            try
            {
                NSImage image = null;
                // 在后台线程中调用：
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    image = new NSImage(imagePath); ;
                });
                // Check for PNA file
                string pnaPath = System.IO.Path.ChangeExtension(imagePath, ".pna");
                if (System.IO.File.Exists(pnaPath))
                {

                    // In WPF, we'd need to manually apply the alpha from the PNA file
                    // This would require custom image processing
                    image = SCAlphaConverter.ConvertImage(image, pnaPath);
                }

                if (type == SCFoundation.SAKURA)
                {
                    _sakura = image;
                }
                else
                {
                    _kero = image;
                }
            }
            catch
            {
                if (type == SCFoundation.SAKURA)
                {
                    _sakura = null;
                }
                else
                {
                    _kero = null;
                }
            }
        }

        private BitmapImage ApplyPnaAlpha(BitmapImage image, string pnaPath)
        {
            // This would need to be implemented to apply the PNA alpha channel
            // For now, just return the original image
            return image;
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            double width = this.ActualWidth;
            double height = this.ActualHeight;

            if (width <= 0 || height <= 0)
                return;

            // Draw background
            if (_background != null)
            {
                double bgWidth = _background.Width;
                double bgHeight = _background.Height;

                // Calculate aspect-preserving scale
                double scale = Math.Max(width / bgWidth, height / bgHeight);
                double scaledWidth = bgWidth * scale;
                double scaledHeight = bgHeight * scale;


                // Draw the background centered
                NSRect bgRect = new NSRect(
                    (width - scaledWidth) / 2,
                    (height - scaledHeight) / 2,
                    scaledWidth,
                    scaledHeight);

                _background.SetFrame(bgRect);

               // dc.DrawImage(_background, bgRect);
            }
            else
            {
                // Fallback background
                dc.DrawRectangle(Brushes.LightBlue, null, new Rect(0, 0, width, height));
            }

            if (!_isEmpty)
            {
                // Check for missing images
                string message = null;
                if (_sakura == null && _kero != null)
                {
                    message = "Missing surface id 0.";
                }
                else if (_sakura != null && _kero == null)
                {
                    message = "Missing surface id 10.";
                }
                else if (_sakura == null && _kero == null)
                {
                    message = "Missing surface both of id 0 and id 10.";
                }

                if (message != null)
                {
                    var formattedText = new FormattedText(
                        message,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(_messageFont, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                        _messageFontSize,
                        _messageColor
                     );

                    double textWidth = formattedText.Width;
                    double textHeight = formattedText.Height;

                    Rect messageRect = new Rect(
                        3,
                        height - 3 - textHeight - 4,
                        textWidth + 6,
                        textHeight + 4);

                    dc.DrawRectangle(_messageBGColor, null, messageRect);
                    dc.DrawText(formattedText, new Point(messageRect.Left + 3, messageRect.Top + 2));

                    return;
                }

                // Draw sakura and kero images
                double sakuraWidth = _sakura.Width;
                double sakuraHeight = _sakura.Height;
                double keroWidth = _kero.Width;
                double keroHeight = _kero.Height;

                // Calculate scale to fit both images
                double shellScale = width / (sakuraWidth + keroWidth);
                double scaledSakuraHeight = sakuraHeight * shellScale;
                double scaledKeroHeight = keroHeight * shellScale;

                if (scaledSakuraHeight > height || scaledKeroHeight > height)
                {
                    double baseHeight = Math.Max(sakuraHeight, keroHeight);
                    shellScale = height / baseHeight;
                }

                double scaledSakuraWidth = sakuraWidth * shellScale;
                scaledSakuraHeight = sakuraHeight * shellScale;
                double scaledKeroWidth = keroWidth * shellScale;
                scaledKeroHeight = keroHeight * shellScale;

                // Draw sakura
                NSRect sakuraRect = new NSRect(
                    0,
                    height - scaledSakuraHeight,
                    scaledSakuraWidth,
                    scaledSakuraHeight);

              //  dc.DrawImage(_sakura, sakuraRect);
                _sakura.SetFrame(sakuraRect);
                // Draw kero
                NSRect keroRect = new NSRect(
                    scaledSakuraWidth,
                    height - scaledKeroHeight,
                    scaledKeroWidth,
                    scaledKeroHeight);
                _kero.SetFrame(keroRect);
               //dc.DrawImage(_kero, keroRect);
            }
        }
    }
}
