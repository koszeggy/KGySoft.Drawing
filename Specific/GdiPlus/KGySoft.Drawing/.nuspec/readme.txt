﻿Thank you for installing KGy SOFT Drawing Libraries 9.0.1
KGy SOFT Drawing Libraries offer advanced drawing features for System.Drawing types.

⚠️ Warning: Version 7.0.0 introduced several breaking changes. Most importantly, the technology-agnostic and
   platform independent APIs have been extracted into a separated package: KGySoft.Drawing.Core.
   Please find the KGySoft.Drawing.Core package at https://www.nuget.org/packages/KGySoft.Drawing.Core/.

Release Notes: https://github.com/koszeggy/KGySoft.Drawing/blob/master/Specific/GdiPlus/KGySoft.Drawing/changelog.txt

📝 Note: In .NET 7 and above this package can be used on Windows only. When targeting earlier versions, Unix/Linux
         based systems are also supported (if the libgdiplus library is installed).

                                                 ~~*~~

          +------------------------------------------------------------------------------------+
          | Example applications for MAUI, UWP, WinForms, WinUI, WPF and Xamarin:              |
          |   https://github.com/koszeggy/KGySoft.Drawing/tree/master/Examples                 |
          | Debugger Visualizers for System.Drawing types built on KGy SOFT Drawing Libraries: |
          |   https://github.com/koszeggy/KGySoft.Drawing.Tools                                |
          +------------------------------------------------------------------------------------+

                                                 ~~*~~

Project Home Page: https://kgysoft.net/drawing
Online documentation: https://docs.kgysoft.net/drawing
GitHub: https://github.com/koszeggy/KGySoft.Drawing

                                                 ~~*~~

Some Highlights of KGy SOFT Drawing Libraries:

- Fast direct Bitmap data access for every PixelFormat.
- High performance shape drawing even into Bitmap instances with indexed pixel formats.
- Quantizing and dithering.
- Several built-in icons as well as simple access to Windows associated and stock icons.
- Extracting bitmaps from multi-frame bitmaps and icons.
- Creating combined icons and multi-resolution bitmaps.
- Saving metafiles in EMF/WMF formats.
- Advanced support for saving images as Icon, BMP, JPEG, PNG, GIF and TIFF formats.
- Creating GIF animations even in high color.
- Converting between various pixel formats preserving transparency if possible.
- Useful extensions for the Icon, Bitmap, Image, Metafile and Graphics types.

💡 Tip:
- For technology-agnostic solutions that can be used on any platform see the KGySoft.Drawing.Core package.
  NuGet: https://www.nuget.org/packages/KGySoft.Drawing.Core
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