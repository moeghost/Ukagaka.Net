using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
using System.Collections;
using Cocoa.AppKit;
using Utils;
namespace Ukagaka
{
    public class SCBalloonSkinServer
    {
        public static  int ARROW_UP = 0;
        public static  int ARROW_DOWN = 1;

        List<SCBalloonSkin> balloons; // [SCBalloonSkin]
        File dir;
        String path;
        String name;
        SCDescription desc;
        NSImage sstpMarker;
        NSImage arrow_up, arrow_down;

        public SCBalloonSkinServer(String dir_path)
        {
            // 存在しないパスを示すバルーンスキンサーバを作ろうとした場合、空のサーバーが生成されます。
            balloons = new List<SCBalloonSkin>();
            path = dir_path;

            if (dir_path == null || dir_path.Length == 0)
            {
                desc = new SCDescription();
                return;
            }

            dir = new File(SCFoundation.GetParentDirOfBundle(), dir_path);
            if (!dir.IsDirectory())
            {
                desc = new SCDescription();
                return;
            }

            desc = new SCDescription(new File(dir.GetPath(), "descript.txt"));
            name = desc.GetStrValue("name");

            File sstpmarkerfile = new File(dir.GetPath(),"sstp.png");
            File arrow0file = new File(dir.GetPath(), "arrow0.png");
            File arrow1file = new File(dir.GetPath(), "arrow1.png");

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (sstpmarkerfile.Exists())
                {
                    sstpMarker = SCAlphaConverter.ConvertImage(new NSImage(sstpmarkerfile.GetPath(), true));
                }
                if (arrow0file.Exists())
                {
                    arrow_up = SCAlphaConverter.ConvertImage(new NSImage(arrow0file.GetPath(), true));
                }
                if (arrow1file.Exists())
                {
                    arrow_down = SCAlphaConverter.ConvertImage(new NSImage(arrow1file.GetPath(), true));
                }
            });
        }

        public SCBalloonSkin findSkin(int id, int type)
        {
            // 見つからなければnullが返されます。
            // nullの場合は偽林檎デフォルトバルーン（画像を使わない）が使われます。

            // まずはロードされているバルーンスキンを検索
            int n_balloons = balloons.Count;
            for (int i = 0; i < n_balloons; i++)
            {
                SCBalloonSkin bs = (SCBalloonSkin)balloons.ElementAt(i);
                if (bs.getID() == id && bs.getType() == type) return bs;
            }


            if (dir == null)
            {
                return null;
            }

            // 見つからなかったので、ファイルが存在していればロードして返す。
            String bskinname = "balloon" + (type == SCFoundation.HONTAI ? 's' : 'k') + id;
            File bskinPng = new File(dir.GetPath(), bskinname + ".png");
            if (bskinPng.Exists())
            {
                SCBalloonSkin bskin = new SCBalloonSkin(desc, dir, bskinname, id, type);
                balloons.Add(bskin);
                return bskin;
            }

            // ファイルが存在しない。
            return null;
        }

        public String GetPath()
        {
            // home/で始まる、このバルーンスキンのディレクトリへのパスを返します。
            return path;
        }

        public SCDescription GetDescription()
        {
            return desc;
        }

        public String GetName()
        {
            return name;
        }

        public NSImage GetSstpMarkerImage()
        {
            // マーカーが無ければnullを返す。
            return sstpMarker;
        }

        public NSImage GetArrowImage(int id)
        {
            // idは定数。
            return (id == ARROW_UP ? arrow_up : arrow_down);
        }

        public String ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("name: ").Append(name);
            buf.Append("; path: ").Append(path);
            return buf.ToString();
        }

        protected void ize()
        {
          //  Logger.log(this, Logger.DEBUG, "ized");
        }


    }
}
