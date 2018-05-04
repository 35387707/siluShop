using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelexBarDLL
{
    public partial class RelexBarEntities
    {
        bool _isdispose = false;
        public bool IsDispose
        {
            get { return _isdispose; }
        }

        protected override void Dispose(bool disposing)
        {
            _isdispose = true;
            base.Dispose(disposing);
        }
    }
}
