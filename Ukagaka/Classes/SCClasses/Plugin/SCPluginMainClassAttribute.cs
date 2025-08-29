using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class SCPluginMainClassAttribute : Attribute
    {
        public string MainClassName { get; }

        public SCPluginMainClassAttribute(string mainClassName)
        {
            MainClassName = mainClassName;
        }
    }
}
