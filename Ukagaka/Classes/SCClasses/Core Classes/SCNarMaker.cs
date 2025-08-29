using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
namespace Ukagaka
{
    public class SCNarMaker
    {
        private File file;
        private SCSession target;

        public SCNarMaker(File file)
        {
            this.file = file;
        }

        public SCNarMaker(File file, SCSession target) : this(file)
        {
            this.target = target;
        }

        internal static bool HasManifestFile(File f)
        {
            throw new NotImplementedException();
        }

        internal bool IsAlive()
        {
            throw new NotImplementedException();
        }

        internal void Start()
        {
            throw new NotImplementedException();
        }

        internal void WaitUntilEnd()
        {
            throw new NotImplementedException();
        }
    }
}
