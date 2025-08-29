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
//using System.IO;
using System.Collections;
using Cocoa.AppKit;
using System.Drawing;
using Utils;
namespace Ukagaka
{
    public class SCSurfaceServer
    {
        SCShell shell;
        File shelldir;
        Dictionary<int, SCSurface> surfaces; // key : (Integer)ID , value : (SCSurface)サーフィス
        double ratio = 1.0;

        public SCSurfaceServer(SCShell shell, File shelldir)
        {
            surfaces = new Dictionary<int, SCSurface>();
            this.shell = shell;
            this.shelldir = shelldir;
            //loadAllSurfaces();
            if (SCFoundation.LOAD_WHOLE_SURFACES_ON_BOOTING_SURFACE_SERVER)
            {
                loadAllSurfaces();
            }
        }

        public bool hasLoadAllSurfaces()
        {
            // 全てのサーフィスをロードしてしまっていたらtrue。
            return SCFoundation.LOAD_WHOLE_SURFACES_ON_BOOTING_SURFACE_SERVER;
        }

        public void loadAllSurfaces()
        {
            SCAliasManager alias = shell.GetAliasManager();

            string[] files = shelldir.List();//Directory.GetFileSystemEntries(shelldir.FullName);



            foreach (string file in files)
            {
                File fi = new File(file);

                if (fi.IsDirectory())
                {
                    continue;
                }

                String name = fi.GetName();
                String lowercase_name = name.ToLower();
                if (lowercase_name.StartsWith("surface") &&
                (lowercase_name.EndsWith(".png") || lowercase_name.EndsWith(".dgp")))
                {
                    try
                    {
                        String id_str = StringExtension.Substring(name, 7, name.Length - 4);   //name.Substring(7, name.Length - 4);
                        loadSurface(Integer.ParseInt(id_str));
                    }
                    catch (Exception e) { }
                }
                else
                {
                    String reg_name = alias.InverseSearch(name.Substring(name.Length - 4));
                    if (reg_name != null)
                    {
                        try
                        {
                            String id_str = StringExtension.Substring(reg_name, 7, reg_name.Length);//reg_name.Substring(7, reg_name.Length);
                            loadSurface(Integer.ParseInt(id_str));
                        }
                        catch (Exception e) { }
                    }
                }
            }

            // surfaces.txtがあればelement定義をサーチする。
            SCBlockedDescription descriptions = shell.GetSurfaceDescriptions();
            if (descriptions != null)
            {
                foreach (String key in descriptions.keys())
                {


                    //   String key = (String)keys.nextElement();
                    Object obj = descriptions.get(key);
                    if (!(obj is List<String>))
                    {
                        continue;
                    }
                    List<String> def = (List<String>)obj;

                    bool found = false;
                    int n_lines = def.Count;
                    for (int i = 0; i < n_lines; i++)
                    {
                        String line = (String)def.ElementAt(i);
                        if (line.StartsWith("element"))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        String reg_name = alias.InverseSearch(key);
                        try
                        {
                            String id_str = (reg_name != null ? reg_name : key).Substring(7);
                            loadSurface(Integer.ParseInt(id_str));
                        }
                        catch (FormatException e)
                        {
                        }
                    }
                }
            }
        }

        public SCSurface findSurface(int id)
        {
            // リダイレクトは行いません。

            int intId = id;
           
            if (!surfaces.ContainsKey(intId) || surfaces[intId] == null)
            {
                // ロードされていないか、存在しない。
                loadSurface(id);
                return (SCSurface)surfaces[intId]; // 存在しなかったらnullが返されます。
            }
            else
            {
                return (SCSurface)surfaces[intId]; // 既にロードされていた。
            }
        }

        public SCSurface findSurface(int id, int type)
        {
            // リダイレクトを行います。
            SCSurface surface = findSurface(id);
            return (surface != null ? surface : findSurface(type == SCFoundation.HONTAI ? 0 : 10));
        }

