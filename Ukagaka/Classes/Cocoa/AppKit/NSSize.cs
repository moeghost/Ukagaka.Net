using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
namespace Cocoa.AppKit
{
    public class NSSize
    {
        public CGFloat width;
        public CGFloat height;


        public NSSize()
        {
            width = new CGFloat(0);
            height = new CGFloat(0); ;

        }

        public NSSize(int Width, int Height)
        {
            this.width = new CGFloat(Width);
            this.height = new CGFloat(Height);
        }

        public NSSize(CGFloat Width,CGFloat Height)
        {
            this.width = Width;
            this.height = Height;
        }

        public NSSize(float Width, float Height)
        {
            this.width = new CGFloat(Width);
            this.height = new CGFloat(Height);
        }

        public Size ToSize()
        {
            return new Size(this.width.IntValue(), this.height.IntValue());

        }

        public CGFloat Width()
        {
            return width;
        }

        public CGFloat Height()
        {
            return height;
        }
        
    }
}
