using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cocoa.AppKit;
using System.Drawing;
namespace Ukagaka
{
    public class SCShellLayer
    {

        SCShellWindowController swc;
        int id;
        int surfaceid;
        int seqid;
        NSImage img = null;
        NSPoint loc = null;
        bool vis = false;

        public SCShellLayer(SCShellWindowController swc, int id, SCSurfaceServer sserver, int surfaceid, int seqid, NSPoint location, bool visible)
        {
            this.swc = swc;
            this.id = id;
            this.surfaceid = surfaceid;
            this.seqid = seqid; // 重ね合わせの順序。

             
            SCSurface surf = sserver?.findSurface(surfaceid);
            img = (surf == null ? null : surf.GetImage());

            loc = location;
            vis = visible;
        }

        public void Draw()
        {
            // locの値は左上座標からのオフセットになっているので気を付けるべし。

            if (img != null && vis)
            {

                img.LockFocus();
               
                double ratio = swc.GetRatio();
                float x = (float)(loc.X().Value * ratio);
                float y = (float)(ratio * (swc.CurrentSurface().GetOriginalSize().Height().Value - loc.Y().Value - img.Size().Height().Value));

                //   g.Dispose();
               // img.SetFrame(new NSRect(x, y, img.Size().Width().Value,img.Size().Height().Value));
                
                img.CompositeToPoint(new NSPoint(x, y), img.GetBitmap());
            }
        }

        public void SetImage(SCSurfaceServer sserver, int surfaceid)
        {
            if (sserver == null)
            {
                return;
            }


            SCSurface surf = sserver.findSurface(surfaceid);
            img = (surf == null ? null : surf.GetImage());
        }


        public NSImage GetImage()
        {

            return img;
        }

        public void SetImage(NSImage img)
        {

            this.img = img;
        }



        public void SetLoc(NSPoint location)
        {
            loc = location;
        }

        public NSPoint GetLoc()
        {
            return loc;
        }


        public bool GetVis()
        {
            return vis;
        }



        public void SetVis(bool visible)
        {
            vis = visible;
        }

        public int Id()
        {
            return id;
        }

        public int Seqid()
        {
            return seqid;
        }



    }
}
