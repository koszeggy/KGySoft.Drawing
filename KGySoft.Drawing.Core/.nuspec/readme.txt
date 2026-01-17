Thank you for installing KGy SOFT Drawing Core Libraries 10.0.1
The KGy SOFT Drawing Core Libraries package offers advanced drawing features for completely managed bitmap data
on multiple platforms.

Release Notes: https://github.com/koszeggy/KGySoft.Drawing/blob/master/KGySoft.Drawing.Core/changelog.txt

                                                 ~~*~~

               +-----------------------------------------------------------------------+
               | Example applications for MAUI, UWP, WinForms, WinUI, WPF and Xamarin: |
               | https://github.com/koszeggy/KGySoft.Drawing/tree/master/Examples      |
               +-----------------------------------------------------------------------+

                                                 ~~*~~

Online Documentation: https://koszeggy.github.io/docs/drawing
GitHub: https://github.com/koszeggy/KGySoft.Drawing

                                                 ~~*~~

Some Highlights of KGy SOFT Drawing Core Libraries:

- Creating managed bitmap data of any pixel format on every platform.
- Creating bitmap data for any preallocated buffer using any pixel format. This allows accessing the pixels
  of bitmaps of any technology if the bitmap data is exposed as a pointer or array.
- Advanced and high-performance shape drawing with any pixel format.
- Supporting color correct alpha blending.
- Quantizing using predefined or optimized colors.
- Dithering using ordered, error diffusion, random noise or interleaved gradient noise dithering techniques.
- Creating GIF animations even in high color.

💡 Tip:
- For GDI+ specific solutions see the KGySoft.Drawing package. It makes possible to create managed, fast accessible
  bitmap data for the Bitmap class, supporting all pixel formats. It also contains many extensions for the Icon,
  Bitmap, Image, Metafile and Graphics types.
  NuGet: https://www.nuget.org/packages/KGySoft.Drawing
- For WPF specific solutions see the KGySoft.Drawing.Wpf package. It makes possible to create managed, fast
  accessible bitmap data for a WriteableBitmap instance of any pixel format.
  NuGet: https://www.nuget.org/packages/KGySoft.Drawing.Wpf
- For dedicated UWP support see the KGySoft.Drawing.Uwp package.
  NuGet: https://www.nuget.org/packages/KGySoft.Drawing.Uwp
- For dedicated WinUI support see the KGySoft.Drawing.WinUI package.
  NuGet: https://www.nuget.org/packages/KGySoft.Drawing.WinUI
- For SkiaSharp specific solutions see the KGySoft.Drawing.SkiaSharp package. It makes possible to create managed,
  fast accessible bitmap data for SKBitmap and SKPixmap instances of any pixel format.
  NuGet: https://www.nuget.org/packages/KGySoft.Drawing.SkiaSharp

💡 Pro Tip:
When debugging with Visual Studio, use the debugger visualizers for your IReadableBitmapData, Palette,
Color32, PColor32, Color64, PColor64, ColorF and PColorF instances. It contains debugger visualizers also for the
various bitmap, palette and color types of GDI+, WPF and SkiaSharp.
Marketplace: https://marketplace.visualstudio.com/items?itemName=KGySoft.drawing-debugger-visualizers-x64
GitHub: https://github.com/koszeggy/KGySoft.Drawing.Tools