        public File findSurfacePngFile(int id)
        {
            return findSurfacePngFile(shelldir, id);
        }
        public static File findSurfacePngFile(File shelldir, int id)
        {
            // surface0*${id}\.(png|dgp)を検索します。
            // 0*が空の場合、つまりsurface${id}.(png|dgp)の形式になっていた場合に最も効率良く検索を行うことが出来ます。
            // 結局見付からなかったらnullを返します。
            File pngFile = new File(shelldir,"surface" + id.ToString().PadLeft(4, '0') + ".png");
            if (pngFile.Exists())
            {
                return pngFile;
            }
            File dgpFile = new File(shelldir,"surface" + id.ToString().PadLeft(4, '0') + ".dgp");
            if (dgpFile.Exists())
            {
                return dgpFile;
            }
            string[] files = shelldir.List();//Directory.GetFileSystemEntries(shelldir.FullName);


            for (int i = 0; i < files.Length; i++)
            {
                String fname = files[i];
                String lower_fname = fname.ToLower();
                if (lower_fname.StartsWith("surface") &&
                (lower_fname.EndsWith(".png") || lower_fname.EndsWith(".dgp")))
                {
                    try
                    {
                        int id_of_current = Integer.ParseInt(StringExtension.Substring(fname,7, fname.Length - 4));     //fname.Substring(7, fname.Length - 4)
                        if (id_of_current == id)
                        {
                            return new File(files[i]);
                        }
                    }
                    catch (Exception e) { }
                }
            }
            return null;
        }

        public void loadSurface(File pngFile, int id)
        {
            SCSurface surface = new SCSurface(pngFile, id, shell.GetSurfaceDescriptions());
            surface.Resize(ratio); // 現在の比率にリサイズしておく
            surfaces[id] = surface;
        }

        public void loadSurface(int id)
        {
            // 指定された番号のサーフィスをロードします。
            // 既にロードされていたり存在しないサーフィスだったら無視します。
            // 実在する画像よりelementでの定義を優先します。
            int intId = id;

            if (surfaces.ContainsKey(intId))
            {
                if (surfaces[intId] != null)
                {
                    return;
                }
            }
            // surface${id}でエイリアスデータベースから検索し、見つかればそれを、見つからなければsurface${id}.pngファイルをロード。
            String surfaceName = shell.GetAliasManager().ResolveAlias("surface" + id);

            // elementの仮想surfaceとして定義されている可能性がある。
            SCBlockedDescription descriptions = shell.GetSurfaceDescriptions();
            if (descriptions != null && descriptions.exists(surfaceName))
            {
                Object obj = descriptions.get(surfaceName);
                if (!(obj is List<String>))
                {
                    return;
                }
                List<String> lines = (List<String>)obj;

                List<Object> elements = new List<Object>(); // <Object[]>elements : [0]=(NSImage)image, [1]=(Integer)x, [2]=(Integer)y
                int n_lines = lines.Count;
              
                for (int i = 0; i < n_lines; i++)
                {
                    String line = (String)lines.ElementAt(i);
                    if (line.StartsWith("element"))
                    {
                        try
                        {
                            int nextToken = 0;
                            string[] st = line.Split(',');

                            nextToken++; // element*
                            nextToken++; // overlay
                            String inter = st[nextToken++]; // surface*.png
                            String x_str = st[nextToken++]; // x
                            String y_str = st[nextToken++]; // y

                            // pngをロードして左上の座標で色抜き。同時にアルファチャンネルを持たせる。
                            File pngfile = new File(shelldir.GetPath() + "/" + inter);
                            if (!pngfile.Exists())
                            {
                                System.Console.WriteLine("SCSurfaceServer : File " + inter + " specified as an element was not found.");
                                continue;
                            }
                            // 在后台线程中调用：
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {

                                NSImage original_image = new NSImage();
                                if (pngfile.GetPath().ToLower().EndsWith(".dgp"))
                                {
                                    //  original_image = DGPLoader.load(pngfile);
                                }
                                else
                                {
                                    original_image = new NSImage(pngfile.GetPath(), true);
                                }
                                NSImage image = SCAlphaConverter.ConvertImage(original_image);

                                // pnaが存在するか？
                                File pnaFile = new File(SCFileUtils.StringByDeletingExtension(pngfile.GetPath()) + ".pna");
                                if (pnaFile.Exists())
                                {
                                    // pnaをロードしてアルファチャンネルにコピー
                                    NSImage pna = new NSImage(pnaFile.GetPath(), true);
                                    SCAlphaConverter.AttachAlphaToImage(pna, ref image);
                                }

                                Object[] entry = new Object[3];
                                entry[0] = image;
                                entry[1] = Integer.ParseInt(x_str);
                                entry[2] = Integer.ParseInt(y_str);
                                elements.Add(entry);
                            });
                        }
                        catch (Exception e)
                        {
                            System.Console.Write("SCSurfaceServer : exception occured in synthesizing \"element\" images.");
                            System.Console.Write("   The definition was " + line);
                            System.Console.Write(e.StackTrace);
                        }
                    }
                }

                if (elements.Count > 0)
                {
                    // 合成後の画像のサイズを求める。
                    int synth_width = 0, synth_height = 0;

                    int n_elements = elements.Count;
                    for (int i = 0; i < n_elements; i++)
                    {
                        Object[] entry = (Object[])elements.ElementAt(i);
                        NSImage image = (NSImage)entry[0];
                        int x = (int)entry[1];
                        int y = (int)entry[2];

                        int width = image.Size().Width().IntValue();
                        int height =image.Size().Height().IntValue(); ;

                        int current_width = x + width;
                        if (synth_width < current_width)
                        {
                            synth_width = current_width;
                        }
                        int current_height = y + height;
                        if (synth_height < current_height)
                        {
                            synth_height = current_height;
                        }
                    }
                    // 在后台线程中调用：
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {

                        // 画像作成
                        NSImage synthesized_image = new NSImage(new NSSize(synth_width, synth_height));

                        synthesized_image.LockFocus();
                        for (int i = 0; i < n_elements; i++)
                        {
                            Object[] entry = (Object[])elements.ElementAt(i);
                            NSImage image = (NSImage)entry[0];
                            int x = (int)entry[1];
                            int y = (int)entry[2];

                            int width = image.Size().Width().IntValue();
                            int height = image.Size().Height().IntValue(); ;
                            // g.DrawImage(image.GetBitmap(),x, synth_height - (height + y), width, height);
                            synthesized_image.CompositeToPoint(new NSPoint(x, synth_height - (height + y)), image.GetBitmap());
                            //     image.compositeToPoint(new NSPoint(x, synth_height - (height + y)), NSImage.CompositeSourceOver);
                        }
                        synthesized_image.UnlockFocus();
                        //g.Dispose();
                        SCSurface surface = new SCSurface(synthesized_image, id, lines);
                        surface.Resize(ratio); // 現在の比率にリサイズしておく
                        surfaces[id] = surface;
                    }); 
                    return;
                }
            }

