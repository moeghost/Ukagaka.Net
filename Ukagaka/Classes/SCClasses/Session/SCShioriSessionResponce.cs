using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
namespace Ukagaka
{
    public class SCShioriSessionResponce
    {
        String respheader;
        Hashtable resp;

        public SCShioriSessionResponce(String respheader, Hashtable resp)
        {
            this.respheader = respheader;
            this.resp = resp;
        }

        public String GetHeader()
        {
            return respheader;
        }

        public Hashtable GetResponce()
        {
            return resp;
        }

        public String GetRespForKey(String key)
        {
            // 見つからなければnullを返します。
            return (String)resp[key];
        }

    }
}
