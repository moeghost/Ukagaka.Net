using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ukagaka;

namespace Cocoa.AppKit
{
    public class NSMenuItemCell:NSCell
    {
        NSImage image;

        NSMenuItem menuItem;

        NSFont font;

        NSMenuView menuView;






        string title;


        public NSMenuItemCell()
        {
            image  = new NSImage();

            menuItem = new NSMenuItem();

            font = new NSFont();

            menuView = new NSMenuView();
        }


        internal NSSize CellSize()
        {
            return Size();
        }

        internal virtual void DrawWithFrameInView(NSRect region, NSView view)
        {
           // throw new NotImplementedException();
        }

        public NSImage Image()
        {
            return image;
        }

        public NSFont Font()
        {
            return font;
        }
    
        public string Title()
        {
             return title;
        }


        public void DrawBorderAndBackgroundWithFrameInView(NSRect r, NSView view)
        {
           //throw new NotImplementedException();
        }

        public NSMenuItem MenuItem()
        {
             return menuItem;
        }


        public NSMenuView MenuView()
        {
            return menuView;
        }

        public void SetMenuItem(NSMenuItem menuItem)
        {
            this.menuItem = menuItem;
        }

        public NSRect TitleRectForBounds(NSRect rect)
        {
            return new NSRect(rect.Origin(), rect.Size());
        }


        public virtual void DrawSeparatorItemWithFrameInView(NSRect r, NSView view)
        {
           // throw new NotImplementedException();
        }

        public virtual void DrawImageWithFrameInView(NSRect r, NSView view)
        {
            //throw new NotImplementedException();
        }

        public virtual void DrawKeyEquivalentWithFrameInView(NSRect r, NSView view)
        {
            //throw new NotImplementedException();
        }

        public virtual void DrawStateImageWithFrameInView(NSRect r, NSView view)
        {
            //throw new NotImplementedException();
        }

        public virtual void DrawTitleWithFrameInView(NSRect r, NSView view)
        {
           // throw new NotImplementedException();
        }
    }
}
