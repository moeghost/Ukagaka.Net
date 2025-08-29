using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{
    public class SCMakotoAccessor : IDisposable
    {
        private object _module; // MAKOTO module class instance
        private readonly MethodInfo _stringGetModuleName; // public string GetModuleName()
        private readonly MethodInfo _stringRequestString; // public string Request(string req)
        private readonly MethodInfo _voidTerminate; // public void Terminate()

        public SCMakotoAccessor(object module)
        {
            _module = module ?? throw new ArgumentNullException(nameof(module));

            Type modClass = module.GetType();
            _stringGetModuleName = modClass.GetMethod("GetModuleName", Type.EmptyTypes);
            _stringRequestString = modClass.GetMethod("Request", new[] { typeof(string) });

            try
            {
                _voidTerminate = modClass.GetMethod("Terminate", Type.EmptyTypes);
            }
            catch (MissingMethodException)
            {
                // Method is optional
                _voidTerminate = null;
            }
        }

        public string GetModuleName()
        {
            try
            {
                return (string)_stringGetModuleName.Invoke(_module, null);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return null;
            }
        }

        public string Request(string req)
        {
            try
            {
                return (string)_stringRequestString.Invoke(_module, new object[] { req });
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return null;
            }
        }

        public void Terminate()
        {
            if (_voidTerminate == null) return;

            try
            {
                _voidTerminate.Invoke(_module, null);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }

            Dispose();
        }

        public override string ToString()
        {
            return $"core: {_module}";
        }

        public void Dispose()
        {
            _module = null;
            // MethodInfos don't need to be disposed in C#
        }
    }
}