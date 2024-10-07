using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAutoService
{
    internal class Helper
    {
        public static bool flag = false;
        public static int prioritet = 0;
        public static Entities ent;
        public static Entities GetContext()
        {
            if (ent == null)
            {
                ent = new Entities();
            }
            return ent;
        }
    }
}
