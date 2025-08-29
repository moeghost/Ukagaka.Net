using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class BufferReader : StreamReader
    {


        public BufferReader(Stream br, Encoding encoding):base(br,encoding)
       {
            
       }


    }
}
