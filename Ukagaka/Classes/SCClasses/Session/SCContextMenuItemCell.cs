using Cocoa.AppKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Ukagaka
{
    public class SCContextMenuItemCell : MenuItem
    {
        public const int ALIGN_LEFT = 1;
        public const int ALIGN_RIGHT = 2;

        private readonly SCSession _session;
        private readonly int _type; // SCFoundation.HONTAI or SCFoundation.UNYUU
        private readonly int _textAlignment;
        private readonly SCMenuAppearanceServer _mas;

        public SCContextMenuItemCell(SCSession session, int type)
        {
            _session = session;
            _type = type;
            _textAlignment = ALIGN_LEFT;
            _mas = session.GetMenuAppearance();
            ApplyAppearance();
        }

        public SCContextMenuItemCell(SCSession session, int type, int textAlignment)
        {
            _session = session;
            _type = type;
            _textAlignment = textAlignment;
            _mas = session.GetMenuAppearance();
            ApplyAppearance();
        }

        private void ApplyAppearance()
        {
            // Create custom template
            var template = new ControlTemplate(typeof(MenuItem));
            var factory = new FrameworkElementFactory(typeof(Grid));

            // Background/border
            var backgroundFactory = new FrameworkElementFactory(typeof(Border));
            backgroundFactory.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            backgroundFactory.SetValue(Border.BorderBrushProperty, Brushes.Transparent);

            // Content presenter
            var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentFactory.SetValue(ContentPresenter.ContentProperty, new TemplateBindingExtension(ContentPresenter.ContentProperty));
            contentFactory.SetValue(ContentPresenter.ContentTemplateProperty, new TemplateBindingExtension(ContentPresenter.ContentTemplateProperty));

            factory.AppendChild(backgroundFactory);
            factory.AppendChild(contentFactory);
            template.VisualTree = factory;

            this.Template = template;
            this.Style = CreateMenuItemStyle();
        }

        private Style CreateMenuItemStyle()
        {
            var style = new Style(typeof(MenuItem));

            // Highlight background
            style.Setters.Add(new Setter(BackgroundProperty, new DynamicResourceExtension(SystemColors.MenuBrushKey)));
            style.Triggers.Add(new Trigger
            {
                Property = IsHighlightedProperty,
                Value = true,
                Setters =
                {
                    new Setter(BackgroundProperty, _mas.GetBgFontColor()),
                    new Setter(ForegroundProperty, _mas.GetFgFontColor())
                }
            });

            // Disabled state
            style.Triggers.Add(new Trigger
            {
                Property = IsEnabledProperty,
                Value = false,
                Setters = { new Setter(ForegroundProperty, _mas.GetDisabledColor()) }
            });

            // Separator style
            var separatorStyle = new Style(typeof(Separator));
            separatorStyle.Setters.Add(new Setter(BackgroundProperty, _mas.GetSeparatorColor()));
            style.RegisterName("SeparatorStyle", separatorStyle);

            return style;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_mas.HasImage())
            {
                // Draw custom background image based on alignment
                var image = IsHighlighted ? _mas.GetFgImage() : _mas.GetBgImage();
                var alignment = IsHighlighted ? _mas.GetAlignmentOfFg() : _mas.GetAlignmentOfBg();
                var bottomRightColor = IsHighlighted ? _mas.GetBottomRightColorOfFgImage() : _mas.GetBottomRightColorOfBgImage();
                var topLeftColor = IsHighlighted ? _mas.GetTopLeftColorOfFgImage() : _mas.GetTopLeftColorOfBgImage();

                var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
                DrawBackgroundImage(drawingContext, image.GetBitmapImage(), alignment, bounds, bottomRightColor.GetBrush(), topLeftColor.GetBrush());
            }
        }

        private void DrawBackgroundImage(DrawingContext dc, ImageSource image, int alignment, Rect bounds, Brush bottomRightColor, Brush topLeftColor)
        {
            var imageWidth = image.Width;
            var imageHeight = image.Height;
            var viewWidth = bounds.Width;
            var viewHeight = bounds.Height;

            switch (alignment)
            {
                case SCMenuAppearanceServer.ALIGN_LEFTTOP:
                    {
                        // Calculate source and destination rectangles for left-top alignment
                        var destX = bounds.Left;
                        var destY = bounds.Top;
                        var srcWidth = Math.Min(imageWidth, bounds.Width);
                        var srcHeight = Math.Min(imageHeight, bounds.Height);

                        var srcRect = new Rect(0, 0, srcWidth, srcHeight);
                        var destRect = new Rect(destX, destY, srcWidth, srcHeight);

                        // Draw the image portion
                        dc.DrawImage(image, destRect);

                        // Fill right margin if needed
                        if (bounds.Width > imageWidth)
                        {
                            var rightRect = new Rect(
                                bounds.Left + imageWidth,
                                bounds.Top,
                                bounds.Width - imageWidth,
                                bounds.Height);
                            dc.DrawRectangle(bottomRightColor, null, rightRect);
                        }

                        // Fill bottom margin if needed
                        if (bounds.Height > imageHeight)
                        {
                            var bottomRect = new Rect(
                                bounds.Left,
                                bounds.Top + imageHeight,
                                bounds.Width,
                                bounds.Height - imageHeight);
                            dc.DrawRectangle(bottomRightColor, null, bottomRect);
                        }
                        break;
                    }

                case SCMenuAppearanceServer.ALIGN_RIGHTTOP:
                    {
                        // Calculate source and destination rectangles for right-top alignment
                        var destX = Math.Max(bounds.Left, bounds.Right - imageWidth);
                        var destY = bounds.Top;
                        var srcWidth = Math.Min(imageWidth, bounds.Width);
                        var srcHeight = Math.Min(imageHeight, bounds.Height);

                        var srcX = (destX > bounds.Left) ? 0 : imageWidth - bounds.Width;
                        var srcRect = new Rect(srcX, 0, srcWidth, srcHeight);
                        var destRect = new Rect(destX, destY, srcWidth, srcHeight);

                        // Draw the image portion
                        dc.DrawImage(image, destRect);

                        // Fill left margin if needed
                        if (bounds.Width > imageWidth)
                        {
                            var leftRect = new Rect(
                                bounds.Left,
                                bounds.Top,
                                bounds.Width - imageWidth,
                                bounds.Height);
                            dc.DrawRectangle(bottomRightColor, null, leftRect);
                        }

                        // Fill bottom margin if needed
                        if (bounds.Height > imageHeight)
                        {
                            var bottomRect = new Rect(
                                bounds.Left,
                                bounds.Top + imageHeight,
                                bounds.Width,
                                bounds.Height - imageHeight);
                            dc.DrawRectangle(bottomRightColor, null, bottomRect);
                        }
                        break;
                    }

                case SCMenuAppearanceServer.ALIGN_LEFTBOTTOM:
                    {
                        // Calculate source and destination rectangles for left-bottom alignment
                        var destX = bounds.Left;
                        var destY = Math.Max(bounds.Top, bounds.Bottom - imageHeight);
                        var srcWidth = Math.Min(imageWidth, bounds.Width);
                        var srcHeight = Math.Min(imageHeight, bounds.Height);

                        var srcY = imageHeight - srcHeight;
                        var srcRect = new Rect(0, srcY, srcWidth, srcHeight);
                        var destRect = new Rect(destX, destY, srcWidth, srcHeight);

                        // Draw the image portion
                        dc.DrawImage(image, destRect);

                        // Fill right margin if needed
                        if (bounds.Width > imageWidth)
                        {
                            var rightRect = new Rect(
                                bounds.Left + imageWidth,
                                bounds.Top,
                                bounds.Width - imageWidth,
                                bounds.Height);
                            dc.DrawRectangle(topLeftColor, null, rightRect);
                        }

                        // Fill top margin if needed
                        if (bounds.Height > imageHeight)
                        {
                            var topRect = new Rect(
                                bounds.Left,
                                bounds.Top,
                                bounds.Width,
                                bounds.Bottom - imageHeight);
                            dc.DrawRectangle(topLeftColor, null, topRect);
                        }
                        break;
                    }

                case SCMenuAppearanceServer.ALIGN_RIGHTBOTTOM:
                    {
                        // Calculate source and destination rectangles for right-bottom alignment
                        var destX = Math.Max(bounds.Left, bounds.Right - imageWidth);
                        var destY = Math.Max(bounds.Top, bounds.Bottom - imageHeight);
                        var srcWidth = Math.Min(imageWidth, bounds.Width);
                        var srcHeight = Math.Min(imageHeight, bounds.Height);

                        var srcX = (destX > bounds.Left) ? 0 : imageWidth - bounds.Width;
                        var srcY = imageHeight - srcHeight;
                        var srcRect = new Rect(srcX, srcY, srcWidth, srcHeight);
                        var destRect = new Rect(destX, destY, srcWidth, srcHeight);

                        // Draw the image portion
                        dc.DrawImage(image, destRect);

                        // Fill left margin if needed
                        if (bounds.Width > imageWidth)
                        {
                            var leftRect = new Rect(
                                bounds.Left,
                                bounds.Top,
                                bounds.Width - imageWidth,
                                bounds.Height);
                            dc.DrawRectangle(bottomRightColor, null, leftRect);
                        }

                        // Fill top margin if needed
                        if (bounds.Height > imageHeight)
                        {
                            var topRect = new Rect(
                                bounds.Left,
                                bounds.Top,
                                bounds.Width,
                                bounds.Bottom - imageHeight);
                            dc.DrawRectangle(topLeftColor, null, topRect);
                        }
                        break;
                    }
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            InvalidateVisual();
        }

        public void SetMenuItem(MenuItem item)
        {
            if (item == null)
            {
                return;
            }
            // Set basic properties
            this.Header = item.Header;
            this.IsEnabled = item.IsEnabled;
            this.Icon = item.Icon;
            this.Command = item.Command;
            this.CommandParameter = item.CommandParameter;
            this.InputGestureText = item.InputGestureText;

            // Copy all menu items if it's a parent menu
            if (item.Items.Count > 0)
            {
                this.Items.Clear();
                foreach (var childItem in item.Items)
                {
                    if (childItem is MenuItem childMenuItem)
                    {
                        var newChild = new SCContextMenuItemCell(_session, _type, _textAlignment);
                        newChild.SetMenuItem(childMenuItem);
                        this.Items.Add(newChild);
                    }
                    else if (childItem is Separator separator)
                    {
                        this.Items.Add(new SCContextMenuSeparator(_mas));
                    }
                }
            }

            // Copy event handlers
            foreach (var handler in GetClickEventHandlers(item))
            {
                this.Click += handler;
            }

            // Apply custom appearance
            ApplyMenuItemSpecificAppearance(item);
        }

        private IEnumerable<RoutedEventHandler> GetClickEventHandlers(MenuItem item)
        {
            // This is a workaround since we can't directly access the event invocation list
            // In practice, you might need to use reflection or maintain your own handler registry
            var handlers = new List<RoutedEventHandler>();

            // For demonstration - in real implementation you'd need proper event handler tracking
            if (item.Command != null)
            {
                handlers.Add((s, e) => item.Command.Execute(item.CommandParameter));
            }

            return handlers;
        }

        private void ApplyMenuItemSpecificAppearance(MenuItem item)
        {
            // Apply any custom appearance settings from the source menu item
            if (item.Tag is Dictionary<string, object> appearanceSettings)
            {
                if (appearanceSettings.TryGetValue("Foreground", out var foreground))
                {
                    this.Foreground = foreground as Brush;
                }

                if (appearanceSettings.TryGetValue("Background", out var background))
                {
                    this.Background = background as Brush;
                }

                // Add more appearance properties as needed
            }

            // Special handling for checkable items
            if (item.IsCheckable)
            {
                var checkBoxFactory = new FrameworkElementFactory(typeof(CheckBox));
                checkBoxFactory.SetValue(CheckBox.IsCheckedProperty, item.IsChecked);
                checkBoxFactory.SetValue(CheckBox.IsEnabledProperty, item.IsEnabled);
                checkBoxFactory.SetValue(CheckBox.VerticalAlignmentProperty, VerticalAlignment.Center);

                this.Icon = new ControlTemplate(typeof(CheckBox))
                {
                    VisualTree = checkBoxFactory
                };
            }
        }

    }

    public class SCContextMenuSeparator : Separator
    {
        private readonly SCMenuAppearanceServer _mas;

        public SCContextMenuSeparator(SCMenuAppearanceServer mas)
        {
            _mas = mas;
            this.Background = _mas.GetBgFontColor().GetBrush();
            this.Height = 1;
            this.Margin = new Thickness(0, 2, 0, 2);
        }
    }













    /*
    public class SCContextMenuItemCell: NSMenuItemCell
    {
         
        public static  int ALIGN_LEFT = 1;
        public static  int ALIGN_RIGHT = 2;

        SCSession session;
        int type; // SCFoundation.HONTAI || SCFoundation.UNYUU
        int text_alignment;
        SCMenuAppearanceServer mas;

        public SCContextMenuItemCell(SCSession session, int type)
        {
            //super();
            this.session = session;
            this.type = type;
            text_alignment = ALIGN_LEFT;

            mas = session.GetMenuAppearance();
        }

        public SCContextMenuItemCell(SCSession session, int type, int text_alignment)
        {
           // super();
            this.session = session;
            this.type = type;
            this.text_alignment = text_alignment;

            mas = session.GetMenuAppearance();
        }

        public NSSize cellSize()
        {
            NSSize size = base.CellSize();
            if (Image() == null)
            {
                return size;
            }
            else
            {
                if (Image().Size().Height() > Size().Height())
                {
                    return new NSSize(Size().Width(), Image().Size().Height());
                }
                else
                {
                    return size;
                }
            }
        }

        NSMutableRect _r = null;
        public void DrawWithFrameInView(NSRect r, NSView view)
        {
            if (!mas.HasImage())
            {
                base.DrawBorderAndBackgroundWithFrameInView(r, view);
                if (MenuItem().IsSeparatorItem)
                {
                    base.DrawSeparatorItemWithFrameInView(r, view);
                }
                else
                {
                    base.DrawImageWithFrameInView(r, view);
                    base.DrawKeyEquivalentWithFrameInView(r, view);
                    base.DrawStateImageWithFrameInView(r, view);
                    base.DrawTitleWithFrameInView(r, view);
                }
            }
            else
            {
                if (_r == null)
                {
                    _r = new NSMutableRect();
                }
                _r.SetOrigin(r.Origin());
                _r.SetSize(r.Size());
                _r.SetX(_r.X().IntValue() + ((SCContextMenuView)MenuView()).GetSidebarWidth());
                _r.SetWidth(_r.Width().IntValue() - ((SCContextMenuView)MenuView()).GetSidebarWidth());

                DrawBorderAndBackgroundWithFrameInView(_r, view);
                if (MenuItem().IsSeparatorItem)
                {
                    DrawSeparatorItemWithFrameInView(_r, view);
                }
                else
                {
                    DrawImageWithFrameInView(_r, view);
                    DrawKeyEquivalentWithFrameInView(_r, view);
                    DrawStateImageWithFrameInView(_r, view);
                    DrawTitleWithFrameInView(_r, view);
                }
            }
        }

        public void DrawBorderAndBackgroundWithFrameInView(NSRect r, NSView view)
        {
            if (!IsHighlighted()) return;

            NSImage image = (IsHighlighted() ? mas.GetFgImage() : mas.GetBgImage());
            int alignment = (IsHighlighted() ? mas.GetAlignmentOfFg() : mas.GetAlignmentOfBg());
            NSColor bottomRightColor = (IsHighlighted() ? mas.GetBottomRightColorOfFgImage() : mas.GetBottomRightColorOfBgImage());
            NSColor topLeftColor = (IsHighlighted() ? mas.GetTopLeftColorOfFgImage() : mas.GetTopLeftColorOfBgImage());

            CGFloat widthOfView = view.Frame.Width();
            CGFloat heightOfView = view.Frame.Height();
            CGFloat widthOfImage = image.Size().Width();
            CGFloat heightOfImage = image.Size().Height();

            switch (alignment)
            {
                case SCMenuAppearanceServer.ALIGN_LEFTTOP:
                    {
                        CGFloat destX = r.X();
                        CGFloat destY = r.Y();
                        CGFloat srcWidth = (widthOfImage > r.Width() ? r.Width() : widthOfImage);
                        CGFloat srcHeight = (destY + r.Height() > heightOfImage ? destY + r.Height() - heightOfImage : r.Height());
                        if (view.IsFlipped() && srcHeight < r.Height())
                        {
                            destY += r.Height() - srcHeight;
                        }

                        if (srcHeight.Value > 0)
                        {
                            NSPoint destPoint = new NSPoint(destX, (view.IsFlipped() ? destY + srcHeight : destY));

                            CGFloat srcX = new CGFloat(0);
                            CGFloat srcY = (view.IsFlipped() ? heightOfImage - destY - srcHeight : r.Y() - (heightOfView - heightOfImage));
                            NSRect srcRect = new NSRect(srcX, srcY, srcWidth, srcHeight);

                            image.CompositeToPointFromRect(destPoint, srcRect, NSImage.CompositeCopy);
                        }

                        // EĚ]
                        if (r.Width() > widthOfImage)
                        {
                            NSRect rectToFill = new NSRect(r.X() + srcWidth,
                                        r.Y(),
                                        r.Width() - widthOfImage,
                                        r.Height());
                            bottomRightColor.Set();
                            NSBezierPath.FillRect(rectToFill);
                        }

                        // şĚ]
                        if (destY + r.Height() > heightOfImage)
                        {
                            NSRect rectToFill = new NSRect(r.X(),
                                        destY,
                                        r.Width(),
                                        r.Y() + r.Height() - destY);
                            bottomRightColor.Set();
                            NSBezierPath.FillRect(rectToFill);
                        }
                    }
                    break;

                case SCMenuAppearanceServer.ALIGN_RIGHTTOP:
                    {
                        CGFloat destX = (widthOfImage > r.Width() ? r.X() : r.X() + (r.Width() - widthOfImage));
                        CGFloat destY = r.Y();
                        CGFloat srcWidth = (widthOfImage > r.Width() ? r.Width() : widthOfImage);
                        CGFloat srcHeight = (destY + r.Height() > heightOfImage ? destY + r.Height() - heightOfImage : r.Height());
                        if (view.IsFlipped() && srcHeight < r.Height())
                        {
                            destY += r.Height() - srcHeight;
                        }

                        if (srcHeight.Value > 0)
                        {
                            NSPoint destPoint = new NSPoint(destX, (view.IsFlipped() ? destY + srcHeight : destY));

                            CGFloat srcX = (destX > r.X() ? new CGFloat(0) : widthOfImage - r.Width());
                            CGFloat srcY = (view.IsFlipped() ? heightOfImage - destY - srcHeight : r.Y() - (heightOfView - heightOfImage));
                            NSRect srcRect = new NSRect(srcX, srcY, srcWidth, srcHeight);

                            image.CompositeToPointFromRect(destPoint, srcRect, NSImage.CompositeCopy);
                        }

                        // śĚ]
                        if (r.Width() > widthOfImage)
                        {
                            NSRect rectToFill = new NSRect(r.X(),
                                        r.Y(),
                                        r.Width() - widthOfImage,
                                        r.Height());
                            bottomRightColor.Set();
                            NSBezierPath.FillRect(rectToFill);
                        }

                        // şĚ]
                        if (destY + r.Height() > heightOfImage)
                        {
                            NSRect rectToFill = new NSRect(r.X(),
                                        destY,
                                        r.Width(),
                                        r.Y() + r.Height() - destY);
                            bottomRightColor.Set();
                            NSBezierPath.FillRect(rectToFill);
                        }
                    }
                    break;

                case SCMenuAppearanceServer.ALIGN_LEFTBOTTOM:
                    {
                        CGFloat destX = r.X();
                        CGFloat destY = r.Y();
                        CGFloat srcWidth = (widthOfImage > r.Width() ? r.Width() : widthOfImage);
                        CGFloat srcHeight = (destY < heightOfView - heightOfImage ? heightOfView - heightOfImage - destY : r.Height());

                        if (srcHeight.Value > 0)
                        {
                            NSPoint destPoint = new NSPoint(destX, destY + srcHeight);

                            CGFloat srcX = new CGFloat(0);
                            CGFloat srcY = heightOfView - (destY + srcHeight);
                            NSRect srcRect = new NSRect(srcX, srcY, srcWidth, srcHeight);

                            image.CompositeToPointFromRect(destPoint, srcRect, NSImage.CompositeCopy);
                        }

                        // EĚ]
                        if (r.Width() > widthOfImage)
                        {
                            NSRect rectToFill = new NSRect(destX + srcWidth,
                                        r.Y(),
                                        r.Width() - widthOfImage,
                                        r.Height());
                            topLeftColor.Set();
                            NSBezierPath.FillRect(rectToFill);
                        }

                        // ăĚ]
                        if (destY < heightOfView - heightOfImage)
                        {
                            NSRect rectToFill = new NSRect(destX,
                                        destY,
                                        r.Width(),
                                        r.Y() + r.Height() - destY);
                            topLeftColor.Set();
                            NSBezierPath.FillRect(rectToFill);
                        }
                    }
                    break;

                case SCMenuAppearanceServer.ALIGN_RIGHTBOTTOM:
                    {
                        CGFloat destX = (widthOfImage > r.Width() ? r.X() : r.X() + (r.Width() - widthOfImage));
                        CGFloat destY = r.Y();
                        CGFloat srcWidth = (widthOfImage > r.Width() ? r.Width() : widthOfImage);
                        CGFloat srcHeight = (destY < heightOfView - heightOfImage ? heightOfView - heightOfImage - destY : r.Height());

                        if (srcHeight.Value > 0)
                        {
                            NSPoint destPoint = new NSPoint(destX, destY + srcHeight);

                            CGFloat srcX = (destX > r.X() ? new CGFloat(0) : widthOfImage - r.Width());
                            CGFloat srcY = heightOfView - (destY + srcHeight);
                            NSRect srcRect = new NSRect(srcX, srcY, srcWidth, srcHeight);

                            image.CompositeToPointFromRect(destPoint, srcRect, NSImage.CompositeCopy);
                        }

                        // śĚ]
                        if (r.Width() > widthOfImage)
                        {
                            NSRect rectToFill = new NSRect(r.X(),
                                        r.Y(),
                                        r.Width() - widthOfImage,
                                        r.Height());
                            bottomRightColor.Set();
                            NSBezierPath.FillRect(rectToFill);
                        }

                        // ăĚ]
                        if (destY < heightOfView - heightOfImage)
                        {
                            NSRect rectToFill = new NSRect(r.X(),
                                        destY,
                                        r.Width(),
                                        r.Y() + r.Height() - destY);
                            topLeftColor.Set();
                            NSBezierPath.FillRect(rectToFill);
                        }
                    }
                    break;


                default:
                    break;
            }
        }

        public override void DrawTitleWithFrameInView(NSRect r, NSView view)
        {
            NSColor color = (IsHighlighted() ? mas.GetFgFontColor() : (MenuItem().IsEnabled() ? mas.GetBgFontColor() : mas.GetDisabledColor()));

            NSMutableAttributedString astr = new NSMutableAttributedString(Title());
            NSRange range_whole = new NSRange(0, Title().Length);
            astr.AddAttributeInRange(NSAttributedString.FontAttributeName, Font(), range_whole);
            astr.AddAttributeInRange(NSAttributedString.ForegroundColorAttributeName, color, range_whole);
            NSSize strSize = NSGraphics.SizeOfAttributedString(astr);

            if (text_alignment == ALIGN_LEFT)
            {
                NSGraphics.DrawAttributedString(astr, TitleRectForBounds(r).Origin());
            }
            else
            {
                NSPoint destPoint = new NSPoint(MenuView().KeyEquivalentOffset() - 5 - strSize.Width().Value,
                                r.Y().Value + (r.Height().Value / 2.0f) - (strSize.Height().Value / 2.0f));
                NSGraphics.DrawAttributedString(astr, destPoint);
            }
        }


        public void DrawSeparatorItemWithFrameInView(NSRect r, NSView view)
        {
            mas.GetSeparatorColor().Set();
            NSBezierPath.SetDefaultLineWidth(1.0f);
            NSBezierPath.SetDefaultLineCapStyle(NSBezierPath.LineCapStyleButt);
            NSBezierPath.StrokeLineFromPoint(new NSPoint(r.X().Value, r.Y().Value + (r.Height().Value / 2.0f)), new NSPoint(r.MaxX().Value, r.Y().Value + (r.Height().Value / 2.0f)));
        }

        public void DrawKeyEquivalentWithFrameInView(NSRect r, NSView view)
        {
            if (!MenuItem().HasSubmenu()) return;

            NSColor color = (IsHighlighted() ? mas.GetFgFontColor() : (MenuItem().IsEnabled() ? mas.GetBgFontColor() : mas.GetDisabledColor()));

            NSMutableAttributedString astr = new NSMutableAttributedString("\u25b6");
            NSRange range_whole = new NSRange(0, 1);
            astr.AddAttributeInRange(NSAttributedString.FontAttributeName, NSFont.SystemFontOfSize(10.0f), range_whole);
            astr.AddAttributeInRange(NSAttributedString.ForegroundColorAttributeName, color, range_whole);
            //NSSize strSize = NSGraphics.SizeOfAttributedString(astr);

            NSGraphics.DrawAttributedString(astr, KeyEquivalentRectForBounds(r).Origin());
        }
 
    }


    */

}
