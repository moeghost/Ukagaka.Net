using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing;
using Cocoa.AppKit;
//using System.IO;
using System.Collections;
using Utils;
namespace Ukagaka
{
    public class SCSurface
    {


        /*
        public NSImage rawImage; // 、ウ、ウ、ャnull、ホ瓶、マ□ソs、オ、□ソNSImage、ャcompressedImage、ヒネ□テ、ニ、、、□」
        NSData compressedImage;
        NSSize originalSize;
        int id;
        Vector collisions; // ヨミノ□マSCSurfaceCollision。」
        double ratio;
        Map defs;
     */
        public NSImage baseImage;
        public int surfaceID = 0;
        public Dictionary<int, SCSerikoSequence> animation = new Dictionary<int, SCSerikoSequence>();
        public int[] collision;

        public SCSurface(int surfaceID, NSImage baseImage)
        {
            this.surfaceID = surfaceID;
            this.baseImage = baseImage;
        }













         

        NSImage rawImage; // ここがnullの時は圧縮されたNSImageがcompressedImageに入っている。
      //  NSData compressedImage;
        NSSize originalSize;
        int id;
        List<SCSurfaceCollision> collisions; // 中身はSCSurfaceCollision。
        double ratio;

        public SCSurface(File f, int id, SCBlockedDescription comprehensiveDefinitions)
        {
            // f : pngまたはdgpファイルへのパス
            // comprehensiveDefinitions : 当たり判定。nullなら代わりにs.txtを読む。
            this.id = id;
            if (!f.Exists())
            {
                return;
            }

            // 在后台线程中调用：
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {

                // pngをロードして左上の座標で色抜き。同時にアルファチャンネルを持たせる。
                NSImage image_original = new NSImage();
                if (f.GetPath().ToLower().EndsWith(".dgp"))
                {
                    //image_original = DGPLoader.load(f);
                }
                else
                {
                    image_original = new NSImage(f.GetPath(), true);
                }
                NSImage image = SCAlphaConverter.ConvertImage(image_original);

                // pnaが存在するか？
                File pnaFile = new File(SCFileUtils.StringByDeletingExtension(f.GetPath()) + ".pna");
                if (pnaFile.Exists())
                {
                    // pnaをロードしてアルファチャンネルにコピー
                    NSImage pna = new NSImage(pnaFile.GetPath(), true);
                    SCAlphaConverter.AttachAlphaToImage(pna, ref image);
                }

                // image.setScalesWhenResized(false);
                originalSize = image.Size();

                //int width = (int)image.Size().Width();
                //   int height = (int)image.Size().Height();

                // 圧縮
                if (SCFoundation.COMPRESS_SURFACES_ON_MEMORY)
                {
                    // compressedImage = compressImage(image);
                    // image = null;
                    //  System.gc();
                }
                else
                {
                    rawImage = image;
                }
            });
            // 当たり判定
            collisions = new List<SCSurfaceCollision>();
            if (comprehensiveDefinitions == null)
            {
                File sfile = new File(SCFileUtils.StringByDeletingExtension(f.GetPath()) + "s.txt");
                if (!sfile.Exists())
                {
                    return;
                }
                LoadCollisionsFromIndividualFile(sfile);
            }
            else
            {
                Object defs = comprehensiveDefinitions.get(SCFileUtils.StringByDeletingExtension(f.GetPath()));
                if (defs == null || !(defs is List<String>)) {
                    defs = comprehensiveDefinitions.get("surface" + id);
                }
                if (defs != null && defs is List<String>) {
                    LoadCollisions((List<String>)defs);
                }
            }
        }

