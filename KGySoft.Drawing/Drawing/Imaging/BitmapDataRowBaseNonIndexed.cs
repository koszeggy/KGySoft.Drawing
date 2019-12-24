using System;

namespace KGySoft.Drawing.Imaging
{
    internal abstract class BitmapDataRowBaseNonIndexed : BitmapDataRowBase
    {
        protected override Color64 DoGetColor64(int x) => DoGetColor32(x);
        protected override Color64 DoSetColor64(int x, Color64 c) => DoSetColor32(x, c.ToColor32());
        protected override int DoGetGetPixelColorIndex(int i) => throw new InvalidOperationException(Res.ImagingInvalidOperationIndexedOnly);
        protected override void DoSetGetPixelColorIndex(int x, int colorIndex) => throw new InvalidOperationException(Res.ImagingInvalidOperationIndexedOnly);
    }
}