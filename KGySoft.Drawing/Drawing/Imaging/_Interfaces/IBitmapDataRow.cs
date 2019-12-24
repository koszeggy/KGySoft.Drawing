using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security;
using KGySoft.CoreLibraries;

namespace KGySoft.Drawing
{
    public interface IBitmapDataRow
    {
        Color this[int x] { get; set; }

        Color32 GetPixelColor32(int x);
        Color64 GetPixelColor64(int x);
        int GetPixelColorIndex(int x);

        Color32 SetPixelColor(int x, Color32 color);
        Color64 SetPixelColor(int x, Color64 color);
        void SetPixelColorIndex(int x, int colorIndex);

        bool MoveNextRow();
    }
}
