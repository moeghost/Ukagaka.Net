using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Ukagaka.Classes.ToolKit
{
    // 正确的静态类定义（非泛型）
    public static class ColorConversionExtensions
    {
        // System.Drawing.Color 转 System.Windows.Media.Color
        public static  System.Windows.Media.Color ToMediaColor(this System.Drawing.Color drawingColor)
        {
            return System.Windows.Media.Color.FromArgb(
                drawingColor.A,
                drawingColor.R,
                drawingColor.G,
                drawingColor.B);
        }

        // System.Windows.Media.Color 转 System.Drawing.Color
        public static System.Drawing.Color ToDrawingColor(this System.Windows.Media.Color mediaColor)
        {
            return System.Drawing.Color.FromArgb(
                mediaColor.A,
                mediaColor.R,
                mediaColor.G,
                mediaColor.B);
        }
    }
}
