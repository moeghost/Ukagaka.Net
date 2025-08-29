using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class ImageHelper
    {



        /// <summary>
        /// Bitmap转换为Icon
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Icon BitmapToIcon(Bitmap bitmap)
        {
            System.IntPtr iconHandle = bitmap.GetHicon();
            return Icon.FromHandle(iconHandle);
        }





    }
}
