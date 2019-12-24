using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGySoft.Drawing.Imaging
{
    internal abstract class BitmapDataRowBase : IBitmapDataRow
    {
        internal abstract unsafe byte* Address { get; set; }
        internal IBitmapDataAccessor Accessor;
        internal int Line;

        public Color this[int x]
        {
            get => GetPixelColor32(x);
            set => SetPixelColor(x, value);
        }

        public Color32 GetPixelColor32(int x)
        {
            ValidateX(x);
            return DoGetColor32(x);
        }

        public Color64 GetPixelColor64(int x)
        {
            ValidateX(x);
            return DoGetColor64(x);
        }

        public int GetPixelColorIndex(int x)
        {
            ValidateX(x);
            return DoGetGetPixelColorIndex(x);
        }


        public Color32 SetPixelColor(int x, Color32 color)
        {
            ValidateX(x);
            return DoSetColor32(x, color);
        }

        public Color64 SetPixelColor(int x, Color64 color)
        {
            ValidateX(x);
            return DoSetColor64(x, color);
        }

        public void SetPixelColorIndex(int x, int colorIndex)
        {
            ValidateX(x);
            DoSetGetPixelColorIndex(x, colorIndex);
        }

        protected abstract Color32 DoGetColor32(int x);
        protected abstract Color32 DoSetColor32(int x, Color32 c);
        protected abstract Color64 DoGetColor64(int x);
        protected abstract Color64 DoSetColor64(int x, Color64 c);
        protected abstract int DoGetGetPixelColorIndex(int x);
        protected abstract void DoSetGetPixelColorIndex(int x, int colorIndex);

        public unsafe bool MoveNextRow()
        {
            if (Line == Accessor.Height - 1)
                return false;
            Line += 1;
            Address += Accessor.Stride;
            return true;
        }

        private void ValidateX(int x)
        {
            if ((uint)x > Accessor.Width)
                throw new ArgumentOutOfRangeException(nameof(x), PublicResources.ArgumentOutOfRange);
        }
    }
}
