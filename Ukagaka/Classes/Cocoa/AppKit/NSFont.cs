using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
namespace Cocoa.AppKit
{
    public class NSFont
    {
        Font font;

        public NSRect BoundingRectForFont;
        private string name;
        private float size;




        public NSFont()
        {

            font = new Font("Arial", 15);
        }

        public float Size
        {
            get
            {
                return size;
            }
             
        }


        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }




        public NSFont(float size)
        {
            this.BoundingRectForFont = new NSRect(new NSPoint(), new NSSize(size,size));
            font = new Font("Arial", size);
        }

        public NSFont(string name, float size):this(size)
        {
            this.name = name;
            this.size = size;
            font = new Font(name, size);
        }

        public static NSFont SystemFontOfSize(float size)
        {
            return new NSFont(size);
        }
         
        public static NSFont FontWithNameAndSize(string name, float size)
        {
            return new NSFont(name, size);
        }


    }
}
