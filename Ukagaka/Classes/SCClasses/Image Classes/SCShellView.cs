using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Cocoa.AppKit;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
namespace Ukagaka
{

    public class SCShellView :NSView
    {


        SCShellWindowController swc;
        NSImage baseImage;
        List<SCShellLayer> layers;

        bool isNeedsDisplay;


        public SCShellView(NSRect r)//:base(r)
        {
            // super(r);
            swc = null;
            baseImage = null;
            //   baseImage = new NSImage();
            layers = new List<SCShellLayer>();
           
           // AddSubview(baseImage);
        }

        public void Initialize(SCShellWindowController swc)
        {
            this.swc = swc;
        }

        public void CleanUp()
        {
            swc = null;
            baseImage = null;
            layers.Clear();
        }
       
        public bool AcceptsFirstMouse(NSEvent theEvent)
        {
            return true;
        }
    
        public void DrawRect()
        {
            if (baseImage == null)
            {
               // NSColor.clearColor().set();
               // NSGraphics.fillRectList(new NSRect[] { r });
            }
            else
            {
                // base.compositeToPoint(NSPoint.ZeroPoint, NSImage.CompositeCopy);

                // ImageDrawing baseDrawing = new ImageDrawing(baseImage.GetBitmapImage(),new Rect());


                //Graphics g = baseImage.LockFocus();
                baseImage.LockFocus();
                
                int n_layers = layers.Count();
                for (int i = 0; i < n_layers; i++)
                {
                    try
                    {
                        SCShellLayer layer = (SCShellLayer)layers.ElementAt(i);
                        if (layer == null)
                        {

                            continue;
                        }
                        if (layer.GetImage() != null && layer.GetVis())
                        {
                            layer.Draw();
                            // g.DrawImage(layer.GetImage().GetBitmap(), layer.getLoc().ToPoint());

                            baseImage.CompositeToPointFromRect(layer.GetLoc(), new NSRect(), layer.GetImage().GetBitmap());
                        }
                        
                    }
                    catch(Exception e)
                    {
                        System.Console.WriteLine("Shell layer Draw Rect Exception:",e);

                    }
                }

                baseImage.UnlockFocus();
            }
        }
              
        public void ChangeSurface(NSImage img)
        {
            // imgはnullでも良く、その場合はサーフィスが消えます。
            baseImage = img;
        }

        public override void Display()
        {
            //  BitmapImage image = new BitmapImage(new Uri("pack://SiteOfOrigin:,,,/ghost/Taromati2/shell/Remilia Scarlet/surface0000.png"));//打开图片
            //    BitmapImage mask = new BitmapImage(new Uri("pack://SiteOfOrigin:,,,/ghost/Taromati2/shell/Remilia Scarlet/surface0000.pna"));//打开图片



            //  this.Source = CGImage.CreateImageMask(image, mask);
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (isNeedsDisplay)
                {

                    this.Image.Source = baseImage?.GetBitmapImage();
                    //  this.baseImage.Source = 
                    //  this.Source = baseImage.GetBitmapImage();
                }
                else
                {
                    this.Image.Source = baseImage?.GetOriginBitmap();
                    //  this.baseImage.Source = baseImage.GetOriginBitmap();
                    //   this.Source = baseImage.GetBitmapImage();
                    //  this.Source = baseImage.GetOriginBitmap();
                }

            }));
          //  this.Source = 
        }

        
        public SCShellLayer AddLayer(SCSurfaceServer sserver, int surfaceid, int seqid, NSPoint loc, bool visible)
        {
            // レイヤーをシーケンスIDに応じた位置に挿入し、そのレイヤーを返します。

            // 使用可能なIDを探す
            int id = 0;
            int n_layers = layers.Count;
            for (int i = 0; i < n_layers; i++)
            {
                for (int j = 0; j < n_layers; j++)
                {
                    try
                    {
                        SCShellLayer shellLayer = (SCShellLayer)layers.ElementAt(j);

                        if (shellLayer?.Id() == id)
                        {
                            id++;
                        }
                    }
                    catch
                    {


                    }
                }
            }
            SCShellLayer layer =
                new SCShellLayer(swc, id, sserver, surfaceid, seqid, loc, visible);

            // 挿入
            bool inserted_flag = false;
            for (int i = 0; i < n_layers; i++)
            {
                // このシーケンスIDより大きな値を持ったレイヤーが現れたら、その真下に挿入する。
                SCShellLayer l = (SCShellLayer)layers.ElementAt(i);

                if (l?.Seqid() > seqid)
                {
                    layers.Insert(i, layer);
                    inserted_flag = true;
                    break;
                }
            }
            if (!inserted_flag)
            { // 挿入する条件を満たしていなかったら
              // 単純に一番上に乗っける。
                layers.Add(layer);
            }
            return layer;
        }
         

        public void SetNeedsDisplay(bool value)
        {
            isNeedsDisplay = value;
            if (value)
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    DrawRect();
                }));
            }

        }

        public void RemoveLayer(SCShellLayer l)
        {
            // レイヤーを削除します。
            int id = l.Id();

            int n_layers = layers.Count;
            List<SCShellLayer> newLayers = new List<SCShellLayer>();



            for (int i = 0; i < layers.Count; i++)
            {

                try
                {
                    SCShellLayer layer = (SCShellLayer)layers.ElementAt(i);

                    if (layer?.Id() == id)
                    {

                        layers.Remove(layer);

                        i--;

                    }
                }
                catch(Exception e)
                {


                }

                //  n_layers = layers.Count;
            }

        }


        public override void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);

             

            // 获取菜单制作器
            var menu  = MenuForEvent(null);

             
            if (menu != null)
            {
                menu.Placement = PlacementMode.MousePoint;
                menu.IsOpen = true;
                e.Handled = true; // 阻止事件继续传递
            }




        }



        public ContextMenu MenuForEvent(NSEvent e)
        { // OSから呼ばれます。
            if (swc.Session().IsInPassiveMode())
            {
                return null;
            }
            SCShellContextMenuMaker scmm = swc.Session().GetShellContextMenuMaker();

            scmm.ReconstructMenu(swc.GetType());
            return scmm.GetMenu(swc.GetType());
        }
        
    }

     
}
