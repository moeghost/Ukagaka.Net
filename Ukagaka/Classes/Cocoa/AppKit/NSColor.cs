using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using Ukagaka.Classes.ToolKit;
using System.Windows.Media;
//using System.Windows.Media;
namespace Cocoa.AppKit
{
    using Color = System.Drawing.Color;

    public class NSColor
    {

        Color cgColor;


     

        public NSColor()
        {
           
        }

        public void Set()
        {



        }


        public Color GetColor()
        {
            return cgColor;
        }



        public System.Windows.Media.Color GetMediaColor()
        {
            return cgColor.ToMediaColor();
           
        }


        public System.Windows.Media.Brush GetBrush()
        {
            return new SolidColorBrush(GetMediaColor());

        }




        public NSColor(float r, float g, float b, float alpha)
        {
            this.cgColor = Color.FromArgb((int)(alpha * 255) % 255, (int)(r * 255) % 255, (int)(g * 255) % 255, (int)(b * 255) % 255);
        }

        public float A
        {
            get
            { 
                return cgColor.A;
            }
        }
        public float R
        {
            get
            {
                return cgColor.R;
            }
        }

        public float G
        {
            get
            {
                return cgColor.G;
            }
        }

        public float B
        {
            get
            {
                return cgColor.B;
            }
        }


      

        public static NSColor ColorWithCalibratedRGB(float r,float g,float b,float alpha)
        {
            NSColor color = new NSColor(r,g,b,alpha);
            return color;
        }


        internal static NSColor WhiteColor()
        {
            return White;
        }

        internal static NSColor BlackColor()
        {
            return Black;
        }

        internal static NSColor RedColor()
        {
            return Red;
        }

        public static NSColor ClearColor
        {

            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.White;
               
                return color;
            }




        }





        public static NSColor Black
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Black;
                return color;
            }
        }

        public static NSColor Maroon
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Maroon;
                return color;
            }
        }

        public static NSColor Brown
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Brown;
                return color;

            }

        }


        public static NSColor Clear
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.White;
                return color;

            }

        }

        public static NSColor DarkGray
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.DarkGray;
                return color;

            }

        }


        public static NSColor LightGray
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.LightGray;
                return color;

            }

        }


        public static NSColor Magenta
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Magenta;
                return color;

            }

        }



        public static NSColor Orange
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Orange;
                return color;

            }

        }




        

        public static NSColor Green
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Green;
                return color;
            }
        }


        public static NSColor Olive
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Olive;
                return color;
            }
        }
        public static NSColor Navy
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Navy;
                return color;
            }
        }
        public static NSColor Purple
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Purple;
                return color;
            }
        }

        public static NSColor Teal
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Teal;
                return color;
            }
        }


        public static NSColor Gray
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Gray;
                return color;
            }
        }
        public static NSColor Silver
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Silver;
                return color;
            }
        }
        public static NSColor Red
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Red;
                return color;
            }
        }
        public static NSColor Lime
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Lime;
                return color;
            }
        }
        public static NSColor Yellow
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Yellow;
                return color;
            }
        }
        public static NSColor Blue
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Blue;
                return color;
            }
        }
        public static NSColor Fuchsia
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Fuchsia;
                return color;
            }
        }
        public static NSColor Aqua
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.Aqua;
                return color;
            }
        }
        public static NSColor White
        {
            get
            {
                NSColor color = new NSColor();
                color.cgColor = Color.White;
                return color;
            }
        }

         


    }
}
