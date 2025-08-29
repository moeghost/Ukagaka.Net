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
    public class CGImage
    {

        public static Bitmap ToBitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }



        //=======================================================
        //获取图像pic的遮罩图像
        //=======================================================

        //将png透明图像，转化为一张不含透明度的jpeg图像 和 一张仅含透明度的png图像
        public static Bitmap[] getPicMask(Image pic)
        {
            Bitmap[] tmp = new Bitmap[2];
            tmp[0] = ToBitmap(pic);
            tmp[1] = ToBitmap(pic);

            tmp[0] = getRGB(tmp[0]);    //获取除透明度信息外的图像
            tmp[1] = getAlpha(tmp[1]);  //获取图像的透明度图像

            return tmp;
        }

        //获取图像对应的RGB图像，透明度数据清除
        public static Bitmap getRGB(Bitmap pic)
        {
            Color C;
            for (int i = 0; i < pic.Height; i++)
            {
                for (int j = 0; j < pic.Width; j++)
                {
                    C = pic.GetPixel(j, i);
                    C = Color.FromArgb(0, C.R, C.G, C.B);   //清除透明度信息

                    pic.SetPixel(j, i, C);
                }
            }
            return pic;
        }

        //获取图像的遮罩图像，仅保留透明度信息
        public static Bitmap getAlpha(Bitmap pic)
        {
            Color C;
            for (int i = 0; i < pic.Height; i++)
            {
                for (int j = 0; j < pic.Width; j++)
                {
                    C = pic.GetPixel(j, i);
                    C = Color.FromArgb(0, C.A, C.A, C.A);   //使用透明度信息生成遮罩图像

                    pic.SetPixel(j, i, C);
                }
            }
            return pic;
        }


        public static BitmapImage CreateImageMask(BitmapImage Pic, BitmapImage Mask)
        {
           
            return ToBitmapImage(CreateImageMask(ToBitmap(Pic), ToBitmap(Mask)));
             
        }




        //=======================================================
        //为图像pic的添加遮罩图像
        //=======================================================
        public static Bitmap CreateImageMask(Image Pic, Image Mask)
        {
            if (Pic == null)
            {
                return null;
            }
            Bitmap pic = ToBitmap(Pic);
            if (Mask == null)
            {
                return pic;
            }
            Bitmap mask = ToBitmap(Mask);
            if (pic.Width != mask.Width || pic.Height != mask.Height)
            {
                return pic;
            }
            Color C, C2;
            for (int i = 0; i < pic.Height; i++)
            {
                for (int j = 0; j < pic.Width; j++)
                {
                    C = pic.GetPixel(j, i);                    //读取原图像的RGB值
                    C2 = mask.GetPixel(j, i);                  //读取蒙板的透明度信息

                    //if (C2.R == 0) C = Color.FromArgb(0, 0, 0, 0);  // Color.Empty;
                    if (C2.R == 0) C = Color.Transparent;
                    else C = Color.FromArgb(C2.R, C.R, C.G, C.B);   //清除透明度信息

                    pic.SetPixel(j, i, C);
                }
            }
            return pic;

        }

        //Image转化为Bitamap
        public static Bitmap ToBitmap(Image pic)
        {
            //创建图像
            Bitmap tmp = new Bitmap(pic.Width, pic.Height);                //按指定大小创建位图
            Rectangle Rect = new Rectangle(0, 0, pic.Width, pic.Height);   //pic的整个区域

            //绘制
            Graphics g = Graphics.FromImage(tmp);                   //从位图创建Graphics对象
            g.Clear(Color.FromArgb(0, 0, 0, 0));                    //清空

            g.DrawImage(pic, Rect, Rect, GraphicsUnit.Pixel);       //从pic的给定区域进行绘制

            return tmp;     //返回构建的新图像
        }

        #region 合并图片
        /// <summary>
        /// 合并图片
        /// </summary>
        /// <param name="maps"></param>
        /// <returns></returns>
        public static Bitmap MergerImg(params Bitmap[] maps)
        {
            int i = maps.Length;
            if (i == 0)
            {
                throw new Exception("图片数不能够为0");
            }
            //创建要显示的图片对象,根据参数的个数设置宽度
            Bitmap backgroudImg = new Bitmap(i * 12, 16);
            Graphics g = Graphics.FromImage(backgroudImg);

            //清除画布,背景设置为白色
            g.Clear(System.Drawing.Color.White);

            for (int j = 0; j < i; j++)
            {
                g.DrawImage(maps[j], j * 11, 0, maps[j].Width, maps[j].Height);
            }
            g.Dispose();
            return backgroudImg;
        }
        #endregion
    }
}
