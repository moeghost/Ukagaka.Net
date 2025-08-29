using Cocoa.AppKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Utils;
namespace Ukagaka
{
    public class SCMenuAppearanceServer
    {
        public const int ALIGN_LEFTTOP = 1;
        public const int ALIGN_RIGHTTOP = 2;
        public const int ALIGN_LEFTBOTTOM = 3;
        public const int ALIGN_RIGHTBOTTOM = 4;
        public const int ALIGN_TOP = 1000;
        public const int ALIGN_BOTTOM = 1001;

        protected static Hashtable ALIGN_TABLE = GetAlignTable();



        private static Hashtable GetAlignTable()
        {
            Hashtable ALIGN_TABLE = new Hashtable();
            ALIGN_TABLE["lefttop"] = ALIGN_LEFTTOP;
            ALIGN_TABLE["righttop"] = ALIGN_RIGHTTOP;
            ALIGN_TABLE["leftbottom"] = ALIGN_LEFTBOTTOM;
            ALIGN_TABLE["rightbottom"] = ALIGN_RIGHTBOTTOM;
            ALIGN_TABLE["top"] = ALIGN_TOP;
            ALIGN_TABLE["bottom"] = ALIGN_BOTTOM;
            return ALIGN_TABLE;
        }


        SCSession session;

        bool hasImage = false;
        NSImage bgImage;
        NSImage fgImage;
        NSImage sakuraSideImage;
        NSImage keroSideImage;
        NSColor bgFontColor;
        NSColor fgFontColor;
        NSColor disabledColor;
        NSColor separatorColor;
        int align_bg;
        int align_fg;
        int align_side;
        NSColor topLeftColorOfSakuraSideImage;
        NSColor topLeftColorOfKeroSideImage;
        NSColor bottomRightColorOfSakuraSideImage;
        NSColor bottomRightColorOfKeroSideImage;
        NSColor topLeftColorOfBgImage;
        NSColor topLeftColorOfFgImage;
        NSColor bottomRightColorOfBgImage;
        NSColor bottomRightColorOfFgImage;

        public SCMenuAppearanceServer(SCSession session)
        {
            this.session = session;

            SCDescription desc;
            File dir;

            SCDescription shell_desc = session.GetCurrentShell().GetDescManager();
            if (shell_desc.Exists("menu.foreground.bitmap.filename") ||
                new File(session.GetCurrentShell().GetRootDir(), "menu_background.png").Exists())
            { // シェル側にあると見做す。
                desc = shell_desc;
                dir = session.GetCurrentShell().GetRootDir();
            }
            else
            { // ゴースト側にあると見做す。
                desc = session.GetMasterSpirit().GetDescManager();
                dir = session.GetMasterSpirit().GetRootDir();
            }

            String bg_image_filename;
            String fg_image_filename;
            String side_image_filename;

            if (desc.Exists("menu.background.bitmap.filename"))
            {
                bg_image_filename = backslash_to_slash(
                desc.GetStrValue("menu.background.bitmap.filename"));
            }
            else
            {
                bg_image_filename = "menu_background.png";
            }

            if (desc.Exists("menu.foreground.bitmap.filename"))
            {
                fg_image_filename = backslash_to_slash(
                desc.GetStrValue("menu.foreground.bitmap.filename"));
            }
            else
            {
                fg_image_filename = "menu_foreground.png";
            }

            if (desc.Exists("menu.sidebar.bitmap.filename"))
            {
                side_image_filename = backslash_to_slash(
                desc.GetStrValue("menu.sidebar.bitmap.filename"));
            }
            else
            {
                side_image_filename = "menu_sidebar.png";
            }

            File bgImgFile = new File(dir, bg_image_filename);
            File fgImgFile = new File(dir, fg_image_filename);

            File sideImgFile = new File(dir, side_image_filename);
            File sakuraSideImgFile = new File(dir, side_image_filename.Substring(0, side_image_filename.Length - 4) + "1.png");
            File keroSideImgFile = new File(dir, side_image_filename.Substring(0, side_image_filename.Length - 4) + "0.png");

            if (bgImgFile.Exists() && fgImgFile.Exists() &&
                (sideImgFile.Exists() || (sakuraSideImgFile.Exists() && keroSideImgFile.Exists())))
            {
                hasImage = true;

                bgImage = new NSImage(bgImgFile.GetPath(), true);
                fgImage = new NSImage(fgImgFile.GetPath(), true);
                if (sideImgFile.Exists())
                {
                    sakuraSideImage = keroSideImage = new NSImage(sideImgFile.GetPath(), true);
                    topLeftColorOfSakuraSideImage = topLeftColorOfKeroSideImage = SCImageUtils.GetTopLeftColor(sakuraSideImage);
                }
                else
                {
                    sakuraSideImage = new NSImage(sakuraSideImgFile.GetPath(), true);
                    keroSideImage = new NSImage(keroSideImgFile.GetPath(), true);

                    topLeftColorOfSakuraSideImage = SCImageUtils.GetTopLeftColor(sakuraSideImage);
                    topLeftColorOfKeroSideImage = SCImageUtils.GetTopLeftColor(keroSideImage);
                }

                topLeftColorOfBgImage = SCImageUtils.GetTopLeftColor(bgImage);
                topLeftColorOfFgImage = SCImageUtils.GetTopLeftColor(fgImage);
                bottomRightColorOfBgImage = SCImageUtils.GetBottomRightColor(bgImage);
                bottomRightColorOfFgImage = SCImageUtils.GetBottomRightColor(fgImage);
            }

            if (desc.Exists("menu.background.font.color.r"))
            {
                float r, g, b;
                r = desc.GetIntValue("menu.background.font.color.r") / 255.0f;
                g = desc.GetIntValue("menu.background.font.color.g") / 255.0f;
                b = desc.GetIntValue("menu.background.font.color.b") / 255.0f;
                bgFontColor = NSColor.ColorWithCalibratedRGB(r, g, b, 1.0f);
                disabledColor = NSColor.ColorWithCalibratedRGB(r, g, b, 0.5f);
            }
            else
            {
                bgFontColor = NSColor.BlackColor();
                disabledColor = NSColor.ColorWithCalibratedRGB(0.0f, 0.0f, 0.0f, 0.5f);
            }

            if (desc.Exists("menu.foreground.font.color.r"))
            {
                float r, g, b;
                r = desc.GetIntValue("menu.foreground.font.color.r") / 255.0f;
                g = desc.GetIntValue("menu.foreground.font.color.g") / 255.0f;
                b = desc.GetIntValue("menu.foreground.font.color.b") / 255.0f;
                fgFontColor = NSColor.ColorWithCalibratedRGB(r, g, b, 1.0f);
            }
            else
            {
                fgFontColor = NSColor.WhiteColor();
            }

            if (desc.Exists("menu.separator.color.r"))
            {
                float r, g, b;
                r = desc.GetIntValue("menu.separator.color.r") / 255.0f;
                g = desc.GetIntValue("menu.separator.color.g") / 255.0f;
                b = desc.GetIntValue("menu.separator.color.b") / 255.0f;
                separatorColor = NSColor.ColorWithCalibratedRGB(r, g, b, 1.0f);
            }
            else
            {
                separatorColor = NSColor.WhiteColor();
            }

            if (desc.Exists("menu.background.alignment"))
            {
                var id = desc.GetStrValue("menu.background.alignment");

                int i = -1;
                i =  ALIGN_TABLE.ContainsKey(id) ? (int)ALIGN_TABLE[id]  : -1;
                align_bg = (i == -1 ? ALIGN_RIGHTTOP : i);
            }
            else
            {
                align_bg = ALIGN_RIGHTTOP;
            }

            if (desc.Exists("menu.foreground.alignment"))
            {
                var id = desc.GetStrValue("menu.foreground.alignment");

                int i = -1;
                i = ALIGN_TABLE.ContainsKey(id) ? (int)ALIGN_TABLE[id] : -1;
                align_fg = (i == -1 ? ALIGN_RIGHTTOP : i);
            }
            else
            {
                align_fg = ALIGN_RIGHTTOP;
            }

            if (desc.Exists("menu.sidebar.alignment"))
            {
                var id = desc.GetStrValue("menu.sidebar.alignment");

                int i = -1;
                i = ALIGN_TABLE.ContainsKey(id) ? (int)ALIGN_TABLE[id] : -1;
                align_side = (i == -1 ? ALIGN_BOTTOM : i);
            }
            else
            {
                align_side = ALIGN_BOTTOM;
            }
        }

