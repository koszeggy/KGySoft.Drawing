using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGySoft.Drawing
{
    public struct DrawingProgress
    {
        public AsyncDrawingOperation OperationType { get; }
        public byte Percentage { get; }
    }
}
