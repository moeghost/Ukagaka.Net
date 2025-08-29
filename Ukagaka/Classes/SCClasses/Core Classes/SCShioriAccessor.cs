using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using aya;

namespace Ukagaka
{
    public class SCShioriAccessor
    {
        Object module; // SHIORIモジュールクラス本体

        MethodInfo string_getModuleName; // public String GetModuleName()
        MethodInfo string_request_string; // public String Request(String req) -- 現時点ではrequestはこのタイプのみサポート。
        MethodInfo void_terminate; // public void Terminate()

        public SCShioriAccessor(Object module)
        {
            this.module = module;
            //     Activator.CreateInstance(System.Type.GetType("className")) as Class;

            if (module is DynamicShioriLoader shioriLoader)
            {
                // AYA5 直接调用，无需反射
                string_request_string = null;
                void_terminate = null;

            }


            else if (module is Aya5ShioriWrapper ayaWrapper)
            {
                // AYA5 直接调用，无需反射
                string_request_string = null;
                void_terminate = null;
            }
            else
            {
                Type mod_class = module.GetType();

                string_getModuleName = mod_class.GetMethod("GetModuleName");
                string_request_string = mod_class.GetMethod("Request");
                try
                {
                    void_terminate = mod_class.GetMethod("Terminate");
                }
                catch (MissingMethodException e)
                { // 省略されている
                    void_terminate = null;
                }
            }
        }

        public String GetModuleName()
        {
            try
            {
                return (String)string_getModuleName.Invoke(module, null);



            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        public String Request(String req)
        {




            if (module is DynamicShioriLoader shioriLoader)
            {
                return shioriLoader.Request(req);
            }


            if (module is Aya5ShioriWrapper ayaWrapper)
            {
                return ayaWrapper.Request(req);
            }
            

            try
            {
                return (String)string_request_string.Invoke(module, new Object[] { req });
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
                return null;
            }
             
        }

        public void Terminate()
        {
            if (module is Aya5ShioriWrapper ayaWrapper)
            {
                ayaWrapper.Terminate();
                return;
            }



            if (void_terminate == null) return;

            try
            {
                void_terminate.Invoke(module, null);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
            }

            module = null;
            string_getModuleName = null;
            string_request_string = null;
            void_terminate = null;
        }

        public String ToString()
        {
            return "core: " + module;
        }


    }
}
