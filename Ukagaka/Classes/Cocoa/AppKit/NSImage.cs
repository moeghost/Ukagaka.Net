using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
namespace Cocoa.AppKit
{
    public class NSImage : System.Windows.Controls.Image
    {
        // CGImage cgImage;

        private Image image;
        public static int CompositeCopy = 1;
        NSSize size;
        Bitmap baseImage;
        BitmapImage backupImage;
        Bitmap backup;
        public static int CompositeSourceOver = 1;

        public NSImage()
        {
            this.size = new NSSize();
    
        }

        public NSImage(Image Image)
        {

            this.image = Image;
            baseImage = CGImage.ToBitmap(Image);
            size = new NSSize(baseImage.Size.Width, baseImage.Size.Height);
            backupImage = CGImage.ToBitmapImage(baseImage);
            this.Source = backupImage;
        }

        public NSImage(Bitmap BaseImage)
        {
            this.baseImage = BaseImage;
            size = new NSSize(BaseImage.Size.Width, BaseImage.Size.Height);
            backupImage = CGImage.ToBitmapImage(BaseImage);
            this.Source = backupImage;
        }




        public NSImage(NSSize size)
        {
            this.size = size;
            try
            {
                baseImage = new Bitmap(size.Width().IntValue(), size.Height().IntValue());
                backupImage = CGImage.ToBitmapImage(baseImage);
                this.Source = backupImage;
            }
            catch 
            { 

            }
        }

        public NSImage(string path)
        {
            image = Image.FromFile(path);
            baseImage = CGImage.ToBitmap(image);
            size = new NSSize(baseImage.Size.Width, baseImage.Size.Height);
            backupImage = CGImage.ToBitmapImage(baseImage);
            this.Source = backupImage;
        }


        public NSImage(string path,bool Bool)
        {
            image = Image.FromFile(path);
            baseImage = CGImage.ToBitmap(image);
            size = new NSSize(baseImage.Size.Width, baseImage.Size.Height);
            backupImage = CGImage.ToBitmapImage(baseImage);
            this.Source = backupImage;
        }
        

        public NSSize Size()
        {

           // Size = new NSSize(BaseImage.Size.Width, BaseImage.Size.Height);
            return size;
 
        }

        


        public Image GetImage()
        {
            return image;
             
        }

        public Bitmap GetBitmap()
        {
            return baseImage;

        }
        public BitmapImage GetOriginBitmap()
        {
            return CGImage.ToBitmapImage(baseImage);
        }



        public BitmapImage GetBitmapImage()
        {
         

            return CGImage.ToBitmapImage(backup);
        }

        public void LockFocus()
        {
            if (baseImage == null)
            {
                return;
            }
            backup = (Bitmap)baseImage.Clone();
            //return Graphics.FromImage(backup);
        }

        private Graphics GetGraphics()
        {
            if (backup == null)
            {
                LockFocus();
               
            }

            if (backup == null)
            {
                return null;
            }

            return Graphics.FromImage(backup);
        }




        public void UnlockFocus()
        {

        }

        public void SetSize(NSSize size)
        {
            LockFocus();

            Graphics g = GetGraphics();
            if (g == null)
            {
                return;
            }


            g.DrawImage(GetBitmap(), 0, 0, size.Width().IntValue(), size.Height().IntValue());

            g.Dispose();
            this.size = size;
        }


        public void SetFrame(NSRect rect)
        {
            LockFocus();

            Graphics g = GetGraphics();
            if (g == null)
            {
                return;
            }


            g.DrawImage(GetBitmap(), rect.X().Value, rect.Y().Value, rect.Width().Value, rect.Height().Value);

            g.Dispose();
            this.size = rect.Size();
        }


        public void CompositeToPointFromRect(NSPoint point,NSRect rect,int CompositeSource)
        {
            LockFocus();

            Graphics g = GetGraphics();
            if (g == null)
            {
                return;
            }

            g.DrawImage(GetBitmap(), rect.X().Value, rect.Y().Value, rect.Width().Value, rect.Height().Value);

            g.Dispose();

        }

        public void CompositeToPointFromRect(NSPoint point, NSRect rect, Bitmap CompositeSource)
        {
            
            Graphics g = GetGraphics();
            if (g == null)
            {
                return;
            }

            g.DrawImage(CompositeSource, point.X().Value, point.Y().Value);

            g.Dispose();

        }


        public void CompositeToPoint(NSPoint point,Bitmap CompositeSource)
        {

            Graphics g = GetGraphics();
            if (g == null)
            {
                return;
            }

            g.DrawImage(CompositeSource, point.X().Value, point.Y().Value);

            g.Dispose();

        }

        internal void DissolveToPointFromRect(NSPoint nSPoint, NSRect r, float v)
        {
            throw new NotImplementedException();
        }
    }
}
