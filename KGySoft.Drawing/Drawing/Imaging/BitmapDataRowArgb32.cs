using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace KGySoft.Drawing.Imaging
{
    internal sealed class BitmapDataRowArgb32 : BitmapDataRowBaseNonIndexed
    {
        private unsafe Color32* row;

        internal override unsafe byte* Address
        {
            get => (byte*)row;
            set => row = (Color32*)value;
        }

        protected override unsafe Color32 DoGetColor32(int x) => row[x];

        protected override unsafe Color32 DoSetColor32(int x, Color32 c)
        {
            row[x] = c;
            return c;
        }
    }
}
