using System;
using KGySoft.CoreLibraries;

namespace KGySoft.Drawing
{
    public interface IAsyncDrawingResult : IAsyncResult
    {
        bool IsCanceled { get; }
        void RequestCancel();

        event EventHandler<EventArgs<DrawingProgress>> ProgressChanged;

        DrawingProgress Progress { get; }
    }
}
