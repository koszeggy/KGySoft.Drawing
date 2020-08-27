using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGySoft.Drawing
{
    internal interface IAsyncContext
    {
        int MaxDegreeOfParallelism { get; }
        bool IsCancellationRequested { get; }
        IProgress<DrawingProgress> ProgressReporter { get; }
    }
}
