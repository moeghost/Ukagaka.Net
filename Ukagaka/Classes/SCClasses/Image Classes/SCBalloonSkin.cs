using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using System.Collections;
using Cocoa.AppKit;

namespace Ukagaka
{
    public class SCBalloonSkin
    {
        NSImage image;
        NSRect frame;
        int type; // SCFoundation.HOONTAI or SCFoundation.UNYUU
        int id;
        SCDescription desc;

        public SCBalloonSkin(SCDescription parentDesc,File  dir, String bskinname, int id, int type)
        {
            // dir : バルーンのディレクトリ
            // bskinname : 例えばballoons0やballoonk1
            this.id = id;
            this.type = type;
            File pngfile =new File(dir.GetPath(), bskinname + ".png");
            if (!pngfile.Exists())
            {
                return;
            }
            // 在后台线程中调用：
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                image = SCAlphaConverter.ConvertImage(new NSImage(pngfile.GetPath(), true));
            });
            frame = new NSRect(NSPoint.ZeroPoint, image.Size());

            desc = new SCDescription(parentDesc, new File(dir.GetPath(), bskinname + "s.txt"));
            //desc.setTag(dir.ToString());
        }

        public NSImage getImage()
        {
            return image;
        }

        public NSRect getFrame()
        {
            return frame;
        }

        public int getID()
        {
            return id;
        }

        public int getType()
        {
            return type;
        }

        public SCDescription getDescription()
        {
            return desc;
        }

        public String toString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("id: ").Append(id);
            buf.Append("; type: ").Append(type);
            return buf.ToString();
        }

        protected void finalize()
        {
            //Logger.log(this, Logger.DEBUG, "finalized");
        }

    }
}