        public SCSurface(NSImage image, int id, List<String> definitions)
        {
            // definitions : surfaces.txtの要素一つ
            this.id = id;

           // image.setScalesWhenResized(false);
            originalSize = image.Size();

            // 圧縮
            if (SCFoundation.COMPRESS_SURFACES_ON_MEMORY)
            {
              //  compressedImage = compressImage(image);
             //   image = null;
             //   System.gc();
            }
            else
            {
                rawImage = image;
            }

            // 当たり判定
            collisions = new List<SCSurfaceCollision>();
            LoadCollisions(definitions);
        }

        protected void LoadCollisionsFromIndividualFile(File sfile)
        {
            // s.txtファイルから当たり判定データを読む。
            try
            {
               System.IO.StreamReader sr = null;
                
                sr = new System.IO.StreamReader(new System.IO.FileStream(sfile.GetPath(), System.IO.FileMode.Open), Encoding.UTF8);

                List<String> valid_defs = new List<String>();
                while (true)
                {
                    String line = sr.ReadLine();

                    if (line == null)
                    {
                        break;
                    }
                    if (line.IndexOf(',') == -1)
                    {
                        continue;
                    }
                    if (line.StartsWith("#"))
                    {
                        continue;
                    }
                    valid_defs.Add(line);
                }
                LoadCollisions(valid_defs);

                sr.Close();
            }
            catch (Exception e)
            {
                System.Console.Write("SCSurface : exception occured in loading collision data from " + sfile.GetPath());
                System.Console.Write(e.StackTrace);
            }
        }

        protected void LoadCollisions(List<String> definitions)
        {
            // definitions : <String>当たり判定定義行の集合。
            // ただし当たり判定定義でない項目が含まれていたら正しく無視する。
            int width = rawImage.Size().Width().IntValue();
            int height =rawImage.Size().Height().IntValue();

            int n_defs = definitions.Count();
            for (int i = 0; i < n_defs; i++)
            {
                String line = (String)definitions.ElementAt(i);

                int comma_pos = line.IndexOf(',');
                if (comma_pos == -1)
                {
                    continue;
                }
                String label = line.Substring(0, comma_pos);
                String data = line.Substring(comma_pos + 1);

                // collisionのみ認識。後は捨てる。collisionは新形式／旧形式どちらでも読む。
                if (label.StartsWith("collision"))
                {
                    if (label.StartsWith("collision."))
                    {
                        // 旧タイプ
                        try
                        {
                            String name = label.Substring(label.IndexOf('.') + 1);
                            if (name.Equals("head"))
                            {
                                name = "Head";
                            }
                            else if (name.Equals("face"))
                            {
                                name = "Face";
                            }
                            else if (name.Equals("bust"))
                            {
                                name = "Bust";
                            }
                            int nextToken = 0;
                            string[] st = data.Split(',');
                            int left = Integer.ParseInt(st[nextToken++]);
                            int top = Integer.ParseInt(st[nextToken++]);
                            int right = Integer.ParseInt(st[nextToken++]);
                            int bottom = Integer.ParseInt(st[nextToken++]);

                            // 座標系の差異を考慮します。
                            collisions.Add(new SCSurfaceCollision(new NSRect(left, height - bottom, right - left, bottom - top), name));
                        }
                        catch (Exception e) { continue; }
                    }
                    else
                    {
                        // 新タイプ

                        int nextToken = 0;
                        string[] st = data.Split(',');
                        try
                        {
                            int left = Integer.ParseInt(st[nextToken++]);
                            int top = Integer.ParseInt(st[nextToken++]);
                            int right = Integer.ParseInt(st[nextToken++]);
                            int bottom = Integer.ParseInt(st[nextToken++]);

                            String name = st[nextToken++];

                            // 座標系の差異を考慮します。
                            //System.out.println(left+","+(height-bottom)+","+(right-left)+","+(bottom-top)+" : "+name);
                            collisions.Add(new SCSurfaceCollision(new NSRect(left, height - bottom, right - left, bottom - top), name));
                        }
                        catch (Exception e) { continue; }
                    }
                }
            }
        }
        /*
        protected static NSData compressImage(NSImage src)
        {
            NSBitmapImageRep rep = (NSBitmapImageRep)src.representations().objectAtIndex(0);
            NSMutableData result = new NSMutableData();
            byte[] header = new byte[10]; // 4:height 4:width 2:spp
            int width = rep.pixelsWide();
            int height = rep.pixelsHigh();
            int spp = rep.samplesPerPixel();
            header[0] = (byte)((width >> 24) & 0x000000ff);
            header[1] = (byte)((width >> 16) & 0x000000ff);
            header[2] = (byte)((width >> 8) & 0x000000ff);
            header[3] = (byte)((width) & 0x000000ff);
            header[4] = (byte)((height >> 24) & 0x000000ff);
            header[5] = (byte)((height >> 16) & 0x000000ff);
            header[6] = (byte)((height >> 8) & 0x000000ff);
            header[7] = (byte)((height) & 0x000000ff);
            header[8] = (byte)((spp >> 8) & 0x000000ff);
            header[9] = (byte)((spp) & 0x000000ff);
            result.AppendData(new NSData(header));
            result.AppendData(new NSData(rep.bitmapData()));
            return result;
        }

        protected static NSImage decompressImage(NSData src)
        {
            byte[] header = src.bytes(0, 10); // 4:height 4:width 2:spp
            int width = header[0] << 24 | header[1] << 16 | header[2] << 8 | header[3];
            int height = header[4] << 24 | header[5] << 16 | header[6] << 8 | header[7];
            int spp = header[8] << 8 | header[9];
            NSImage image = new NSImage();
            NSBitmapImageRep bitmap = new NSBitmapImageRep(
                width,
                height,
                spp,
                4,
                true,
                false,
                NSGraphics.CalibratedRGBColorSpace,
                0,
                0);
            bitmap.setBitmapData(src.bytes(10, src.length() - 10));
            image.addRepresentation(bitmap);
            return image;
        }
        */
        public NSImage GetImage()
        {
            if (rawImage == null)
            {
                // compresed
                /*  NSImage decompressedImage = decompressImage(compressedImage);

                  NSSize newsize = new NSSize((float)(originalSize.Width() * ratio), (float)(originalSize.Height() * ratio));
                  if (ratio > 1.0) decompressedImage.setSize(newsize);

                  NSImageRep rep = (NSImageRep)decompressedImage.representations().objectAtIndex(0);
                  rep.setSize(newsize);

                  return decompressedImage;
                  */
                return null;

            }
            else
            {
                return rawImage;
            }
        }

