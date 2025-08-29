using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;
using Cocoa.AppKit;
namespace Ukagaka
{
    public class SCAlphaConverter
    {

        public static NSImage ConvertImage(NSImage src)
        {
            return src;
             
        }
        public static NSImage ConvertImage(NSImage src,string pnaPath)
        {
            //NSImage dest = new NSImage(pnaPath);

          //  NSImage image = new NSImage(dest.GetImage());
           // NSImage mask = new NSImage(src.GetImage());

           // dest = new NSImage(CGImage.CreateImageMask(image.GetImage(), mask.GetImage()));

            return src;
        }

        public static void AttachAlphaToImage(NSImage src, ref NSImage dest)
        {
            NSImage image = new NSImage(dest.GetImage());
            NSImage mask = new NSImage(src.GetImage());
             
           dest = new NSImage(CGImage.CreateImageMask(image.GetImage(), mask.GetImage()));

        }


    }
}