        public static String backslash_to_slash(String src)
        {
            if (src.IndexOf('\\') != -1)
            {
                StringBuffer buf = new StringBuffer(src);
                int pos;
                while ((pos = buf.ToString().IndexOf('\\')) != -1)
                {
                    buf.SetCharAt(pos, '/');
                }
                return buf.ToString();
            }
            else
            {
                return src;
            }
        }

        public bool HasImage()
        {
            return hasImage;
        }
        public NSImage GetBgImage()
        {
            return bgImage;
        }
        public NSImage GetFgImage() 
        { 
            return fgImage; 
        }
        public NSImage GetSakuraSideImage() 
        { 
            return sakuraSideImage;
        }
        public NSImage GetKeroSideImage() 
        { 
            return keroSideImage;
        }
        public NSColor GetBgFontColor() 
        { 
            return bgFontColor;
        }
        public NSColor GetFgFontColor()
        { 
            return fgFontColor;
        }
        public NSColor GetDisabledColor()
        { 
            return disabledColor; 
        }
        public NSColor GetSeparatorColor() 
        { 
            return separatorColor; 
        }
        public int GetAlignmentOfBg() 
        { 
            return align_bg;
        }
        public int GetAlignmentOfFg() 
        { 
            return align_fg;
        }
        public int GetAlignmentOfSide()
        { 
            return align_side;
        }
        public NSColor GetTopLeftColorOfSakuraSideImage() 
        { 
            return topLeftColorOfSakuraSideImage; 
        }
        public NSColor GetTopLeftColorOfKeroSideImage() 
        { 
            return topLeftColorOfKeroSideImage; 
        }
        public NSColor GetBottomRightColorOfSakuraSideImage() 
        { 
            return bottomRightColorOfSakuraSideImage;
        }
        public NSColor GetBottomRightColorOfKeroSideImage() 
        { 
            return bottomRightColorOfKeroSideImage;
        }
        public NSColor GetTopLeftColorOfBgImage() 
        { 
            return topLeftColorOfBgImage;
        }
        public NSColor GetTopLeftColorOfFgImage() 
        { 
            return topLeftColorOfFgImage;
        }
        public NSColor GetBottomRightColorOfBgImage() 
        { 
            return bottomRightColorOfBgImage; 
        }
        public NSColor GetBottomRightColorOfFgImage()
        { 
            return bottomRightColorOfFgImage;
        }


    }
}
