using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGySoft.Drawing
{
    public enum AsyncDrawingOperation : byte
    {
        Initializing,
        OptimizingPalette,
        ProcessingPixels
    }
}
