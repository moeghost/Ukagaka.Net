using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Drawing;
using Cocoa.AppKit;
namespace Ukagaka
{

   // using NSColor = Color;
    //using NSFont = Font;
   // using NSRect = Rectangle;
   // using NSMutableAttributedString = Attribute;
   // using NSRange = Rangr
    public class SCBalloonAlternative:NSView
    {
        protected String refcon;
        protected Object target;
        protected NSSelector selector;

        String title;
        NSMutableAttributedString normal_str, selected_str;
        NSColor pen_color, brush_color;


        bool draw_underline, draw_square, draw_underline_even_if_unselected;

        bool mouse_cursor_is_on_this_view = false;

        public SCBalloonAlternative(String title, NSFont font, NSColor normal_font_color, NSColor selected_font_color,
                    NSColor pen_color, NSColor brush_color, bool draw_underline, bool draw_square,
                    bool draw_underline_even_if_unselected)
        {
            // draw_underline_even_if_unselectedがtrueであれば、カーソルが乗せられていなくてもアンダーラインを表示。
            // ただしdraw_underlineがtrueになっている場合のみ。
            this.title = title;
            this.pen_color = pen_color;
            this.brush_color = brush_color;
            this.draw_underline = draw_underline;
            this.draw_square = draw_square;
            this.draw_underline_even_if_unselected = draw_underline_even_if_unselected;

            if (selected_font_color == null)
            { // これだけはnullでも良い。

                selected_font_color = NSColor.ColorWithCalibratedRGB(
                    normal_font_color.A,
                    1 - normal_font_color.R,
                    1 - normal_font_color.G,
                    1 - normal_font_color.B
                    );
               
            }

         //   NSRange range_all = new NSRange(0, title.length());
           /* normal_str = new NSMutableAttributedString()
            normal_str.addAttributeInRange(NSAttributedString.FontAttributeName, font, range_all);

            selected_str = new NSMutableAttributedString(normal_str); // copy

            normal_str.addAttributeInRange(NSAttributedString.ForegroundColorAttributeName, normal_font_color, range_all);
            selected_str.addAttributeInRange(NSAttributedString.ForegroundColorAttributeName, selected_font_color, range_all);

            setFrame(new NSRect(NSPoint.ZeroPoint, NSGraphics.sizeOfAttributedString(normal_str)));*/
        }

        public String Title()
        {
            return title;
        }

        internal void SetAction(NSSelector selector)
        {
            this.selector = selector;
        }

        internal void SetTarget(SCSafetyBalloonController sCSafetyBalloonController)
        {
            this.target = sCSafetyBalloonController;
            //throw new NotImplementedException();
        }

        internal void SetRefcon(string param)
        {
            this.refcon = param;
           // throw new NotImplementedException();
        }

        internal string GetRefcon()
        {
            return this.refcon;
        }

        internal void SetToolTip(string id)
        {
            throw new NotImplementedException();
        }

        private NSRect[] rect_list_contais_bounds = null;
        private NSRect rect_underline;


        /*
        public void drawRect(NSRect r)
        {
            if (rect_list_contais_bounds == null)
                rect_list_contais_bounds = new NSRect[] { bounds() };
            if (rect_underline == null)
            {
                NSRect bounds = bounds();
                rect_underline = new NSRect(0, 0, bounds.Width(), 1);
            }

            if (mouse_cursor_is_on_this_view && draw_square)
            {
                brush_color.set();
                NSGraphics.fillRectList(rect_list_contais_bounds);
            }

            NSGraphics.drawAttributedString(mouse_cursor_is_on_this_view ? selected_str : normal_str, NSPoint.ZeroPoint);

            if (mouse_cursor_is_on_this_view && draw_square)
            {
                pen_color.set();
                NSGraphics.frameRectWithWidth(bounds(), 1.0f);
            }

            //System.out.println("draw_underline = "+draw_underline);
            //  System.out.println("draw_underline_even_if_unselected = "+draw_underline_even_if_unselected);
           //   System.out.println("mouse_cursor_is_on_this_view = "+mouse_cursor_is_on_this_view);
            if (draw_underline &&
                (draw_underline_even_if_unselected ||
                 mouse_cursor_is_on_this_view))
            {
                pen_color.set();
                NSGraphics.frameRect(rect_underline);
            }
        }

        public bool acceptsFirstMouse(NSEvent theEvent)
        {
            return true;
        }

        public void mouseDown(NSEvent event) {
            try
            {
                selector.invoke(target, this);
            }
            catch (Exception e)
            {
                System.err.println("BallonAlternative: exception occured.");
                System.err.println("  target was " + target.getClass().GetName());
                e.printStackTrace();
            }
        }

        // javaからでは何故かトラッキング領域を定義出来ない。このコードが無意味になることを願う。
        private bool mouse_entered_already = false;
        public void mouseMoved(NSEvent theEvent)
        {
            NSPoint mouse_point;
            bool is_mouse_in;
            mouse_point = theEvent.locationInWindow();
            mouse_point = convertPointFromView(mouse_point, null);
            is_mouse_in = isMouseInRect(mouse_point, bounds());  //Viewの内部か

            if (is_mouse_in && !mouse_entered_already)
            {
                this.mouseEntered(theEvent);
                mouse_entered_already = true;
            }
            else if (!is_mouse_in && mouse_entered_already)
            {
                this.mouseExited(theEvent);
                mouse_entered_already = false;
            }
            //base.mouseMoved(theEvent);
        }

        public void mouseEntered(NSEvent event) {
            mouse_cursor_is_on_this_view = true;
            display();
        }

        public void mouseExited(NSEvent event) {
            mouse_cursor_is_on_this_view = false;
            display();
        }

        public void setAction(NSSelector sel)
        {
            selector = sel;
        }

        public void setTarget(Object o)
        {
            target = o;
        }

        public String getRefcon()
        {
            return refcon;
        }

        public void setRefcon(String refcon)
        {
            this.refcon = refcon;
        }
        */
    }
}
