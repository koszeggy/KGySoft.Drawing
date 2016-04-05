using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KGySoft.Drawing
{
    /// <summary>
    /// Represents the rendering quality of a control.
    /// </summary>
    public enum RenderingQuality
    {
        /// <summary>
        /// Represents the default rendering quality.
        /// </summary>
        SystemDefault,

        /// <summary>
        /// Represents low quality but fast performance.
        /// </summary>
        Low,

        /// <summary>
        /// Represents balanced quality rendering with slower performance.
        /// </summary>
        Medium,

        /// <summary>
        /// Represents the best quality rendering with least performance.
        /// </summary>
        High
    }
}
