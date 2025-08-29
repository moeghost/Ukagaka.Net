using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ukagaka;

namespace Cocoa.AppKit
{
    public class NSMenuView : NSView
    {
        NSMenu menu;

        public NSMenuView()
        {
            menu = new NSMenu();


        }


        internal int ImageAndTitleOffset()
        {
            throw new NotImplementedException();
        }

        internal int StateImageOffset()
        {
            throw new NotImplementedException();
        }


        public bool IsFlipped()
        {
            return false;
        }

        public float StateImageOffSet()
        {
            return 0;
        }

        public float ImageAndTitleOffSet()
        {
            return 0;
        }

        public NSMenuItemCell MenuItemCellForItemAtIndex(int index)
        {
            return new NSMenuItemCell();
        }

        public NSRect RectOfItemAtIndex(int index)
        {
            return new NSRect();
        }

        internal void SetMenuItemCellForItemAtIndex(SCContextMenuItemCell mic, int i)
        {
             
        }

        internal NSMenu Menu()
        {
            return menu;
        }

        internal int KeyEquivalentOffset()
        {
            return 0;
        }
    }
}