            // ここに来ているという事は、elementで定義されていなかった。
            String surfaceNameFormat = shell.GetAliasManager().ResolveAlias("surface" + id.ToString().PadLeft(4,'0'));
            File pngFile = new File(shelldir.GetPath() + "/"+ surfaceNameFormat + ".png");
            File dgpFile = new File(shelldir.GetPath() + "/" + surfaceNameFormat + ".dgp");
            if (pngFile.Exists())
            {
                loadSurface(pngFile, id);
                return;
            }
            else if (dgpFile.Exists())
            {
                loadSurface(dgpFile, id);
                return;
            }
            else
            {
                pngFile = findSurfacePngFile(id);
                if (pngFile != null)
                {
                    loadSurface(pngFile, id);
                    return;
                }
            }
        }

        public void resizeSurfaces(double ratio)
        {
            this.ratio = ratio;
            foreach (int key in surfaces.Keys)
            {
                Object elem = surfaces[key];
                if (elem is SCSurface)
                {
                    ((SCSurface)elem).Resize(ratio);
                }
            }
        }

        public String toString()
        {
            StringBuilder buf = new StringBuilder();
            buf.Append("shell: {").Append(shell).Append('}');
            if (surfaces.Count > 0)
            {
                buf.Append("; surfaces: {");
                foreach (int key in surfaces.Keys)
                {
                 
                    buf.Append(key).Append(" => {").
                        Append(surfaces[key]).Append('}');
                    
                        buf.Append(", ");
                    
                }
                buf.Append('}');
            }
            buf.Append("; ratio: ").Append(ratio);
            return buf.ToString();
        }

        protected void finalize()
        {
            //Logger.log(this, Logger.DEBUG, "finalized");
        }

       

    }
}
