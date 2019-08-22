using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wordman.Words
{
    [Flags]
    public enum ChangeMod
    {
        Del = 1,
        New = 2,
        Mod = Del | New
    }
}
