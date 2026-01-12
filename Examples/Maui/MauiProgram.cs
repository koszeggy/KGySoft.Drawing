#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MauiProgram.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

using SkiaSharp.Views.Maui.Controls.Hosting;

#endregion

namespace KGySoft.Drawing.Examples.Maui
{
    public static class MauiProgram
    {
        #region Methods

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp() // this must be added manually to make rendering of SKBitmaps work in a MAUI app
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            return builder.Build();
        }

        #endregion
    }
}