        public int GetID()
        {
            return id;
        }

        public NSSize GetOriginalSize()
        {
            return originalSize;
        }

        public void Resize(double ratio)
        {
            this.ratio = ratio;
            if (rawImage == null)
            {
                return;
            }
            NSSize newsize = new NSSize((float)(originalSize.Width().IntValue() * ratio), (float)(originalSize.Height().IntValue() * ratio));
           // if (ratio > 1.0)
            {
                rawImage.SetSize(newsize);
            }


          //  NSImageRep rep = (NSImageRep)rawImage.representations().objectAtIndex(0);
         //   rep.setSize(newsize);
        }

        public String GetCollisionNameAt(NSPoint pt)
        {
            // 見つからなければnullを返す。
            int n_cols = collisions.Count;
            for (int i = 0; i < n_cols; i++)
            {
                SCSurfaceCollision col = (SCSurfaceCollision)collisions.ElementAt(i);

                if (col.Rect().ContainsPoint(pt))
                {
                    return col.Name();
                }
            }
            return null;
        }

        public String ToString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("rawImage: {").Append(
                rawImage.ToString().Trim()).Append('}');
            buf.Append("; id: ").Append(id);
            buf.Append("; ratio: ").Append(ratio);
            return buf.ToString();
        }

        protected void Finalize()
        {
            //Logger.log(this, Logger.DEBUG, "finalized");
        }

     

    }
}
